using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Amadevus.RecordGenerator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RecordEntriesMustDifferCaseInsensitive : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptors.X1002_RecordEntriesMustDifferCaseInsensitive);

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
            if (!HasAnyRecordAttributes(typeDeclaration))
            {
                // not a record, ignore
                return;
            }
            var typeModel = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            // get record properties: public, instance, auto-readonly
            var members = typeModel.GetMembers();
            var membersByName = members
                .Where(IsRecordViableMember)
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1)
                .ToImmutableDictionary(x => x.Key);
            if (membersByName.Count == 0)
            {
                // all member names are case-insensitive different
                return;
            }
            foreach (var nameGroup in membersByName)
            {
                foreach (var entry in nameGroup.Value)
                {
                    var otherMember = nameGroup.Value.First(x => x != entry);
                    context.ReportDiagnostic(CreateDiagnostic(entry, otherMember.Name));
                }
            }
            bool IsRecordViableMember(ISymbol symbol)
            {
                return symbol.DeclaredAccessibility == Accessibility.Public
                    && !symbol.IsStatic
                    && symbol.Kind == SymbolKind.Property
                    && symbol is IPropertySymbol propertySymbol
                    && propertySymbol.IsReadOnly && !propertySymbol.IsIndexer
                    && symbol.DeclaringSyntaxReferences[0].GetSyntax() is PropertyDeclarationSyntax propDeclaration
                    && propDeclaration.ExpressionBody == null
                    && propDeclaration.AccessorList.Accessors[0].Body == null;
            }
        }

        private static Diagnostic CreateDiagnostic(ISymbol property, string otherMemberName)
        {
            var propertyNode = property.DeclaringSyntaxReferences[0].GetSyntax() as PropertyDeclarationSyntax;
            return Diagnostic.Create(
                Descriptors.X1002_RecordEntriesMustDifferCaseInsensitive,
                propertyNode?.Identifier.GetLocation(),
                property.Name,
                otherMemberName);
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
