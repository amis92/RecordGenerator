using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using System.Reflection;

namespace Amadevus.RecordGenerator
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingRecordPartialCodeFixProvider)), Shared]
    public sealed class MissingRecordPartialCodeFixProvider : CodeFixProvider
    {
        private const string title = "Generate RecordAttribute declaration";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(MissingRecordPartialDiagnostic.DiagnosticId);
            }
        }
        
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {

                var diagnosticSpan = diagnostic.Location.SourceSpan;
                // Find the type declaration identified by the diagnostic.
                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();


                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution: c => CreateRecordPartialDocumentAsync(context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private async Task<Solution> CreateRecordPartialDocumentAsync(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
        {
            (document, declaration) = await FixPartialModifier(document, declaration, c).ConfigureAwait(false);

            var syntaxTree = await document.GetSyntaxTreeAsync(c).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(c).ConfigureAwait(false);
            var typeSymbol = semanticModel.GetDeclaredSymbol(declaration, c);
            var typeName = typeSymbol.Name;

            //var propertySymbols = typeSymbol.GetMembers().OfType<IPropertySymbol>().ToImmutableArray();
            
            var newTypeDeclaration = await CreateRecordPartialDeclarationAsync(document, declaration, c);

            var newRootMemberDeclaration =
                declaration
                .Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .Aggregate(newTypeDeclaration as MemberDeclarationSyntax, (prev, curr) =>
                {
                    return curr.WithMembers(SyntaxFactory.SingletonList(prev));
                });



            var recordPartialCompilationUnit = SyntaxFactory.CompilationUnit()
                .WithUsings(syntaxTree.GetCompilationUnitRoot(c).Usings)
                .WithMembers(SyntaxFactory.SingletonList(newRootMemberDeclaration));

            var recordPartialRoot = Formatter.Format(recordPartialCompilationUnit, document.Project.Solution.Workspace, cancellationToken: c);
            var recordPartialDocument = document.Project.AddDocument($"{typeName}.Record.cs", recordPartialRoot, document.Folders);


            return recordPartialDocument.Project.Solution;
        }

        private async Task<(Document document, TypeDeclarationSyntax declaration)> FixPartialModifier(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(c).ConfigureAwait(false);

            // add 'partial' to original declaration, if missing
            if (declaration.Modifiers.All(m => m.Kind() != SyntaxKind.PartialKeyword))
            {
                var newDeclaration = declaration.WithPartialModifier();
                var newDocument = document.WithSyntaxRoot(syntaxRoot.ReplaceNode(declaration, newDeclaration));
                return (newDocument, newDeclaration);
            }
            return (document, declaration);
        }

        private async Task<TypeDeclarationSyntax> CreateRecordPartialDeclarationAsync(Document document, TypeDeclarationSyntax declaration, CancellationToken c)
        {
            if (declaration is ClassDeclarationSyntax classSyntax)
            {
                return await CreateRecordClassPartialAsync(document, classSyntax, c);
            }
            if (declaration is StructDeclarationSyntax structSyntax)
            {
                return await CreateRecordStructPartialAsync(document, structSyntax, c);
            }
            // should never happen
            return declaration;
        }

        private Task<StructDeclarationSyntax> CreateRecordStructPartialAsync(Document document, StructDeclarationSyntax declaration, CancellationToken c)
        {
            var newDeclaration = declaration
                .WithAttributeLists(
                    SyntaxFactory.List(new[] {
                        GeneratedCodeAttribute()
                    }))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(RecordPartialMembers(declaration.Identifier, declaration.Members.GetRecordProperties().AsRecordEntries()));
            return Task.FromResult(newDeclaration);
        }

        private Task<ClassDeclarationSyntax> CreateRecordClassPartialAsync(Document document, ClassDeclarationSyntax declaration, CancellationToken c)
        {
            var newDeclaration = declaration
                .WithAttributeLists(
                    SyntaxFactory.List(new [] {
                        GeneratedCodeAttribute()
                    }))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(RecordPartialMembers(declaration.Identifier, declaration.Members.GetRecordProperties().AsRecordEntries()));
            return Task.FromResult(newDeclaration);
        }

        private SyntaxList<MemberDeclarationSyntax> RecordPartialMembers(SyntaxToken identifier, IReadOnlyList<RecordEntry> properties)
        {
            var ctor = SyntaxFactory.ConstructorDeclaration(identifier.ValueText)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(properties.IntoCtorParameterList())
                .WithBody(properties.IntoCtorBody());

            return SyntaxFactory.List(new MemberDeclarationSyntax[]
            {
                ctor
            });
        }

        private AttributeListSyntax GeneratedCodeAttribute()
        {
            return
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName("System.CodeDom.Compiler.GeneratedCode"))
                        .WithArgumentList(
                            SyntaxFactory.ParseAttributeArgumentList($"(\"{nameof(RecordGenerator)}\", \"{RecordGeneratorProperties.VersionString}\")"))));
        }
    }


    /// <summary>
    /// For testing purposes if creating With mutator with all properties wrapped like that may make sense.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct RecordDelta<T>
    {
        private readonly bool _isNotDefault;
        private readonly T _value;

        public RecordDelta(T value)
        {
            _isNotDefault = true;
            _value = value;
        }

        public bool IsDefault => !_isNotDefault;

        public T Value => IsDefault ? throw new InvalidOperationException($"{nameof(Value)} was not set") : _value;

        public static implicit operator RecordDelta<T>(T value)
        {
            return new RecordDelta<T>(value);
        }
    }

    internal static class SyntaxExtensions
    {

        public static TypeDeclarationSyntax WithPartialModifier(this TypeDeclarationSyntax declaration)
        {
            if (declaration is ClassDeclarationSyntax classDeclaration)
            {
                return classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            }
            if (declaration is StructDeclarationSyntax structDeclaration)
            {
                return structDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            }
            return declaration;
        }

        public static bool IsRecordViable(this PropertyDeclarationSyntax pdSyntax)
        {
            return pdSyntax.AccessorList?.Accessors.All(x => x.Kind() != SyntaxKind.SetAccessorDeclaration && x.Body == null) ?? false
                && pdSyntax.Modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword)
                && pdSyntax.Modifiers.All(x => x.Kind() != SyntaxKind.StaticKeyword);
        }

        public static ImmutableArray<PropertyDeclarationSyntax> GetRecordProperties(this SyntaxList<MemberDeclarationSyntax> members)
        {
            return members.OfType<PropertyDeclarationSyntax>()
                .Where(propSyntax => propSyntax.IsRecordViable())
                .ToImmutableArray();
        }

        public static ImmutableArray<RecordEntry> AsRecordEntries(this IEnumerable<PropertyDeclarationSyntax> properties)
        {
            return properties.Select(p => new RecordEntry(p.Identifier, p.Type)).ToImmutableArray();
        }

        public static BlockSyntax IntoCtorBody(this IReadOnlyList<RecordEntry> properties)
        {
            var block = SyntaxFactory.Block(properties.IntoCtorAssignments());
            return block;
        }

        private static IEnumerable<StatementSyntax> IntoCtorAssignments(this IReadOnlyList<RecordEntry> properties)
        {
            return properties.Select(p => p.IntoCtorAssignment());
        }

        private static StatementSyntax IntoCtorAssignment(this RecordEntry p)
        {
            var assignment =
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxFactory.IdentifierName(p.Identifier)),
                        SyntaxFactory.IdentifierName(p.Identifier)));
            return assignment;
        }

        public static ParameterListSyntax IntoCtorParameterList(this IReadOnlyList<RecordEntry> properties)
        {
            var list =
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList(properties.Select(p => p.IntoParameter())));
            return list;
        }


        public static ParameterSyntax IntoParameter(this RecordEntry property)
        {
            var parameter = 
                SyntaxFactory.Parameter(property.Identifier)
                .WithType(property.Type);
            return parameter;
        }
    }

    [Record]
    internal class RecordEntry
    {
        public RecordEntry(SyntaxToken identifier, TypeSyntax type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        public SyntaxToken Identifier { get; }

        public TypeSyntax Type { get; }
    }
}
