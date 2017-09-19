using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal static class SyntaxExtensions
    {
        public static string NameWithArity(this TypeDeclarationSyntax declaration)
        {
            return declaration.Arity == 0 ? declaration.Identifier.ValueText : $"{declaration.Identifier.ValueText}`{declaration.Arity}";
        }

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

        public static bool IsRecordViable(this PropertyDeclarationSyntax pdSyntax)
        {
            return (pdSyntax.AccessorList?.Accessors.All(x => x.Kind() != SyntaxKind.SetAccessorDeclaration && x.Body == null) ?? false)
                && pdSyntax.Modifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword)
                && pdSyntax.Modifiers.All(x => x.Kind() != SyntaxKind.StaticKeyword);
        }

        public static ImmutableArray<PropertyDeclarationSyntax> GetRecordProperties(this SyntaxList<MemberDeclarationSyntax> members)
        {
            return members.OfType<PropertyDeclarationSyntax>()
                .Where(propSyntax => propSyntax.IsRecordViable())
                .ToImmutableArray();
        }

        public static ImmutableArray<RecordEntry> AsRecordEntries(this IEnumerable<PropertyDeclarationSyntax> properties)
        {
            return properties.Select(p => new RecordEntry(p.Identifier, p.Type)).ToImmutableArray();
        }

        public static BlockSyntax IntoCtorBody(this IReadOnlyList<RecordEntry> properties)
        {
            var block = SyntaxFactory.Block(properties.IntoCtorAssignments());
            return block;
        }

        private static IEnumerable<StatementSyntax> IntoCtorAssignments(this IReadOnlyList<RecordEntry> properties)
        {
            return properties.Select(p => p.IntoCtorAssignment());
        }

        private static StatementSyntax IntoCtorAssignment(this RecordEntry p)
        {
            var assignment =
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ThisExpression(),
                            SyntaxFactory.IdentifierName(p.Identifier)),
                        SyntaxFactory.IdentifierName(p.Identifier)));
            return assignment;
        }

        public static ParameterListSyntax IntoCtorParameterList(this IReadOnlyList<RecordEntry> properties)
        {
            var list =
                SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList(properties.Select(p => p.IntoParameter())));
            return list;
        }


        public static ParameterSyntax IntoParameter(this RecordEntry property)
        {
            var parameter = 
                SyntaxFactory.Parameter(property.Identifier)
                .WithType(property.Type);
            return parameter;
        }
    }
}
