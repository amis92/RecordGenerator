using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Amadevus.RecordGenerator
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

        public static AttributeSyntax ExtractRecordAttribute(this TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.AttributeLists
                   .SelectMany(attListSyntax => attListSyntax.Attributes.Where(att => att.IsRecordAttributeSyntax()))
                   .FirstOrDefault();
        }

        public static SyntaxKind GetConstructorAccessSyntaxKind(this AttributeSyntax syntax)
        {
            switch (syntax.GetConstructorAccess())
            {
                case "public": return SyntaxKind.PublicKeyword;
                case "protected": return SyntaxKind.ProtectedKeyword;
                case "private": return SyntaxKind.PrivateKeyword;
                default:
                    return SyntaxKind.PublicKeyword;
            }
        }

        public static string GetConstructorAccess(this AttributeSyntax syntax)
        {
            var argument = syntax?.ArgumentList?.Arguments
                .FirstOrDefault(
                    x => x.NameEquals.Name.Identifier.ValueText == RecordAttributeProperties.ConstructorAccessName);
            var token = (argument?.Expression as LiteralExpressionSyntax)?.Token ?? default(SyntaxToken);
            return token.IsKind(SyntaxKind.StringLiteralToken)
                ? MapToCorrectAccessValueOrDefault(token.ValueText)
                : RecordAttributeProperties.ConstructorAccessDefault;
        }

        private static string MapToCorrectAccessValueOrDefault(string access)
        {
            return access == "public" || access == "protected" || access == "private"
                ? access
                : RecordAttributeProperties.ConstructorAccessDefault;
        }

        public static bool GetGenerateMutators(this AttributeSyntax syntax)
        {
            var argument = syntax?.ArgumentList?.Arguments
                .FirstOrDefault(
                    x => x.NameEquals.Name.Identifier.ValueText == RecordAttributeProperties.GenerateMutatorsName);
            var token = (argument?.Expression as LiteralExpressionSyntax)?.Token ?? default(SyntaxToken);
            return token.IsKind(SyntaxKind.FalseKeyword)
                || token.IsKind(SyntaxKind.TrueKeyword)
                ? (bool)token.Value
                : RecordAttributeProperties.GenerateMutatorsDefault;
        }

        public static bool GetGenerateCollectionMutators(this AttributeSyntax syntax)
        {
            var argument = syntax?.ArgumentList?.Arguments
                .FirstOrDefault(
                    x => x.NameEquals.Name.Identifier.ValueText == RecordAttributeProperties.GenerateCollectionMutatorsName);
            var token = (argument?.Expression as LiteralExpressionSyntax)?.Token ?? default(SyntaxToken);
            return token.IsKind(SyntaxKind.FalseKeyword)
                || token.IsKind(SyntaxKind.TrueKeyword)
                ? (bool)token.Value
                : RecordAttributeProperties.GenerateCollectionMutatorsDefault;
        }
    }
}