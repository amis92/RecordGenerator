using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class RecordPartialGenerator
    {
        public static IPartialGenerator Instance =>
            PartialGenerator.Combine(
                // constructor
                PartialGenerator.Create(Features.Constructor,
                    descriptor =>
                        PartialGenerationResult.Empty
                        .AddMembers(
                            GenerateConstructor(descriptor),
                            PartialOnConstructedMethodDeclaration)),
                // withers
                PartialGenerator.Create(Features.Withers,
                    descriptor =>
                        PartialGenerationResult.Empty
                        .AddMember(GenerateUpdateMethod(descriptor))
                        .AddMembers(GenerateMutators(descriptor))),
                // string formatting
                PartialGenerator.Member(Features.ToString, GenerateToString));

        private static readonly MethodDeclarationSyntax PartialOnConstructedMethodDeclaration =
            MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Names.OnConstructed)
            .AddModifiers(SyntaxKind.PartialKeyword)
            .WithSemicolonToken();

        private static readonly StatementSyntax OnConstructedInvocationStatement =
            ExpressionStatement(InvocationExpression(IdentifierName(PartialOnConstructedMethodDeclaration.Identifier)));

        private static ConstructorDeclarationSyntax GenerateConstructor(RecordDescriptor descriptor)
        {
            return
                ConstructorDeclaration(descriptor.TypeIdentifier)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithParameters(
                    descriptor.Entries.Select(CreateParameter))
                .WithBodyStatements(
                    descriptor.Entries
                    .Select(CreateCtorAssignment)
                    .Append(OnConstructedInvocationStatement));
            StatementSyntax CreateCtorAssignment(RecordDescriptor.Entry entry)
            {
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(entry.Identifier)),
                            IdentifierName(entry.IdentifierInCamelCase)));
            }
        }

        private static MethodDeclarationSyntax GenerateUpdateMethod(RecordDescriptor descriptor)
        {
            var arguments = descriptor.Entries.Select(x =>
            {
                return Argument(
                    IdentifierName(x.IdentifierInCamelCase));
            });
            return
                MethodDeclaration(descriptor.TypeSyntax, Names.Update)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithParameters(
                    descriptor.Entries.Select(CreateParameter))
                .WithBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            descriptor.TypeSyntax)
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList(arguments)))));
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateMutators(RecordDescriptor descriptor)
        {
            return descriptor.Entries.Select(CreateRecordMutator);
            MethodDeclarationSyntax CreateRecordMutator(RecordDescriptor.Entry entry)
            {
                var valueIdentifier = Identifier(Names.Value);

                var arguments = descriptor.Entries.Select(x =>
                {
                    return Argument(
                        IdentifierName(x == entry ? valueIdentifier : x.Identifier));
                });

                var mutator =
                    MethodDeclaration(
                        descriptor.TypeSyntax,
                        GetMutatorIdentifier())
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(
                        Parameter(
                            valueIdentifier)
                        .WithType(entry.TypeSyntax))
                    .WithBodyStatements(
                        ReturnStatement(
                            InvocationExpression(
                                IdentifierName(Names.Update))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList(arguments)))));
                return mutator;
                SyntaxToken GetMutatorIdentifier()
                {
                    return Identifier($"{Names.WithPrefix}{entry.Identifier.ValueText}");
                }
            }
        }

        public static MemberDeclarationSyntax GenerateToString(RecordDescriptor descriptor)
        {
            var expression = descriptor.Entries.Length == 1
                ? SinglePropertyToString(descriptor)
                : MultiplePropertiesToString(descriptor);

            return MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.StringKeyword)),
                        Names.ToString)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .WithExpressionBody(expression);


            ExpressionSyntax SinglePropertyToString(RecordDescriptor rd)
            {
                var name = rd.Entries[0].Identifier;

                return BinaryExpression(
                    SyntaxKind.AddExpression,
                    BinaryExpression(
                        SyntaxKind.AddExpression,
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal("{ " + name + " = ")),
                        IdentifierName(name)),
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(" }")));
            }

            ExpressionSyntax MultiplePropertiesToString(RecordDescriptor rd)
            {
                var properties = from e in rd.Entries
                                 select AnonymousObjectMemberDeclarator(IdentifierName(e.Identifier));

                return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, 
                        AnonymousObjectCreationExpression()
                            .AddInitializers(properties.ToArray()),
                        IdentifierName(Names.ToString)));
            }

        }

        private static ParameterSyntax CreateParameter(RecordDescriptor.Entry property)
        {
            return Parameter(
                    property.IdentifierInCamelCase)
                .WithType(property.TypeSyntax);
        }
    }
}
