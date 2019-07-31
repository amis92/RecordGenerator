using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class DeconstructPartialGenerator
    {
        public static IPartialGenerator Instance =>
            PartialGenerator.Member(Features.Deconstruct, GenerateDeconstruct);

        private static MemberDeclarationSyntax GenerateDeconstruct(RecordDescriptor descriptor)
        {
            return
                MethodDeclaration(
                        PredefinedType(Token(SyntaxKind.VoidKeyword)), Names.Deconstruct)
                    .AddModifiers(SyntaxKind.PublicKeyword)
                    .WithParameters(
                        descriptor.Entries.Select(CreateParameter))
                    .WithBodyStatements(
                        descriptor.Entries.Select(CreateAssignment));
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
