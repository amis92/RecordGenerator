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
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol == null)
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
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            if (classSymbol == null)
            {
                return;
            }

            // ### NOTE ###
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
                        typeDeclaration.Identifier.GetLocation(),
                        typeDeclaration.Identifier.ValueText);

                context.ReportDiagnostic(diagnostic);
                return;
            }

            // get all partial declarations (except the original one)
            var declarations =
                syntaxRefs
                .Select(@ref => @ref.GetSyntax() as ClassDeclarationSyntax)
                .Where(syntax => syntax != null && syntax != typeDeclaration)
                .ToList();

            // find the one with appropriate header
            var recordPartial = declarations.FirstOrDefault(d => d.IsPartOfRecordPartialFile());
            if (recordPartial == null)
            {
                // TODO report diagnostic "record partial missing"
            }
            // TODO check if partial is up-to-date else report "record partial not up-to-date"
        }
    }
}