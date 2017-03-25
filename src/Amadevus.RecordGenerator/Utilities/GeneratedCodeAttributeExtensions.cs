using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Amadevus.RecordGenerator
{
    public static class GeneratedCodeAttributeExtensions
    {
        public const string AttributeQualifiedName = "System.CodeDom.Compiler.GeneratedCode";

        public static readonly NameSyntax AttributeName = SyntaxFactory.ParseName(AttributeQualifiedName);

        public static string ExtractGeneratedCodeAttributeVersionArgument(this TypeDeclarationSyntax typeDeclaration)
        {
            var generatedCodeAttribute =
                typeDeclaration.AttributeLists.Select(
                    list => list.Attributes.FirstOrDefault(
                        attribute => attribute.Name.IsEquivalentTo(AttributeName)))
                .FirstOrDefault();
            if (generatedCodeAttribute == null)
            {
                return null;
            }
            var attArguments = generatedCodeAttribute.ArgumentList.Arguments;
            if (attArguments.Count < 2)
            {
                // weird situation, GeneratedCode has 2 required parameters
                return null;
            }
            var version =
                attArguments[1]
                .DescendantTokens()
                .FirstOrDefault(token => token.IsKind(SyntaxKind.StringLiteralToken))
                .ValueText;
            return version;
        }

        public static AttributeListSyntax CreateAttribute()
        {
            return
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName(AttributeQualifiedName))
                        .WithArgumentList(
                            SyntaxFactory.ParseAttributeArgumentList($"(\"{nameof(RecordGenerator)}\", \"{Properties.VersionString}\")"))));
        }
    }
}
