using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    public static class RecordPartialComparer
    {
        public enum Result
        {
            Equivalent,
            EquivalentExceptGeneratedCodeAttributeVersion,
            NotEquivalent
        }

        public static Result IsTokenwiseEquivalentTo(this SyntaxNode nodeA, SyntaxNode nodeB) => IsTokenwiseEquivalent(nodeA, nodeB);

        public static Result IsTokenwiseEquivalent(SyntaxNode nodeA, SyntaxNode nodeB)
        {
            var listA = nodeA.ChildNodesAndTokens();
            var listB = nodeB.ChildNodesAndTokens();
            if (listA.Count != listB.Count)
            {
                return Result.NotEquivalent;
            }
            var difference =
                listA
                .Zip(listB, IsTokenwiseEquivalent)
                .SkipWhile(equivalent => equivalent == Result.Equivalent)
                .Select(result => (Result?)result)
                .FirstOrDefault() ?? Result.Equivalent;
            switch (difference)
            {
                case Result.EquivalentExceptGeneratedCodeAttributeVersion:
                case Result.Equivalent:
                    return difference;
                case Result.NotEquivalent:
                    {
                        if (nodeA is AttributeSyntax attributeA
                            && nodeB is AttributeSyntax attributeB
                            && attributeA.Name.IsEquivalentTo(GeneratedCodeAttributeExtensions.AttributeName)
                            && attributeB.Name.IsEquivalentTo(GeneratedCodeAttributeExtensions.AttributeName))
                        {
                            return Result.EquivalentExceptGeneratedCodeAttributeVersion;
                        }
                    }
                    break;
                default:
                    break;
            }
            return Result.NotEquivalent;
        }

        public static Result IsTokenwiseEquivalent(SyntaxNodeOrToken nodeOrTokenA, SyntaxNodeOrToken nodeOrTokenB)
        {
            if (nodeOrTokenA.IsNode && nodeOrTokenB.IsNode)
            {
                return IsTokenwiseEquivalent(nodeOrTokenA.AsNode(), nodeOrTokenB.AsNode());
            }
            if (nodeOrTokenA.IsToken && nodeOrTokenB.IsToken)
            {
                return IsTokenwiseEquivalent(nodeOrTokenA.AsToken(), nodeOrTokenB.AsToken());
            }
            return Result.NotEquivalent;
        }

        public static Result IsTokenwiseEquivalent(SyntaxToken tokenA, SyntaxToken tokenB)
        {
            if (tokenA.RawKind == tokenB.RawKind
                && tokenA.ValueText == tokenB.ValueText)
            {
                return Result.Equivalent;
            }
            return Result.NotEquivalent;
        }
    }
}
