using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class BuilderPartialGenerator
    {
        public static IPartialGenerator Instance =>
            PartialGenerator.Create(Features.Builder, (descriptor, _) =>
                PartialGenerationResult.Empty
                    .AddMembers(GenerateToBuilderMethod(descriptor),
                                GenerateBuilder(descriptor)));

        private static ClassDeclarationSyntax GenerateBuilder(RecordDescriptor descriptor)
        {
            return
                ClassDeclaration(Names.Builder)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                .WithMembers(GenerateBuilderMembers());

            SyntaxList<MemberDeclarationSyntax> GenerateBuilderMembers()
            {
                return List<MemberDeclarationSyntax>()
                    .AddRange(descriptor.Entries.SelectMany(GetPropertyMembers))
                    .Add(GetBuilderToImmutableMethod(descriptor));
            }
        }

        private static IEnumerable<MemberDeclarationSyntax> GetPropertyMembers(RecordDescriptor.Entry entry)
        {
            return CreateSimpleProperty();

            IEnumerable<PropertyDeclarationSyntax> CreateSimpleProperty()
            {
                yield return
                    PropertyDeclaration(
                        entry.Type,
                        entry.Identifier)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken());
            }
        }

        private static MethodDeclarationSyntax GetBuilderToImmutableMethod(RecordDescriptor descriptor)
        {
            return
                MethodDeclaration(
                    descriptor.TypeSyntax,
                    Names.ToImmutable)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithBody(
                    Block(
                        ReturnStatement(
                            ObjectCreationExpression(descriptor.TypeSyntax)
                            .WithArgumentList(
                                CreateArgumentList()))));
            ArgumentListSyntax CreateArgumentList()
            {
                return ArgumentList(
                        SeparatedList(
                            descriptor.Entries.Select(
                                entry => Argument(IdentifierName(entry.Identifier)))));
            }
        }

        private static MethodDeclarationSyntax GenerateToBuilderMethod(RecordDescriptor descriptor)
        {
            return MethodDeclaration(
                    IdentifierName(Names.Builder),
                    Names.ToBuilder)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithBody(
                    Block(
                        CreateStatements()));
            IEnumerable<StatementSyntax> CreateStatements()
            {
                yield return
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName(Names.Builder))
                        .WithInitializer(
                            InitializerExpression(
                                SyntaxKind.ObjectInitializerExpression,
                                SeparatedList(
                                    descriptor.Entries.Select(CreateInitializerForEntry)))));
            }
            ExpressionSyntax CreateInitializerForEntry(RecordDescriptor.Entry entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(entry.Identifier),
                        IdentifierName(entry.Identifier));
            }
        }
    }
}
