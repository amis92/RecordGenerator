using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Amadevus.RecordGenerator
{
    internal static class SyntaxExtensions
    {
        public static ClassDeclarationSyntax WithPartialModifier(this ClassDeclarationSyntax declaration)
        {
            return declaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        }
        public static StructDeclarationSyntax WithPartialModifier(this StructDeclarationSyntax declaration)
        {
            return declaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        }

        public static TypeDeclarationSyntax WithPartialModifier(this TypeDeclarationSyntax declaration)
        {
            if (declaration is ClassDeclarationSyntax classDeclaration)
            {
                return classDeclaration.WithPartialModifier();
            }
            if (declaration is StructDeclarationSyntax structDeclaration)
            {
                return structDeclaration.WithPartialModifier();
            }
            return declaration;
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
            return accessor.Kind() == SyntaxKind.GetAccessorDeclaration && accessor.Body == null;
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
