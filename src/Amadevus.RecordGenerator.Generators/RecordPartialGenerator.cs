using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class RecordPartialGenerator : PartialGeneratorBase
    {
        protected RecordPartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken)
            : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new RecordPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override Features TriggeringFeatures =>
            Features.Constructor | Features.Withers | Features.ToString;

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(_().SelectMany(x => x));

            IEnumerable<IEnumerable<MemberDeclarationSyntax>> _()
            {
                if (Descriptor.Features.HasFlag(Features.Constructor))
                {
                    yield return GenerateConstructor();
                    yield return GenerateValidatePartialMethod();
                }
                if (Descriptor.Features.HasFlag(Features.Withers))
                {
                    yield return GenerateUpdateMethod();
                    yield return GenerateMutators();
                }
                if (Descriptor.Features.HasFlag(Features.ToString))
                {
                    yield return GenerateToString();
                }
            }
        }

        private IEnumerable<ConstructorDeclarationSyntax> GenerateConstructor()
        {
            yield return
                ConstructorDeclaration(Descriptor.TypeIdentifier)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithParameters(
                    Descriptor.Entries.Select(CreateParameter))
                .WithBodyStatements(
                    Descriptor.Entries
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
                            Descriptor.Entries.Select(CreateValidateArgument).ToArray()));
            }
            ArgumentSyntax CreateValidateArgument(RecordDescriptor.Entry entry)
            {
                return
                    Argument(
                        IdentifierName(entry.IdentifierInCamelCase))
                    .WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
            }
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateUpdateMethod()
        {
            var arguments = Descriptor.Entries.Select(x =>
            {
                return Argument(
                    IdentifierName(x.IdentifierInCamelCase));
            });
            yield return
                MethodDeclaration(Descriptor.Type, Names.Update)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithParameters(
                    Descriptor.Entries.Select(CreateParameter))
                .WithBodyStatements(
                    ReturnStatement(
                        ObjectCreationExpression(
                            Descriptor.Type)
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList(arguments)))));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMutators()
        {
            return Descriptor.Entries.Select(CreateRecordMutator);
            MethodDeclarationSyntax CreateRecordMutator(RecordDescriptor.Entry entry)
            {
                var valueIdentifier = Identifier(Names.Value);

                var arguments = Descriptor.Entries.Select(x =>
                {
                    return Argument(
                        IdentifierName(x == entry ? valueIdentifier : x.Identifier));
                });

                var mutator =
                    MethodDeclaration(
                        Descriptor.Type,
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

        private IEnumerable<MemberDeclarationSyntax> GenerateToString()
        {
            var properties =
                from e in Descriptor.Entries
                select AnonymousObjectMemberDeclarator(IdentifierName(e.Identifier));
            yield return
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

        private IEnumerable<MemberDeclarationSyntax> GenerateValidatePartialMethod()
        {
            yield return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Names.Validate)
                .AddParameterListParameters(
                    Descriptor.Entries.Select(CreateValidateParameter).ToArray())
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

        private ParameterListSyntax GenerateFullParameterList()
        {
            return ParameterList(
                    SeparatedList(
                        Descriptor.Entries.Select(CreateParameter)));
        }
    }
}
