using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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
                    MissingRecordAttributeDeclarationDiagnostic.Descriptor,
                    MissingRecordPartialDiagnostic.Descriptor,
                    InvalidGeneratedRecordPartialDiagnostic.Descriptor);
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
                        MissingRecordAttributeDeclarationDiagnostic.Create(
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
            var recordPartial = GetGeneratedPartial(typeDeclaration, typeSymbol);
            if (recordPartial == null)
            {
                // no generated partial found
                var missingPartialDiagnostic =
                    MissingRecordPartialDiagnostic.Create(
                        typeDeclaration.Identifier.GetLocation(),
                        typeDeclaration.Identifier.ValueText);

                context.ReportDiagnostic(missingPartialDiagnostic);
                return;
            }

            // check partial is equivalent to would-be generated partial
            var wouldBePartialRoot = RecordPartialGenerator.GenerateRecordPartialRoot(typeDeclaration, context.CancellationToken);
            var currentPartialRoot = recordPartial.SyntaxTree.GetRoot(context.CancellationToken);

            /* TODO create CSharpSyntaxWalker which will compare just tokens (not trivia)
             * and also return if the only difference is GeneratedCodeAttribute version
             */

            if (wouldBePartialRoot.IsEquivalentTo(currentPartialRoot, topLevel:false))
            {
                // no generation necessary
                return;
            }
            // report error "record partial requires update"
            var invalidPartialDiagnostic =
                InvalidGeneratedRecordPartialDiagnostic.Create(
                    typeDeclaration.Identifier.GetLocation(),
                    typeDeclaration.Identifier.ValueText);

            context.ReportDiagnostic(invalidPartialDiagnostic);
        }

        private static TypeDeclarationSyntax GetGeneratedPartial(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
        {
            var syntaxRefs = typeSymbol.DeclaringSyntaxReferences;
            if (syntaxRefs.Length == 1)
            {
                return null;
            }
            // get all partial declarations (except the original one)
            var declarations =
                syntaxRefs
                .Select(@ref => @ref.GetSyntax() as TypeDeclarationSyntax)
                .Where(syntax => syntax != null && syntax != typeDeclaration)
                .ToList();

            // find the one with appropriate header
            var recordPartial = declarations.FirstOrDefault(d => d.IsFileHeaderPresent());
            return recordPartial;
        }
    }
}