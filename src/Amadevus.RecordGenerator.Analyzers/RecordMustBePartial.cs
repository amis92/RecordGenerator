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
            context.RegisterSyntaxNodeAction(
                AnalyzeType,
                SyntaxKind.StructDeclaration,
                SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                // if partial, nothing to do
                return;
            }
            if (HasAnyRecordAttributes(typeDeclaration))
            {
                context.ReportDiagnostic(CreateDiagnostic(typeDeclaration));
                return;
            }
            // if the type is not a record, check inner types
            if (typeDeclaration.DescendantNodes().OfType<TypeDeclarationSyntax>().Any(HasAnyRecordAttributes))
            {
                context.ReportDiagnostic(CreateDiagnostic(typeDeclaration));
            }
        }

        private static Diagnostic CreateDiagnostic(TypeDeclarationSyntax typeSyntax)
        {
            return Diagnostic.Create(
                Descriptors.X1000_RecordMustBePartial,
                typeSyntax.Identifier.GetLocation(),
                typeSyntax.Identifier.ValueText);
        }

        private static bool HasAnyRecordAttributes(TypeDeclarationSyntax typeDeclaration)
        {
            return
                typeDeclaration.AttributeLists
                .SelectMany(list => list.Attributes.Where(att => att.IsRecordAttributeSyntax()))
                .Any();
        }
    }
}
