using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return
                SingletonList<MemberDeclarationSyntax>(
                    GenerateConstructor())
                .Add(
                    GenerateUpdateMethod())
                .AddRange(
                    GenerateMutators())
                .Add(GenerateValidatePartialMethod());
        }

        private ConstructorDeclarationSyntax GenerateConstructor()
        {
            return ConstructorDeclaration(Descriptor.TypeIdentifier)
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
                            IdentifierName(entry.Identifier)));
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
                        IdentifierName(entry.Identifier))
                    .WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
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
                var arguments = Descriptor.Entries.Select(x =>
                {
                    return Argument(
                        IdentifierName(x.Identifier));
                });
                var mutator =
                    MethodDeclaration(
                        Descriptor.Type,
                        GetMutatorIdentifier())
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(
                        Parameter(
                            entry.Identifier)
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

        private MemberDeclarationSyntax GenerateValidatePartialMethod()
        {
            return
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
                    Parameter(entry.Identifier)
                    .WithType(entry.Type)
                    .AddModifiers(Token(SyntaxKind.RefKeyword));
            }
        }

        private static ParameterSyntax CreateParameter(RecordDescriptor.Entry property)
        {
            return Parameter(
                    property.Identifier)
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
