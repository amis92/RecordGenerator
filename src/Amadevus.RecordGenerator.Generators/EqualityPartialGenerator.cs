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
        private const string equalsMethodName = "Equals";
        private readonly RecordDescriptor descriptor;

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
            var systemNamespace = IdentifierName(Names.SystemNamespace);
            var equatableGeneric = GenericName(Names.IEquatableName)
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(new TypeSyntax[] { ClassIdentifier })));
            var equatable = QualifiedName(systemNamespace, equatableGeneric);

            return BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(equatable)));
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new MemberDeclarationSyntax[]
            {
                GenerateGenericEquals(),
                GenerateObjectEquals(),
                GenerateGetHashCode(),
            });
        }

        public MemberDeclarationSyntax GenerateGenericEquals()
        {
            const string objVariableName = "obj";

            var method = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.BoolKeyword)),
                equalsMethodName);

            method = method.AddModifiers(new [] 
            {
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.OverrideKeyword)
            });

            method = method.WithParameters( 
                Parameter(Identifier(objVariableName))
                .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword))));

            var asExpression = BinaryExpression(SyntaxKind.AsExpression,
                IdentifierName(objVariableName),
                ClassIdentifier);
            var equotableEqualsInvocation = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                ThisExpression(), IdentifierName(equalsMethodName)))
                .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(asExpression) })));
            var body = Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(equotableEqualsInvocation)));

            method = method.WithBody(body);

            return method;
        }

        public MemberDeclarationSyntax GenerateObjectEquals()
        {
            const string otherVariableName = "other";

            var method = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.BoolKeyword)),
                equalsMethodName);

            method = method.AddModifiers(new[]
            {
                Token(SyntaxKind.PublicKeyword)
            });

            method = method.WithParameters(
                Parameter(Identifier(otherVariableName))
                .WithType(ClassIdentifier));

            var equalsExpressions = Entries.Select(property =>
            {
                return InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    GenerateEqualityComparerDefaultExpression(property.Type), IdentifierName(equalsMethodName)))
                .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] 
                {
                    Argument(IdentifierName(property.Identifier.Text)),
                    Token(SyntaxKind.CommaToken),
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(otherVariableName),
                        IdentifierName(property.Identifier.Text))),
                })));
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

            method = method.WithBody(body);

            return method;
        }

        public MemberDeclarationSyntax GenerateGetHashCode()
        {
            const string varKeyWord = "var";
            const string getHashCodeMethodName = "GetHashCode";
            const string hashCodeVariableName = "hashCode";
            const int hashCodeInitialValue = 2085527896;
            const int hashCodeMultiplicationValue = 1521134295;

            var method = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.IntKeyword)),
                Identifier(getHashCodeMethodName));

            method = method.AddModifiers(
                SyntaxKind.PublicKeyword,
                SyntaxKind.OverrideKeyword);

            var body = Block();

            body = body.AddStatements(LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(varKeyWord))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier(hashCodeVariableName))
                    .WithInitializer(
                        EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(hashCodeInitialValue))))))));

            foreach (var property in Entries)
            {
                var defaultEqualityComparer = GenerateEqualityComparerDefaultExpression(property.Type);

                var getHashCodeInvocation = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        defaultEqualityComparer, IdentifierName(getHashCodeMethodName)))
                    .WithArgumentList(ArgumentList(SeparatedList(new [] { Argument(IdentifierName(property.Identifier.Text)) })));

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

                body = body.AddStatements(hashCodeAssignment);
            }

            body = body.AddStatements(ReturnStatement(
                IdentifierName(hashCodeVariableName)));

            body = Block(SingletonList<StatementSyntax>(
                CheckedStatement(SyntaxKind.UncheckedStatement, body)));

            method = method.WithBody(body);

            return method;
        }

        private MemberAccessExpressionSyntax GenerateEqualityComparerDefaultExpression(TypeSyntax comparerType)
        {
            string defaultMemberName = "Default";

            var equalityComparerQN = ParseName(Names.SystemCollectionsGenericNamespace);
            var equalityComparerGeneric = GenericName(Names.EqualityComparerName)
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(new[] { comparerType })));
            var equalityComparer = QualifiedName(equalityComparerQN, equalityComparerGeneric);

            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                equalityComparer, IdentifierName(defaultMemberName));
        }
    }
}
