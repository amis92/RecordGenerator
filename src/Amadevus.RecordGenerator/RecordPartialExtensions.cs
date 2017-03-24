using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal static class RecordPartialExtensions
    {
        public static bool IsFileHeaderPresent(this TypeDeclarationSyntax declaration)
        {
            var firstSingleLineComment = declaration.SyntaxTree
                                .GetRoot()
                                .FindTrivia(0);
            return firstSingleLineComment.Kind() != SyntaxKind.SingleLineCommentTrivia
                ? false
                : firstSingleLineComment.ToString().StartsWith(RecordPartial.FileHeader);
        }
    }
}