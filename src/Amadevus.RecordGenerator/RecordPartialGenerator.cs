using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Amadevus.RecordGenerator
{
    internal abstract class RecordPartialGenerator
    {
        protected RecordPartialGenerator(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            Document = document;
            TypeDeclaration = declaration;
            CancellationToken = cancellationToken;
            TypeSyntaxLazy = new Lazy<TypeSyntax>(GetTypeSyntax);
            RecordPropertiesLazy = new Lazy<ImmutableArray<RecordEntry>>(GetRecordProperties);
        }

        protected Document Document { get; }

        protected TypeDeclarationSyntax TypeDeclaration { get; }

        protected CancellationToken CancellationToken { get; }

        protected IReadOnlyList<RecordEntry> RecordProperties => RecordPropertiesLazy.Value;

        protected TypeSyntax RecordTypeSyntax => TypeSyntaxLazy.Value;

        private Lazy<ImmutableArray<RecordEntry>> RecordPropertiesLazy { get; }

        private Lazy<TypeSyntax> TypeSyntaxLazy { get; }

        public static RecordPartialGenerator Create(Document document, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            if (declaration is ClassDeclarationSyntax classDeclaration)
            {
                return new ClassRecordPartialGenerator(document, classDeclaration, cancellationToken);
            }
            if (declaration is StructDeclarationSyntax structDeclaration)
            {
                return new StructRecordPartialGenerator(document, structDeclaration, cancellationToken);
            }
            return null;
        }

        public abstract Task<Document> GenerateRecordPartialAsync();

        protected abstract string TypeName();

        protected async Task<Document> GenerateDocumentAsync()
        {
            var typeDeclaration = await TypeDeclarationAsync().ConfigureAwait(false);

            var rootMemberDeclaration = RootMemberDeclaration(typeDeclaration);

            var compilationUnit = await CompilationUnitAsync(rootMemberDeclaration).ConfigureAwait(false);

            var document = DocumentAsync(compilationUnit);

            return document;
        }

        protected Document DocumentAsync(CompilationUnitSyntax compilationUnit)
        {
            var typeName = TypeName();
            var recordPartialRoot = Formatter.Format(compilationUnit, Document.Project.Solution.Workspace, cancellationToken: CancellationToken);
            var recordPartialDocument = Document.Project.AddDocument($"{typeName}.Record.cs", recordPartialRoot, Document.Folders);
            return recordPartialDocument;
        }

        protected async Task<CompilationUnitSyntax> CompilationUnitAsync(MemberDeclarationSyntax rootMemberDeclaration)
        {
            var syntaxTree = await Document.GetSyntaxTreeAsync(CancellationToken).ConfigureAwait(false);
            var recordPartialCompilationUnit = SyntaxFactory.CompilationUnit()
                .WithUsings(syntaxTree.GetCompilationUnitRoot(CancellationToken).Usings)
                .WithMembers(SyntaxFactory.SingletonList(rootMemberDeclaration));

            return recordPartialCompilationUnit;
        }

        protected MemberDeclarationSyntax RootMemberDeclaration(TypeDeclarationSyntax newTypeDeclaration)
        {
            var newRootMemberDeclaration =
                TypeDeclaration
                .Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .Aggregate(newTypeDeclaration as MemberDeclarationSyntax, (prev, curr) =>
                {
                    return curr.WithMembers(SyntaxFactory.SingletonList(prev));
                });
            return newRootMemberDeclaration;
        }

        protected abstract Task<TypeDeclarationSyntax> TypeDeclarationAsync();
        
        protected SyntaxList<MemberDeclarationSyntax> GenerateMembers(SyntaxToken identifier, IReadOnlyList<RecordEntry> properties)
        {
            var ctor = SyntaxFactory.ConstructorDeclaration(identifier.ValueText)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(properties.IntoCtorParameterList())
                .WithBody(properties.IntoCtorBody());

            return SyntaxFactory.SingletonList<MemberDeclarationSyntax>(ctor)
                .AddRange(RecordProperties.Select(p => Mutator(p)));
        }

        protected MethodDeclarationSyntax Mutator(RecordEntry entry)
        {
            var arguments = RecordProperties.Select(x =>
            {
                return SyntaxFactory.Argument(
                    SyntaxFactory.IdentifierName(x.Identifier));
            });

            var mutator =
                SyntaxFactory.MethodDeclaration(RecordTypeSyntax, MutatorIdentifier(entry))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                entry.Identifier)
                                .WithType(entry.Type))))
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.ObjectCreationExpression(RecordTypeSyntax)
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(arguments))))));
            return mutator;
        }

        protected SyntaxToken MutatorIdentifier(RecordEntry entry)
        {
            return SyntaxFactory.Identifier($"With{entry.Identifier.ValueText}");
        }

        protected AttributeListSyntax GeneratedCodeAttribute()
        {
            return
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName("System.CodeDom.Compiler.GeneratedCode"))
                        .WithArgumentList(
                            SyntaxFactory.ParseAttributeArgumentList($"(\"{nameof(RecordGenerator)}\", \"{RecordGeneratorProperties.VersionString}\")"))));
        }

        private TypeSyntax GetTypeSyntax()
        {
            var typeParamList = TypeDeclaration.TypeParameterList;
            if (typeParamList == null)
            {
                return SyntaxFactory.IdentifierName(TypeDeclaration.Identifier);
            }

            var arguments = typeParamList.Parameters.Select(param => SyntaxFactory.IdentifierName(param.Identifier));
            var typeArgList =
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList<TypeSyntax>(
                        arguments));

            return SyntaxFactory.GenericName(TypeDeclaration.Identifier, typeArgList);
        }

        protected ImmutableArray<RecordEntry> GetRecordProperties()
        {
            return TypeDeclaration.Members.GetRecordProperties().AsRecordEntries();
        }
    }
}
