using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.IO;
using System.Threading;

namespace Amadevus.RecordGenerator.Analyzers
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RecordAttributeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(RecordAttributeDeclarationMissingDiagnostic.Descriptor);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = context.Node as ClassDeclarationSyntax;
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
        }
    }
}