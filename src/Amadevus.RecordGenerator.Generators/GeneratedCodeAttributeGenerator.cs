using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    public static class GeneratedCodeAttributeGeneratorExtensions
    {
        public static TypeDeclarationSyntax WithGeneratedCodeAttribute(
            this TypeDeclarationSyntax typeSyntax)
        {
            return new GeneratedCodeAttributeGenerator(typeSyntax)
                .GetWithGeneratedCodeAttribute();
        }
    }

    internal sealed class GeneratedCodeAttributeGenerator
    {
        private readonly TypeDeclarationSyntax typeSyntax;

        public GeneratedCodeAttributeGenerator(
            TypeDeclarationSyntax typeSyntax)
        {
            this.typeSyntax = typeSyntax;
        }

        public TypeDeclarationSyntax GetWithGeneratedCodeAttribute()
        {
            return typeSyntax.WithAttributeLists(
                SingletonList(
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(GenerateQualifiedName())
                            .WithArgumentList(GenerateAttributeArgumentList())))));
        } 

        private AttributeArgumentListSyntax GenerateAttributeArgumentList()
        {
            var toolName = Names.ToolName;
            var toolVersion = GetToolVersion();

            return AttributeArgumentList(
                SeparatedList<AttributeArgumentSyntax>(
                    new SyntaxNodeOrToken[]{
                        AttributeArgument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(toolName))),
                        Token(SyntaxKind.CommaToken),
                        AttributeArgument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(toolVersion)))}));
        }

        private QualifiedNameSyntax GenerateQualifiedName()
        {
            var namespaceAndIdentifier = Names.GeneratedCodeAttribute.Split('.')
                .Select(n => IdentifierName(n)).ToList();

            var attributeName = QualifiedName(
                namespaceAndIdentifier.First(),
                namespaceAndIdentifier.Skip(1).First());

            foreach (var identifier in namespaceAndIdentifier.Skip(2))
            {
                attributeName = QualifiedName(attributeName, identifier);
            }

            return attributeName; 
        }

        private string GetToolVersion() => GetType().GetTypeInfo().Assembly.GetName().Version.ToString();
    }
}
