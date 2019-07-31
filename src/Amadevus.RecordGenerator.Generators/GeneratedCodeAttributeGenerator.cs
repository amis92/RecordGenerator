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
        private static readonly GeneratedCodeAttributeApplier attributeApplier
            = new GeneratedCodeAttributeApplier(new GeneratedCodeAttributeGenerator());

        public static TypeDeclarationSyntax AddGeneratedCodeAttributeOnMembers(
            this TypeDeclarationSyntax typeSyntax)
        {
            return attributeApplier.AddGeneratedCodeAttributeOnMembers(typeSyntax);
        }
    }

    internal sealed class GeneratedCodeAttributeApplier
    {
        private readonly GeneratedCodeAttributeGenerator attributeGenerator;

        private readonly IDictionary<Type, Func<MemberReplacementContext, SyntaxNode>> Updates
            = new Dictionary<Type, Func<MemberReplacementContext, SyntaxNode>>
        {
            {
                typeof(MethodDeclarationSyntax),
                (context) => ((MethodDeclarationSyntax)context.Member).AddAttributeLists(context.AttributeGenerator.GenerateAttributeListSyntax())
            },
            {
                typeof(PropertyDeclarationSyntax),
                (context) => ((PropertyDeclarationSyntax)context.Member).AddAttributeLists(context.AttributeGenerator.GenerateAttributeListSyntax())
            },
            {
                typeof(ConstructorDeclarationSyntax),
                (context) => ((ConstructorDeclarationSyntax)context.Member).AddAttributeLists(context.AttributeGenerator.GenerateAttributeListSyntax())
            },
            {
                typeof(ClassDeclarationSyntax),
                (context) => context.AttributeApplier.AddGeneratedCodeAttributeOnMembers((ClassDeclarationSyntax)context.Member)
            }
        };


        public GeneratedCodeAttributeApplier(
            GeneratedCodeAttributeGenerator attributeGenerator)
        {
            this.attributeGenerator = attributeGenerator;
        }

        
        public TypeDeclarationSyntax AddGeneratedCodeAttributeOnMembers(
            TypeDeclarationSyntax typeDeclaration)
        {
            var nodesToBeReplaced = typeDeclaration
                .ChildNodes()
                .Where(node => Updates.Keys.Any(key => key.IsAssignableFrom(node.GetType())));

            SyntaxNode calculateReplacement(SyntaxNode rootNode, SyntaxNode toBeReplacedNode)
            {
                var context = new MemberReplacementContext(toBeReplacedNode, this, attributeGenerator);
                var updater = Updates[toBeReplacedNode.GetType()];
                return updater(context);
            }

            return typeDeclaration.ReplaceNodes(nodesToBeReplaced, calculateReplacement);
        }

        private class MemberReplacementContext
        {
            public SyntaxNode Member { get; }
            public GeneratedCodeAttributeApplier AttributeApplier { get; }
            public GeneratedCodeAttributeGenerator AttributeGenerator { get; }

            public MemberReplacementContext(
                SyntaxNode member,
                GeneratedCodeAttributeApplier attributeApplier,
                GeneratedCodeAttributeGenerator attributeGenerator)
            {
                Member = member;
                AttributeApplier = attributeApplier;
                AttributeGenerator = attributeGenerator;
            }
        }
    }

    internal sealed class GeneratedCodeAttributeGenerator
    {
        public AttributeListSyntax GenerateAttributeListSyntax()
        {
            return AttributeList(
                    SingletonSeparatedList(
                        Attribute(GenerateQualifiedName())
                        .WithArgumentList(GenerateAttributeArgumentList())));
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
