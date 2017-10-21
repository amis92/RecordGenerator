using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.IO;
using System.Threading;

namespace Amadevus.RecordGenerator
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RecordGeneratorAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    RecordAttributeDeclarationMissingDiagnostic.Descriptor,
                    RecordPartialMissingDiagnostic.Descriptor,
                    RecordPartialInvalidDiagnostic.Descriptor,
                    GeneratorVersionDifferentDiagnostic.Descriptor);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = context.Node as TypeDeclarationSyntax;
            var typeIdentifierText = typeDeclaration.Identifier.ValueText;

            var recordNamedAttributes =
                typeDeclaration.AttributeLists
                .SelectMany(attListSyntax => attListSyntax.Attributes.Where(att => att.IsRecordAttributeSyntax()))
                .ToImmutableArray();

            // if no record attributes, stop diagnostic
            if (recordNamedAttributes.Length == 0)
            {
                return;
            }

            // check if RecordAttribute is already declared
            foreach (var attribute in recordNamedAttributes)
            {
                var attributeSymbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (attributeSymbolInfo.Symbol == null)
                {
                    var diagnostic =
                        RecordAttributeDeclarationMissingDiagnostic.Create(
                            attribute.GetLocation(),
                            typeIdentifierText,
                            attribute.GetUnqualifiedName());
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // check if there already is record partial
            var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            if (typeSymbol == null)
            {
                return;
            }

            // ### NOTE ###
            // this part will be unnecessary once source generators are a thing
            // for now we must check if the "would-be-generated" partial exists
            // if not - report diagnostic, if does - check if it's still correct
            // (and report diagnostic if it's not)
            AnalyzeIfGenerationRequired(context, typeDeclaration, typeSymbol);
        }

        private static void AnalyzeIfGenerationRequired(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
        {
            var recordPartial = RecordPartialGenerator.GetGeneratedPartial(typeDeclaration, typeSymbol);
            var currentPartialFilename = Path.GetFileName(recordPartial.SyntaxTree.FilePath);
            if (recordPartial == null || RecordPartialGenerator.GetGeneratedFilename(typeDeclaration, CancellationToken.None) != currentPartialFilename)
            {
                // no generated partial found
                var missingPartialDiagnostic =
                    RecordPartialMissingDiagnostic.Create(
                        typeDeclaration.Identifier.GetLocation(),
                        typeDeclaration.Identifier.ValueText);

                context.ReportDiagnostic(missingPartialDiagnostic);
                return;
            }

            // check partial is equivalent to would-be generated partial
            var wouldBePartialRoot = RecordPartialGenerator.GenerateRecordPartialRoot(typeDeclaration, context.CancellationToken);
            var currentPartialRoot = recordPartial.SyntaxTree.GetRoot(context.CancellationToken);
            
            var equivalenceResult = wouldBePartialRoot.IsTokenwiseEquivalentTo(currentPartialRoot);
            switch (equivalenceResult)
            {
                case RecordPartialComparer.Result.Equivalent:
                    return;
                case RecordPartialComparer.Result.EquivalentExceptGeneratedCodeAttributeVersion:
                    {
                        string version = recordPartial.ExtractGeneratedCodeAttributeVersionArgument();
                        // report info "record partial might need update"
                        var differentToolVersionDiagnostic =
                            GeneratorVersionDifferentDiagnostic.Create(
                                typeDeclaration.Identifier.GetLocation(),
                                typeDeclaration.Identifier.ValueText,
                                version,
                                Properties.VersionString);

                        context.ReportDiagnostic(differentToolVersionDiagnostic);
                    }
                    return;
                case RecordPartialComparer.Result.NotEquivalent:
                    {
                        // report error "record partial requires update"
                        var invalidPartialDiagnostic =
                            RecordPartialInvalidDiagnostic.Create(
                                typeDeclaration.Identifier.GetLocation(),
                                typeDeclaration.Identifier.ValueText);

                        context.ReportDiagnostic(invalidPartialDiagnostic);
                    }
                    return;
                default:
                    break;
            }
        }
    }
}