using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class EqualityPartialGenerator : PartialGeneratorBase
    {
        private const string equalsMethodName = "Equals";
        private readonly RecordDescriptor descriptor;
        private IEnumerable<string> wellKnownTypes { get; } =
            new [] { BoolKeyword, ByteKeyword, SByteKeyword, CharKeyword, IntKeyword, UIntKeyword, LongKeyword, ULongKeyword, ShortKeyword, UShortKeyword, StringKeyword }
            .Select(k => k.ToString().ToLower()).Select(k => k.Substring(0, k.Length - "Keyword".Length)).ToArray();

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
                GenerateObjectEquals(),
                GenerateGenericEquals(),
                GenerateGetHashCode(),
            });
        }

        public MemberDeclarationSyntax GenerateObjectEquals()
        {
            const string objVariableName = "obj";

            var method = MethodDeclaration(
                PredefinedType(Token(BoolKeyword)),
                equalsMethodName);

            method = method.AddModifiers(new [] 
            {
                Token(PublicKeyword),
                Token(OverrideKeyword)
            });

            method = method.WithParameters( 
                Parameter(Identifier(objVariableName))
                .WithType(PredefinedType(Token(ObjectKeyword))));

            var asExpression = BinaryExpression(AsExpression,
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

        public MemberDeclarationSyntax GenerateGenericEquals()
        {
            const string otherVariableName = "other";

            var method = MethodDeclaration(
                PredefinedType(Token(BoolKeyword)),
                equalsMethodName);

            method = method.AddModifiers(new[]
            {
                Token(PublicKeyword)
            });

            method = method.WithParameters(
                Parameter(Identifier(otherVariableName))
                .WithType(ClassIdentifier));

            var equalsExpressions = Entries.Select<RecordDescriptor.Entry, ExpressionSyntax>(property =>
            {
                var otherMemberValueAccess = MemberAccessExpression(SimpleMemberAccessExpression,
                    IdentifierName(otherVariableName),
                    IdentifierName(property.Identifier.Text));
                var thisMemberValueAccess = IdentifierName(property.Identifier.Text);

                System.Diagnostics.Debugger.Launch();

                if (property.Type is PredefinedTypeSyntax predefinedType)
                {
                    if (wellKnownTypes.Contains(predefinedType.Keyword.ValueText))
                        return BinaryExpression(EqualsExpression, thisMemberValueAccess, otherMemberValueAccess);
                }

                return InvocationExpression(MemberAccessExpression(SimpleMemberAccessExpression,
                    GenerateEqualityComparerDefaultExpression(property.Type), IdentifierName(equalsMethodName)))
                .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] 
                {
                    Argument(thisMemberValueAccess),
                    Token(CommaToken),
                    Argument(otherMemberValueAccess),
                })));
            }).Cast<ExpressionSyntax>().ToList();

            var notNullCheck = BinaryExpression(NotEqualsExpression,
                IdentifierName(otherVariableName),
                LiteralExpression(NullLiteralExpression));

            var checks = notNullCheck;  
            foreach (var equalsExpression in equalsExpressions)
            {
                checks = BinaryExpression(LogicalAndExpression,
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
                PredefinedType(Token(IntKeyword)),
                Identifier(getHashCodeMethodName));

            method = method.AddModifiers(
                PublicKeyword,
                OverrideKeyword);

            var body = Block();

            body = body.AddStatements(LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(varKeyWord))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier(hashCodeVariableName))
                    .WithInitializer(
                        EqualsValueClause(LiteralExpression(NumericLiteralExpression, Literal(hashCodeInitialValue))))))));

            foreach (var property in Entries)
            {
                var defaultEqualityComparer = GenerateEqualityComparerDefaultExpression(property.Type);

                var getHashCodeInvocation = InvocationExpression(MemberAccessExpression(SimpleMemberAccessExpression,
                        defaultEqualityComparer, IdentifierName(getHashCodeMethodName)))
                    .WithArgumentList(ArgumentList(SeparatedList(new [] { Argument(IdentifierName(property.Identifier.Text)) })));

                var hashCodeAssignment = ExpressionStatement(
                    AssignmentExpression(SimpleAssignmentExpression,
                        IdentifierName(hashCodeVariableName),
                        BinaryExpression(AddExpression,
                            BinaryExpression(MultiplyExpression,
                                IdentifierName(hashCodeVariableName),
                                PrefixUnaryExpression(UnaryMinusExpression,
                                    LiteralExpression(NumericLiteralExpression,
                                        Literal(hashCodeMultiplicationValue)))),
                            getHashCodeInvocation)));

                body = body.AddStatements(hashCodeAssignment);
            }

            body = body.AddStatements(ReturnStatement(
                IdentifierName(hashCodeVariableName)));

            body = Block(SingletonList<StatementSyntax>(
                CheckedStatement(UncheckedStatement, body)));

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

            return MemberAccessExpression(SimpleMemberAccessExpression,
                equalityComparer, IdentifierName(defaultMemberName));
        }
    }
}
