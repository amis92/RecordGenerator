using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class SyntaxExtensions
    {
        public static ParameterListSyntax ToOptionalParameterSyntax(this IEnumerable<RecordDescriptor.Entry> entries) 
        {
            // (in Optional<string> name = default, in Optional<int> age = default)
            return
            ParameterList(
                SeparatedList(
                    entries.Select(x => Parameter(x.IdentifierInCamelCase)
                                        .AddModifiers(Token(SyntaxKind.InKeyword))
                                        .WithType(x.TypeSyntax.ToOptionalType())
                                        .WithDefault(EqualsValueClause(LiteralExpression(
                                            SyntaxKind.DefaultLiteralExpression,
                                            Token(SyntaxKind.DefaultKeyword)))))));
        }

        public static QualifiedNameSyntax ToOptionalType(this TypeSyntax type) 
        {
            // Optional<type>
            return 
            QualifiedName(
                QualifiedName(
                    IdentifierName(nameof(Amadevus)),
                    IdentifierName(nameof(Amadevus.RecordGenerator))
                ),
                GenericName(Identifier(nameof(Amadevus.RecordGenerator.Optional<int>)))
                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(type)))
            );
        }

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

        public static bool HasOnlyGetterWithNoBody(this PropertyDeclarationSyntax pdSyntax)
        {
            return pdSyntax.AccessorList is AccessorListSyntax accList
                && accList.Accessors.Count == 1 && accList.Accessors.Single().IsGetterWithNoBody();
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

        public static SyntaxToken WithoutTrivia(this SyntaxToken syntax)
            => syntax.WithLeadingTrivia(TriviaList()).WithTrailingTrivia(TriviaList());

        public static NameSyntax GetTypeSyntax(this TypeDeclarationSyntax typeDeclaration)
        {
            var identifier = typeDeclaration.Identifier.WithoutTrivia();
            return
                typeDeclaration.TypeParameterList == null
                ? IdentifierName(identifier)
                : (NameSyntax)GenericName(identifier)
                .AddTypeArgumentListArguments(
                    typeDeclaration.TypeParameterList.Parameters
                    .Select(param => IdentifierName(param.Identifier))
                    .ToArray());
        }

        public static string GetQualifiedName(this ISymbol symbol)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
            return symbol.ToDisplayString(symbolDisplayFormat);
        }
    }
}
