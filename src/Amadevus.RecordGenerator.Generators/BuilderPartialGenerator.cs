using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class BuilderPartialGenerator : PartialGeneratorBase
    {
        protected BuilderPartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new BuilderPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override Features TriggeringFeatures => Features.Builder;

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return
                SingletonList<MemberDeclarationSyntax>(
                    GenerateToBuilderMethod())
                .Add(GenerateBuilder());
        }

        private ClassDeclarationSyntax GenerateBuilder()
        {
            return
                ClassDeclaration(Names.Builder)
                .AddModifiers(SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
                .WithMembers(GenerateBuilderMembers());

            SyntaxList<MemberDeclarationSyntax> GenerateBuilderMembers()
            {
                return List<MemberDeclarationSyntax>()
                    .AddRange(Descriptor.Entries.SelectMany(GetPropertyMembers))
                    .Add(GetBuilderToImmutableMethod());
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GetPropertyMembers(RecordDescriptor.Entry entry)
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

        private MethodDeclarationSyntax GetBuilderToImmutableMethod()
        {
            return
                MethodDeclaration(
                    Descriptor.Type,
                    Names.ToImmutable)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithBody(
                    Block(
                        ReturnStatement(
                            ObjectCreationExpression(Descriptor.Type)
                            .WithArgumentList(
                                CreateArgumentList()))));
            ArgumentListSyntax CreateArgumentList()
            {
                return ArgumentList(
                        SeparatedList(
                            Descriptor.Entries.Select(
                                entry => Argument(IdentifierName(entry.Identifier)))));
            }
        }

        private MethodDeclarationSyntax GenerateToBuilderMethod()
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
                                    Descriptor.Entries.Select(CreateInitializerForEntry)))));
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
