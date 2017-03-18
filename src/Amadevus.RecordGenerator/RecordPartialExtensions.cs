using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal static class RecordPartialExtensions
    {
        public static bool IsPartOfRecordPartialFile(this ClassDeclarationSyntax declaration)
        {
            var firstSingleLineComment = declaration.SyntaxTree
                                .GetRoot()
                                .DescendantTrivia()
                                .FirstOrDefault(trivia => trivia.Kind() == SyntaxKind.SingleLineCommentTrivia);
            return firstSingleLineComment.Kind() == SyntaxKind.None 
                ? false
                : firstSingleLineComment.Token.ValueText.StartsWith(RecordPartial.FileHeader);
        }
    }
}