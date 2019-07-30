using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class DeconstructPartialGenerator : PartialGeneratorBase
    {
        protected DeconstructPartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new DeconstructPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override Features TriggeringFeatures => Features.Deconstruct;

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return SingletonList(GenerateDeconstruct());
        }

        private MemberDeclarationSyntax GenerateDeconstruct()
        {
            return
                MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)), Names.Deconstruct)
                .AddModifiers(SyntaxKind.PublicKeyword)
                .WithParameters(
                    Descriptor.Entries.Select(CreateParameter))
                .WithBodyStatements(
                    Descriptor.Entries.Select(CreateAssignment));
            ParameterSyntax CreateParameter(RecordDescriptor.Entry entry)
            {
                return
                    Parameter(entry.IdentifierInCamelCase)
                    .WithType(entry.Type)
                    .AddModifiers(Token(SyntaxKind.OutKeyword));
            }
            StatementSyntax CreateAssignment(RecordDescriptor.Entry entry)
            {
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(entry.IdentifierInCamelCase),
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(entry.Identifier))));
            }
        }
    }
}
