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
            PartialGenerator.Combine(ConstructorGenerator,
                                     WithersGenerator,
                                     ToStringGenerator);

        private static readonly IPartialGenerator ConstructorGenerator =
            PartialGenerator.Create(Features.Constructor, descriptor =>
                Generation.Empty.AddMembers(GenerateConstructor(descriptor),
                                            GenerateValidatePartialMethod(descriptor)));

        private static readonly IPartialGenerator WithersGenerator =
            PartialGenerator.Create(Features.Withers, descriptor =>
                Generation.Empty.AddMember(GenerateUpdateMethod(descriptor))
                                .AddMembers(GenerateMutators(descriptor)));

        private static readonly IPartialGenerator ToStringGenerator =
            PartialGenerator.Member(Features.ToString, GenerateToString);

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
                            .Prepend(CreateValidateInvocation()));
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
            StatementSyntax CreateValidateInvocation()
            {
                return
                    ExpressionStatement(
                        InvocationExpression(
                                IdentifierName(Names.Validate))
                            .AddArgumentListArguments(
                                descriptor.Entries.Select(CreateValidateArgument).ToArray()));
            }
            ArgumentSyntax CreateValidateArgument(RecordDescriptor.Entry entry)
            {
                return
                    Argument(
                            IdentifierName(entry.IdentifierInCamelCase))
                        .WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
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
                MethodDeclaration(descriptor.Type, Names.Update)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(
                        descriptor.Entries.Select(CreateParameter))
                    .WithBodyStatements(
                        ReturnStatement(
                            ObjectCreationExpression(
                                    descriptor.Type)
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
                            descriptor.Type,
                            GetMutatorIdentifier())
                        .AddModifiers(SyntaxKind.PublicKeyword)
                        .WithParameters(
                            Parameter(
                                    valueIdentifier)
                                .WithType(entry.Type))
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
            var properties =
                from e in descriptor.Entries
                select AnonymousObjectMemberDeclarator(IdentifierName(e.Identifier));
            return
                MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.StringKeyword)),
                        Names.ToString)
                    .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                    .WithExpressionBody(
                        InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                AnonymousObjectCreationExpression()
                                    .AddInitializers(properties.ToArray()),
                                IdentifierName(Names.ToString))));
        }

        private static MemberDeclarationSyntax GenerateValidatePartialMethod(RecordDescriptor descriptor)
        {
            return
                MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)),
                        Names.Validate)
                    .AddParameterListParameters(
                        descriptor.Entries.Select(CreateValidateParameter).ToArray())
                    .AddModifiers(SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword)
                    .WithSemicolonToken();
            ParameterSyntax CreateValidateParameter(RecordDescriptor.Entry entry)
            {
                return
                    Parameter(entry.IdentifierInCamelCase)
                        .WithType(entry.Type)
                        .AddModifiers(Token(SyntaxKind.RefKeyword));
            }
        }

        private static ParameterSyntax CreateParameter(RecordDescriptor.Entry property)
        {
            return Parameter(
                    property.IdentifierInCamelCase)
                .WithType(property.Type);
        }
    }
}
