using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amadevus.RecordGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RecordGeneratorAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string Category = "ResourceGenerator";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    MissingRecordAttributeDeclarationDiagnostic.Descriptor,
                    MissingRecordPartialDiagnostic.Descriptor);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
            //context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classSyntax = context.Node as ClassDeclarationSyntax;
            var classIdSyntax = classSyntax.Identifier.ValueText;

            var attributes = classSyntax.AttributeLists
                .SelectMany(attListSyntax => attListSyntax.Attributes.Where(att => att.IsRecordAttributeSyntax()))
                .ToList();

            if (!attributes.Any())
            {
                return;
            }

            var anyRecordAttribute = false;
            foreach (var attribute in attributes)
            {
                var name = attribute.GetUnqualifiedName();
                if (!name.IsRecordAttributeName())
                {
                    continue;
                }
                anyRecordAttribute = true;
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol == null)
                {
                    var missingAttributeDiagnostic =
                        MissingRecordAttributeDeclarationDiagnostic.Create(
                            attribute.GetLocation(),
                            classSyntax.Identifier.ValueText,
                            name);
                    context.ReportDiagnostic(missingAttributeDiagnostic);
                }
            }

            if (!anyRecordAttribute)
            {
                return;
            }

            // check if there already is record partial
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classSyntax);
            if (classSymbol == null)
            {
                return;
            }


            // this part will be unnecessary once source generators are a thing
            // for now we must check if the "would-be-generated" partial exists
            // if not - report diagnostic, if does - check if it's still correct
            // (and report diagnostic if it's not)

            var syntaxRefs = classSymbol.DeclaringSyntaxReferences;
            if (syntaxRefs.Length == 1)
            {
                // single part means there sure is no generated partial
                var diagnostic =
                    MissingRecordPartialDiagnostic.Create(
                        classSyntax.Identifier.GetLocation(),
                        classSyntax.Identifier.ValueText);

                context.ReportDiagnostic(diagnostic);
                return;
            }

            // get all partial declarations (except the original one)
            var declarations =
                syntaxRefs
                .Select(@ref => @ref.GetSyntax() as ClassDeclarationSyntax)
                .Where(syntax => syntax != null && syntax != classSyntax)
                .ToList();

            // find the one with appropriate header
            var recordPartial = declarations.FirstOrDefault(IsPartOfRecordPartialFile);
            if (recordPartial == null)
            {
                // report diagnostic "record partial missing"
            }
            // check if partial is up-to-date else report "record partial 
        }

        private static bool IsPartOfRecordPartialFile(ClassDeclarationSyntax declaration)
        {
            var firstSingleLineComment = declaration.SyntaxTree
                                .GetRoot()
                                .DescendantTrivia()
                                .FirstOrDefault(trivia => trivia.Kind() == SyntaxKind.SingleLineCommentTrivia);
            return firstSingleLineComment.Token.ValueText == RecordPartialFileHeader;
        }

        const string RecordPartialFileHeader = "// Record partial generated by RecordGenerator";

        internal static class MissingRecordAttributeDeclarationDiagnostic
        {
            public const string DiagnosticId = "RG0001";
            private static readonly string Title = "Missing RecordAttribute declaration";
            private static readonly string MessageFormat = "Type {0} has [{1}] attribute but no such attribute is defined.";
            private static readonly string Description = "No RecordAttribute id defined.";

            public static DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

            public static Diagnostic Create(Location location, params object[] messageArgs)
            {
                return Diagnostic.Create(Descriptor, location, messageArgs);
            }
        }
        internal static class MissingRecordPartialDiagnostic
        {
            public const string DiagnosticId = "RG0002";
            private static readonly string Title = "Missing record partial";
            private static readonly string MessageFormat = "Type {0} is marked as [Record] but no appropriate partial was found (with constructor and/or mutators).";
            private static readonly string Description = "Record type doesn't have neither primary constructor nor mutators (WithProperty methods) defined.";

            public static DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

            public static Diagnostic Create(Location location, params object[] messageArgs)
            {
                return Diagnostic.Create(Descriptor, location, messageArgs);
            }
        }
    }

    internal static class RecordAttributeExtensions
    {
        public static bool IsRecordAttributeSyntax(this AttributeSyntax attSyntax)
        {
            return attSyntax.GetUnqualifiedName()?.IsRecordAttributeName() ?? false;
        }

        public static bool IsRecordAttributeName(this string text)
        {
            return !string.IsNullOrWhiteSpace(text) && (text == "Record" || text == "RecordAttribute");
        }

        public static string GetUnqualifiedName(this AttributeSyntax attSyntax)
        {
            var identifierNameSyntax =
                attSyntax.Name
                .DescendantNodesAndSelf()
                .LastOrDefault(node => node is IdentifierNameSyntax) as IdentifierNameSyntax;
            return identifierNameSyntax?.Identifier.ValueText;
        }
    }

    [Record]
    public partial class X { }

    partial class X { }

    [Record]
    public partial struct Y { }

    public partial struct Y { }
}