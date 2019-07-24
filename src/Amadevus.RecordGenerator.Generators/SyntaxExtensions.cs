using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class SyntaxExtensions
    {
        public static MethodDeclarationSyntax AddModifiers(this MethodDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static PropertyDeclarationSyntax AddModifiers(this PropertyDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static ConstructorDeclarationSyntax AddModifiers(this ConstructorDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static ClassDeclarationSyntax AddModifiers(this ClassDeclarationSyntax syntax, params SyntaxKind[] modifier)
        {
            return syntax.AddModifiers(modifier.Select(Token).ToArray());
        }

        public static PropertyDeclarationSyntax WithSemicolonToken(this PropertyDeclarationSyntax syntax)
        {
            return syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static AccessorDeclarationSyntax WithSemicolonToken(this AccessorDeclarationSyntax syntax)
        {
            return syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static MethodDeclarationSyntax WithSemicolonToken(this MethodDeclarationSyntax syntax)
        {
            return syntax.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        public static ConstructorDeclarationSyntax WithParameters(this ConstructorDeclarationSyntax syntax, IEnumerable<ParameterSyntax> parameters)
        {
            return syntax.WithParameterList(ParameterList(SeparatedList(parameters)));
        }

        public static ConstructorDeclarationSyntax WithBodyStatements(this ConstructorDeclarationSyntax syntax, IEnumerable<StatementSyntax> parameters)
        {
            return syntax.WithBody(Block(parameters));
        }

        public static ConstructorDeclarationSyntax WithBodyStatements(this ConstructorDeclarationSyntax syntax, params StatementSyntax[] parameters)
        {
            return syntax.WithBody(Block(parameters));
        }

        public static MethodDeclarationSyntax WithParameters(this MethodDeclarationSyntax syntax, IEnumerable<ParameterSyntax> parameters)
        {
            return syntax.WithParameterList(ParameterList(SeparatedList(parameters)));
        }

        public static MethodDeclarationSyntax WithParameters(this MethodDeclarationSyntax syntax, params ParameterSyntax[] parameters)
        {
            return syntax.WithParameterList(ParameterList(SeparatedList(parameters)));
        }

        public static MethodDeclarationSyntax WithBodyStatements(this MethodDeclarationSyntax syntax, IEnumerable<StatementSyntax> parameters)
        {
            return syntax.WithBody(Block(parameters));
        }

        public static MethodDeclarationSyntax WithBodyStatements(this MethodDeclarationSyntax syntax, params StatementSyntax[] parameters)
        {
            return syntax.WithBody(Block(parameters));
        }

        public static MethodDeclarationSyntax WithExpressionBody(this MethodDeclarationSyntax syntax, ExpressionSyntax body)
        {
            return
                syntax
                .WithExpressionBody(
                    ArrowExpressionClause(body))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
        }

        public static PropertyDeclarationSyntax WithAccessors(this PropertyDeclarationSyntax syntax, params AccessorDeclarationSyntax[] parameters)
        {
            return syntax.WithAccessorList(AccessorList(List(parameters)));
        }

        public static bool IsRecordViable(this PropertyDeclarationSyntax property)
        {
            return property.HasOnlyGetterWithNoBody() && property.IsPublic() && !property.IsStatic();
        }

        public static bool IsNamed(this AttributeSyntax attribute, string name)
        {
            return attribute.Name is IdentifierNameSyntax id && (id.Identifier.Text == name || id.Identifier.Text == name + "Attribute");
        }

        public static bool HasOnlyGetterWithNoBody(this PropertyDeclarationSyntax pdSyntax)
        {
            return pdSyntax.AccessorList is AccessorListSyntax accList
                ? accList.Accessors.Count == 1 && accList.Accessors.Single().IsGetterWithNoBody()
                : false;
        }

        public static bool IsGetterWithNoBody(this AccessorDeclarationSyntax accessor)
        {
            return accessor.Kind() == SyntaxKind.GetAccessorDeclaration
                && accessor.Body is null
                && accessor.ExpressionBody is null;
        }

        public static bool IsPublic(this PropertyDeclarationSyntax pdSyntax)
        {
            return pdSyntax.Modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword);
        }

        public static bool IsStatic(this PropertyDeclarationSyntax pdSyntax)
        {
            return pdSyntax.Modifiers.Any(x => x.Kind() == SyntaxKind.StaticKeyword);
        }

        public static SyntaxToken WithoutTrivia(this SyntaxToken syntax) => syntax.WithLeadingTrivia(SyntaxFactory.TriviaList()).WithTrailingTrivia(SyntaxFactory.TriviaList());

        public static NameSyntax GetTypeSyntax(this TypeDeclarationSyntax typeDeclaration)
        {
            var identifier = typeDeclaration.Identifier.WithoutTrivia();
            var typeParamList = typeDeclaration.TypeParameterList;
            if (typeParamList == null)
            {
                return SyntaxFactory.IdentifierName(identifier);
            }

            var arguments = typeParamList.Parameters.Select(param => SyntaxFactory.IdentifierName(param.Identifier));
            var typeArgList =
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList<TypeSyntax>(
                        arguments));

            return SyntaxFactory.GenericName(identifier, typeArgList);
        }

        public static bool IsImmutableArrayType(this PropertyDeclarationSyntax property)
        {
            return property.Type is GenericNameSyntax genericName && genericName.Identifier.Text == "ImmutableArray";
        }

        public static TypeSyntax WithNamespace(this SimpleNameSyntax name, string @namespace)
        {
            return
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.ParseName(@namespace),
                    name);
        }
    }
}
