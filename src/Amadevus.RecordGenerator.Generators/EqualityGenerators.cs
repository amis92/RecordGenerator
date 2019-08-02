using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace Amadevus.RecordGenerator.Generators
{
    internal abstract class EqualityPartialGeneratorBase : PartialGeneratorBase
    {
        protected const string EqualsMethodName = "Equals";
        protected const string GetHashCodeMethodName = "GetHashCode";

        protected ImmutableArray<string> OperatorEqualityTypes => new[] { typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(string) }
            .Select(t => t.FullName).ToImmutableArray();

        protected EqualityPartialGeneratorBase(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken) { }

        protected MemberAccessExpressionSyntax GenerateEqualityComparerDefaultExpression(TypeSyntax comparerType)
        {
            string defaultMemberName = "Default";

            var equalityComparerQN = ParseName(Names.SystemCollectionsGenericNamespace);
            var equalityComparerGeneric = GenericName(Names.EqualityComparerName)
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(new[] { comparerType })));
            var equalityComparer = QualifiedName(equalityComparerQN, equalityComparerGeneric);

            return MemberAccessExpression(SimpleMemberAccessExpression,
                equalityComparer, IdentifierName(defaultMemberName));
        }

        protected ExpressionSyntax[] GenerateEqualsExpressions(string otherVariableName)
        {

            return Descriptor.Entries.Select<RecordDescriptor.Entry, ExpressionSyntax>(property =>
            {
                var typeQualifiedName = property.TypeSymbol.GetQualifiedName().TrimEnd('?');

                var otherMemberValueAccess = MemberAccessExpression(SimpleMemberAccessExpression,
                    IdentifierName(otherVariableName),
                    IdentifierName(property.Identifier.Text));
                var thisMemberValueAccess = IdentifierName(property.Identifier.Text);

                if (OperatorEqualityTypes.Contains(typeQualifiedName))
                {
                    return BinaryExpression(EqualsExpression, thisMemberValueAccess, otherMemberValueAccess);
                }

                return InvocationExpression(MemberAccessExpression(SimpleMemberAccessExpression,
                    GenerateEqualityComparerDefaultExpression(property.Type),
                    IdentifierName(EqualsMethodName)))
                .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] 
                {
                    Argument(thisMemberValueAccess),
                    Token(CommaToken),
                    Argument(otherMemberValueAccess),
                })));
            }).Cast<ExpressionSyntax>().ToArray();
        }
    }

    internal sealed class ObjectEqualsGenerator : EqualityPartialGeneratorBase
    {
        private const string objVariableName = "obj";
        public ObjectEqualsGenerator(RecordDescriptor descriptor, CancellationToken token) : base(descriptor, token) { }

        protected override Features TriggeringFeatures => Features.ObjectEquals;

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            return new ObjectEqualsGenerator(descriptor, cancellationToken).GenerateTypeDeclaration();
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new [] 
            {
                GenerateObjectEquals(Descriptor.Features.HasFlag(Features.EquatableEquals) 
                    ? GenerateObjectEqualsWithCallToEquatableEquals()
                    : GenerateFullObjectEquals()),
                GenerateGetHashCode()
            });
        }

        public MemberDeclarationSyntax GenerateObjectEquals(BlockSyntax body)
        {
            var method = MethodDeclaration(
                PredefinedType(Token(BoolKeyword)),
                EqualsMethodName);
            
            method = method.AddModifiers(new [] 
            {
                Token(PublicKeyword),
                Token(OverrideKeyword)
            });

            method = method.WithParameters( 
                Parameter(Identifier(objVariableName))
                .WithType(PredefinedType(Token(ObjectKeyword))));

            method = method.WithBody(body);

            return method;
        }

        public BlockSyntax GenerateFullObjectEquals()
        {
            const string otherVariableName = "other";

            ExpressionSyntax notNullCheck = IsPatternExpression(
                IdentifierName(objVariableName),
                DeclarationPattern(
                    SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration),
                        SingleVariableDesignation(
                            Identifier(otherVariableName))));

            var checks = notNullCheck;  
            foreach (var equalsExpression in GenerateEqualsExpressions(otherVariableName))
            {
                checks = BinaryExpression(LogicalAndExpression, checks, equalsExpression); 
            }

            return Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(checks)));
        }

        public BlockSyntax GenerateObjectEqualsWithCallToEquatableEquals()
        {
            var asExpression = BinaryExpression(AsExpression,
                IdentifierName(objVariableName),
                SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration));

            var equotableEqualsInvocation = InvocationExpression(MemberAccessExpression(SimpleMemberAccessExpression,
                ThisExpression(), IdentifierName(EqualsMethodName)))
                .WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(asExpression) })));

            return Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(equotableEqualsInvocation)));
        }

        public MemberDeclarationSyntax GenerateGetHashCode()
        {
            const string varKeyWord = "var";
            const string hashCodeVariableName = "hashCode";
            const int hashCodeInitialValue = 2085527896;
            const int hashCodeMultiplicationValue = 1521134295;

            var method = MethodDeclaration(
                PredefinedType(Token(IntKeyword)),
                Identifier(GetHashCodeMethodName));

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

            foreach (var property in Descriptor.Entries)
            {
                var defaultEqualityComparer = GenerateEqualityComparerDefaultExpression(property.Type);

                var getHashCodeInvocation = InvocationExpression(MemberAccessExpression(SimpleMemberAccessExpression,
                        defaultEqualityComparer, IdentifierName(GetHashCodeMethodName)))
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
    }

    internal sealed class EquatableEqualsPartialGenerator : EqualityPartialGeneratorBase
    {
        public EquatableEqualsPartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken) { }

        protected override Features TriggeringFeatures => Features.EquatableEquals;

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            return new EquatableEqualsPartialGenerator(descriptor, cancellationToken).GenerateTypeDeclaration();
        }

        protected override BaseListSyntax GenerateBaseList()
        {
            var typeSyntax = SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration);
            var systemNamespace = IdentifierName(Names.SystemNamespace);
            var equatableGeneric = GenericName(Names.IEquatableName)
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(new TypeSyntax[] { typeSyntax })));
            var equatable = QualifiedName(systemNamespace, equatableGeneric);

            return BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(equatable)));
        }
        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new [] { GenerateGenericEquals() });
        }

        public MemberDeclarationSyntax GenerateGenericEquals()
        {
            const string otherVariableName = "other";

            var method = MethodDeclaration(
                PredefinedType(Token(BoolKeyword)),
                EqualsMethodName);

            method = method.AddModifiers(new[]
            {
                Token(PublicKeyword)
            });

            method = method.WithParameters(
                Parameter(Identifier(otherVariableName))
                .WithType(SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration)));

            var equalsExpressions = GenerateEqualsExpressions(otherVariableName);

            var notNullCheck = BinaryExpression(NotEqualsExpression,
                IdentifierName(otherVariableName),
                LiteralExpression(NullLiteralExpression));

            var checks = notNullCheck;  
            foreach (var equalsExpression in equalsExpressions)
            {
                checks = BinaryExpression(LogicalAndExpression, checks, equalsExpression); 
            }

            var body = Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(checks)));

            method = method.WithBody(body);

            return method;
        }
    }

    internal sealed class OperatorEqualityPartialGenerator : EqualityPartialGeneratorBase
    {
        private const string rightVariableName = "right";
        private const string leftVariableName = "left";

        public OperatorEqualityPartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken) { }

        protected override Features TriggeringFeatures => Features.OperatorEquals;

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            return new OperatorEqualityPartialGenerator(descriptor, cancellationToken).GenerateTypeDeclaration();
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new MemberDeclarationSyntax[] 
            {
                GenerateOperatorDeclarationBase(EqualsEqualsToken).WithBody(GenerateEqualsEqualsBody()),
                GenerateOperatorDeclarationBase(ExclamationEqualsToken).WithBody(GenerateExclamationEqualsBody()),
            });
        }

        private OperatorDeclarationSyntax GenerateOperatorDeclarationBase(SyntaxKind token)
        {
            var @operator = OperatorDeclaration(
                    PredefinedType(Token(BoolKeyword)),
                    Token(token));

            @operator = @operator.WithModifiers(TokenList(new[]{
                    Token(PublicKeyword),
                    Token(StaticKeyword)}));

            @operator = @operator.WithParameterList(ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]{
                            Parameter(Identifier(leftVariableName))
                            .WithType(SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration)),
                            Token(CommaToken),
                            Parameter(Identifier(rightVariableName))
                            .WithType(SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration))})));

            return @operator;
        }

        private BlockSyntax GenerateExclamationEqualsBody()
        {
            return Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(
                        PrefixUnaryExpression(LogicalNotExpression,
                            ParenthesizedExpression(
                                BinaryExpression(EqualsExpression,
                                    IdentifierName(leftVariableName),
                                    IdentifierName(rightVariableName)))))));
        }

        private BlockSyntax GenerateEqualsEqualsBody()
        {
            return Block(SingletonList<StatementSyntax>(
                ReturnStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            GenerateEqualityComparerDefaultExpression(SyntaxExtensions.GetTypeSyntax(Descriptor.TypeDeclaration)),
                            IdentifierName(EqualsMethodName)))
                    .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[] 
                    {
                        Argument(IdentifierName(leftVariableName)),
                        Token(CommaToken),
                        Argument(IdentifierName(rightVariableName)),
                    }))))));
        }
    }
}
