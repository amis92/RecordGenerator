using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator.Analyzers
{
    internal static class RecordAttributeExtensions
    {
        public static bool IsRecordAttributeSyntax(this AttributeSyntax attSyntax)
        {
            return attSyntax.GetUnqualifiedName()?.IsRecordAttributeName() ?? false;
        }

        public static bool IsRecordAttributeName(this string text)
        {
            return !string.IsNullOrWhiteSpace(text)
                && (text == "Record" || text == "RecordAttribute");
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
}