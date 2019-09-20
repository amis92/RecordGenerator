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
                        .AddMembers(GenerateMutators(descriptor))
                        .AddMembers(GenerateMutatorWithOptionalParams(descriptor))),
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

        private static IEnumerable<MemberDeclarationSyntax> GenerateMutatorWithOptionalParams(RecordDescriptor descriptor) 
        {
            if (descriptor.Entries.Length == 0) yield break;

            //public Person With(in Optional<string> name = default, in Optional<int> age = default) {
            //    return new Person(name.GetValueOrDefault(this.Name), age.GetValueOrDefault(this.Age));
            //}

            yield return
            MethodDeclaration(descriptor.TypeSyntax, Identifier(Names.WithPrefix))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(descriptor.Entries.ToOptionalParameterSyntax())
            .WithBody(Block(SingletonList(ReturnStatement(
                            ObjectCreationExpression(descriptor.TypeSyntax)
                            .WithArgumentList(ArgumentList(SeparatedList(descriptor.Entries.Select(GetValue))))))));

            ArgumentSyntax GetValue(RecordDescriptor.Entry entry) {
                return
                Argument(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(entry.IdentifierInCamelCase.Text),
                            IdentifierName(nameof(Optional<int>.GetValueOr))))
                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(entry.Identifier.Text)))))));
            }
        }

        public static MemberDeclarationSyntax GenerateToString(RecordDescriptor descriptor)
        {
            var expression = descriptor.Entries.Length == 1
                ? SinglePropertyToString()
                : MultiplePropertiesToString();
            return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    Names.ToString)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword)
                .WithExpressionBody(expression);

            ExpressionSyntax SinglePropertyToString()
            {
                var name = descriptor.Entries[0].Identifier;
                return
                    BinaryExpression(
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

            ExpressionSyntax MultiplePropertiesToString()
            {
                var properties =
                    from e in descriptor.Entries
                    select AnonymousObjectMemberDeclarator(IdentifierName(e.Identifier));
                return
                    InvocationExpression(
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
