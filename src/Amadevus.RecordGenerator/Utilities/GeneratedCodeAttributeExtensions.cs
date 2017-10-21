using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Amadevus.RecordGenerator
{
    public static class GeneratedCodeAttributeExtensions
    {
        public const string AttributeQualifiedName = "System.CodeDom.Compiler.GeneratedCode";

        public static readonly NameSyntax AttributeName = SyntaxFactory.ParseName(AttributeQualifiedName);

        public static bool HasGeneratedCodeAttribute(this TypeDeclarationSyntax typeDeclaration, bool checkToolName = true)
        {
            return typeDeclaration.ExtractGeneratedCodeAttributeToolArgument() is string tool
                && (checkToolName ? tool == Properties.AnalyzerName : true);
        }
        
        public static AttributeSyntax ExtractGeneratedCodeAttribute(this TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.AttributeLists
                .Select(
                    list => list.Attributes.FirstOrDefault(
                        attribute => attribute.Name.IsEquivalentTo(AttributeName)))
                .FirstOrDefault();
        }

        public static string ExtractGeneratedCodeAttributeToolArgument(this TypeDeclarationSyntax typeDeclaration)
        {
            var generatedCodeAttribute = typeDeclaration.ExtractGeneratedCodeAttribute();
            if (generatedCodeAttribute == null)
            {
                return null;
            }
            return generatedCodeAttribute.ExtractGeneratedCodeAttributeToolArgument();
        }

        public static string ExtractGeneratedCodeAttributeToolArgument(this AttributeSyntax generatedCodeAttribute)
        {
            var attArguments = generatedCodeAttribute.ArgumentList.Arguments;
            if (attArguments.Count < 2)
            {
                // weird situation, GeneratedCode has 2 required parameters
                return null;
            }
            var version =
                attArguments[0]
                .DescendantTokens()
                .FirstOrDefault(token => token.IsKind(SyntaxKind.StringLiteralToken))
                .ValueText;
            return version;
        }

        public static string ExtractGeneratedCodeAttributeVersionArgument(this TypeDeclarationSyntax typeDeclaration)
        {
            var generatedCodeAttribute = typeDeclaration.ExtractGeneratedCodeAttribute();
            if (generatedCodeAttribute == null)
            {
                return null;
            }
            return generatedCodeAttribute.ExtractGeneratedCodeAttributeVersionArgument();
        }

        public static string ExtractGeneratedCodeAttributeVersionArgument(this AttributeSyntax generatedCodeAttribute)
        {
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
                            SyntaxFactory.ParseAttributeArgumentList($"(\"{Properties.AnalyzerName}\", \"{Properties.VersionString}\")"))));
        }
    }
}
