using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class EqualityPartialGenerator : PartialGeneratorBase
    {
        private readonly RecordDescriptor descriptor;
        private readonly EqualityPartialCodeGenerator generator = new EqualityPartialCodeGenerator();

        public EqualityPartialGenerator(
            RecordDescriptor descriptor,
            CancellationToken cancellationToken) 
            : base(descriptor, cancellationToken)
        {
            this.descriptor = descriptor;
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new EqualityPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        private NameSyntax ClassIdentifier => SyntaxExtensions.GetTypeSyntax(descriptor.TypeDeclaration);
        private IEnumerable<RecordDescriptor.Entry> Entries => descriptor.Entries;

        protected override BaseListSyntax GenerateBaseList()
        {
            return generator.GenerateBaseListSyntax(ClassIdentifier);
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new MemberDeclarationSyntax[]
            {
                generator.GenerateGenericEquals(ClassIdentifier),
                generator.GenerateObjectEquals(ClassIdentifier, Entries),
                generator.GenerateGetHashCode(Entries),
            });
        }
    }

    internal sealed class EqualityPartialCodeGenerator
    {
        private const string equalsMethodName = "Equals";

        public BaseListSyntax GenerateBaseListSyntax(NameSyntax identifier)
        {
            return BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(
                        QualifiedNameGenerator.GenerateGenericQualifiedName(
                            Names.IEquotableQualifiedName, new[] { identifier }))));
        }
       
        public MemberDeclarationSyntax GenerateGenericEquals(NameSyntax identifier)
        {
            const string objVariableName = "obj";

            var methodBuilder = new MethodBuilder();
            methodBuilder.Identifier = equalsMethodName;
            methodBuilder.ReturnType = PredefinedType(Token(SyntaxKind.BoolKeyword));

            methodBuilder.Modifiers.Add(SyntaxKind.PublicKeyword);
            methodBuilder.Modifiers.Add(SyntaxKind.OverrideKeyword);

            methodBuilder.Parameters.Add(
                objVariableName, PredefinedType(Token(SyntaxKind.ObjectKeyword)));

            var asExpression = BinaryExpression(SyntaxKind.AsExpression,
                IdentifierName(objVariableName),
                identifier);
            var equotableEqualsInvocation = MemberAccessGenerator.GenerateInvocation(
                ThisExpression(),
                equalsMethodName,
                new[] { asExpression });
            var body = Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(equotableEqualsInvocation)));

            methodBuilder.Body = body;

            return methodBuilder.Create();
        }

        public MemberDeclarationSyntax GenerateObjectEquals(
            NameSyntax identifierName,
            IEnumerable<RecordDescriptor.Entry> propertiesToCompare)
        {
            const string otherVariableName = "other";

            var methodBuilder = new MethodBuilder();
            methodBuilder.Identifier = equalsMethodName;
            methodBuilder.ReturnType = PredefinedType(Token(SyntaxKind.BoolKeyword));
            methodBuilder.Modifiers.Add(SyntaxKind.PublicKeyword);
            methodBuilder.Parameters.Add(otherVariableName, identifierName);

            var equalsExpressions = propertiesToCompare.Select(property =>
            {
                return MemberAccessGenerator.GenerateInvocation(
                    GenerateEqualityComparerDefaultExpression(property.Type),
                    equalsMethodName,
                    new ExpressionSyntax[]
                    {
                        IdentifierName(property.Identifier.Text),
                        MemberAccessGenerator.GenerateMemberAccess(
                            IdentifierName(otherVariableName),
                            property.Identifier.Text),
                    }); 
            }).Cast<ExpressionSyntax>().ToList();

            var notNullCheck = BinaryExpression(SyntaxKind.NotEqualsExpression,
                IdentifierName(otherVariableName),
                LiteralExpression(SyntaxKind.NullLiteralExpression));

            equalsExpressions.Insert(0, notNullCheck);

            var checks = notNullCheck;  
            foreach (var equalsExpression in equalsExpressions)
            {
                checks = BinaryExpression(SyntaxKind.LogicalAndExpression,
                    checks,
                    equalsExpression); 
            }

            var body = Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(checks)));

            methodBuilder.Body = body;

            return methodBuilder.Create();
        }

        public MemberDeclarationSyntax GenerateGetHashCode(
            IEnumerable<RecordDescriptor.Entry> propertiesToCompare)
        {
            const string getHashCodeMethodName = "GetHashCode";
            const string hashCodeVariableName = "hashCode";
            const int hashCodeInitialValue = 2085527896;
            const int hashCodeMultiplicationValue = 1521134295;

            var methodBuilder = new MethodBuilder();

            methodBuilder.Identifier = getHashCodeMethodName;
            methodBuilder.ReturnType = PredefinedType(Token(SyntaxKind.IntKeyword));

            methodBuilder.Modifiers.Add(SyntaxKind.PublicKeyword);
            methodBuilder.Modifiers.Add(SyntaxKind.OverrideKeyword);

            var body = Block();

            body.AddStatements(
                localVariableDeclarationGenerator.GenerateLocalVariableDeclaration(
                    hashCodeVariableName,
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(hashCodeInitialValue))));

            foreach (var property in propertiesToCompare)
            {
                var defaultEqualityComparer = GenerateEqualityComparerDefaultExpression(property.Type); 
                var getHashCodeInvocation = MemberAccessGenerator.GenerateInvocation(
                    defaultEqualityComparer,
                    getHashCodeMethodName,
                    new[] { IdentifierName(property.Identifier.Text) });

                var hashCodeAssignment = ExpressionStatement(
                    AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(hashCodeVariableName),
                        BinaryExpression(SyntaxKind.AddExpression,
                            BinaryExpression(SyntaxKind.MultiplyExpression,
                                IdentifierName(hashCodeVariableName),
                                PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression,
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                        Literal(hashCodeMultiplicationValue)))),
                            getHashCodeInvocation)));

                body.AddStatements(hashCodeAssignment);
            }

            body.AddStatements(ReturnStatement(
                IdentifierName(hashCodeVariableName)));

            body = Block(SingletonList<StatementSyntax>(
                CheckedStatement(SyntaxKind.UncheckedStatement, body)));

            methodBuilder.Body = body;

            return methodBuilder.Create();
        }

        private MemberAccessExpressionSyntax GenerateEqualityComparerDefaultExpression(TypeSyntax comparerType)
        {
            var equalityComparerAccess = QualifiedNameGenerator.GenerateGenericQualifiedName(
                Names.EqualityCompararerQualifiedName, new[] { comparerType });
            return MemberAccessGenerator.GenerateMemberAccess(
                equalityComparerAccess, "Default");
        }

    }
}
