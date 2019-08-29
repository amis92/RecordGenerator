using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amadevus.RecordGenerator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RecordMustBeSealedIfEqualityIsEnabled : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled);

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(
                AnalyzeType,
                SyntaxKind.ClassDeclaration);
            // only classes can be sealed, so analyze only them
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (typeDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword))
            {
                // quit if sealed
                return;
            }
            if (typeDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                // quit if abstract
                return;
            }
            if (!HasAnyRecordAttributesWithEqualityFeature(typeDeclaration, context.SemanticModel))
            {
                // quit if no record attributes with arguments that have Equality features
                return;
            }
            context.ReportDiagnostic(CreateDiagnostic(typeDeclaration));
        }

        private static Diagnostic CreateDiagnostic(TypeDeclarationSyntax typeSyntax)
        {
            return Diagnostic.Create(
                Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled,
                typeSyntax.Identifier.GetLocation(),
                typeSyntax.Identifier.ValueText);
        }

        private static bool HasAnyRecordAttributesWithEqualityFeature(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            var candidate =
                typeDeclaration.AttributeLists
                .SelectMany(list => list.Attributes.Where(att => att.IsRecordAttributeSyntax()))
                .FirstOrDefault();
            if (candidate == null)
            {
                // no [Record] attribute
                return false;
            }
            var features = candidate.ArgumentList?.Arguments.Count > 0 ? GetDeclaredFeatures() : Features.Default;
            return (features & Features.Equality) != 0;
            Features GetDeclaredFeatures()
            {
                try
                {
                    var typeModel = semanticModel.GetDeclaredSymbol(typeDeclaration);
                    var recordAttributeData =
                        typeModel.GetAttributes().First(x => x.IsRecordAttribute());
                    return (Features)recordAttributeData.ConstructorArguments[0].Value;
                }
                catch
                {
                    return Features.None;
                }
            }
        }
    }
}
