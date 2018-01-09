using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
                && text.ToLower() is var lowerCase
                && (lowerCase == "Record".ToLower() || lowerCase == "RecordAttribute".ToLower());
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