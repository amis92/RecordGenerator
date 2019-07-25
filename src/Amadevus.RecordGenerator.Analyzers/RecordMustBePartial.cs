using System.Collections.Immutable;
using System.Linq;
using Amadevus.RecordGenerator.DiagnosticDescriptors;
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
            context.RegisterSyntaxNodeAction(AnalyzeOuterType, SyntaxKind.StructDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            if (classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return;
            }
            // if no record attributes, stop diagnostic
            if (!HasAnyRecordAttributes(classDeclaration))
            {
                return;
            }
            context.ReportDiagnostic(CreateDiagnostic(classDeclaration));
        }

        private static void AnalyzeOuterType(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                return;
            }
            // if no record types, stop diagnostic
            if (!typeDeclaration.DescendantNodes().OfType<ClassDeclarationSyntax>().Any(HasAnyRecordAttributes))
            {
                return;
            }
            context.ReportDiagnostic(CreateDiagnostic(typeDeclaration));
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
