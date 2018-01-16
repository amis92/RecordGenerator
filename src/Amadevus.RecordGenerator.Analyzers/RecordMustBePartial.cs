using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amadevus.RecordGenerator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RecordMustBePartial : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptors.X1000_RecordMustBePartial);

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax classDeclaration ))
            {
                return;
            }
            var recordNamedAttributes =
                classDeclaration.AttributeLists
                .SelectMany(attListSyntax => attListSyntax.Attributes.Where(att => att.IsRecordAttributeSyntax()))
                .ToImmutableArray();
            // if no record attributes, stop diagnostic
            if (recordNamedAttributes.Length == 0)
            {
                return;
            }
            foreach (var typeSyntax in classDeclaration.AncestorsAndSelf().OfType<TypeDeclarationSyntax>())
            {
                if (!typeSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
                    && (typeSyntax is ClassDeclarationSyntax || typeSyntax is StructDeclarationSyntax))
                {
                    Report(typeSyntax);
                }
            }
            void Report(TypeDeclarationSyntax typeSyntax)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.X1000_RecordMustBePartial,
                        typeSyntax.Identifier.GetLocation(),
                        typeSyntax.Identifier.ValueText));
            }
        }
    }
}
