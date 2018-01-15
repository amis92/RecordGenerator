using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator
{
    internal class RecordCorePartialGenerator : CorePartialGeneratorBase
    {
        protected RecordCorePartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken)
            : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new RecordCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return
                SingletonList<MemberDeclarationSyntax>(
                    GenerateConstructor())
                .Add(
                    GenerateUpdateMethod())
                .AddRange(
                    GenerateMutators());
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            return ConstructorDeclaration(Descriptor.TypeIdentifier)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(GenerateFullParameterList())
                .WithBody(CreateBody());

            BlockSyntax CreateBody()
            {
                return Block(Descriptor.Entries.Select(CreateCtorAssignment));

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
                                IdentifierName(entry.Identifier)));
                }
            }
        }

        private MethodDeclarationSyntax GenerateUpdateMethod()
        {
            var arguments = Descriptor.Entries.Select(x =>
            {
                return Argument(
                    IdentifierName(x.Identifier));
            });
            return MethodDeclaration(Descriptor.Type, Names.Update)
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(GenerateFullParameterList())
                .WithBody(
                    Block(
                        ReturnStatement(
                            ObjectCreationExpression(
                                Descriptor.Type)
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList(arguments))))));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMutators()
        {
            return Descriptor.Entries.Select(CreateRecordMutator);
            MethodDeclarationSyntax CreateRecordMutator(RecordDescriptor.Entry entry)
            {
                var arguments = Descriptor.Entries.Select(x =>
                {
                    return Argument(
                        IdentifierName(x.Identifier));
                });
                var mutator =
                    MethodDeclaration(
                        Descriptor.Type,
                        GetMutatorIdentifier())
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(
                                    entry.Identifier)
                                .WithType(entry.Type))))
                    .WithBody(
                        Block(
                            ReturnStatement(
                                InvocationExpression(
                                    IdentifierName(Names.Update))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList(arguments))))));
                return mutator;

                SyntaxToken GetMutatorIdentifier()
                {
                    return Identifier($"{Names.WithPrefix}{entry.Identifier.ValueText}");
                }
            }
        }

        private ParameterListSyntax GenerateFullParameterList()
        {
            return ParameterList(
                    SeparatedList(
                        Descriptor.Entries.Select(CreateParameter)));

            ParameterSyntax CreateParameter(RecordDescriptor.Entry property)
            {
                return Parameter(
                        property.Identifier)
                    .WithType(property.Type);
            }
        }
    }
}
