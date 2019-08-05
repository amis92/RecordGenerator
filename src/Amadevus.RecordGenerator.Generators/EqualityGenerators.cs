using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Amadevus.RecordGenerator.Generators
{
    internal abstract class EqualityPartialGeneratorBase : PartialGeneratorBase
    {
        protected const string EqualsMethodName = "Equals";
        protected const string GetHashCodeMethodName = "GetHashCode";

        protected ImmutableArray<string> OperatorEqualityTypes =>
            new[] { typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(int), typeof(uint),
                typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(string) }
            .Select(t => t.FullName).ToImmutableArray();

        protected EqualityPartialGeneratorBase(RecordDescriptor descriptor, CancellationToken cancellationToken)
            : base(descriptor, cancellationToken) { }

        protected MemberAccessExpressionSyntax GenerateEqualityComparerDefaultExpression(TypeSyntax comparedType)
        {
            // System.Collections.Generic.EqualityComparer<[comparedType]>.Default
            const string defaultMemberName = "Default";
            var equalityComparer =
                QualifiedName(
                    ParseName(Names.SystemCollectionsGenericNamespace),
                    GenericName(Names.EqualityComparerName)
                    .AddTypeArgumentListArguments(comparedType));

            return MemberAccessExpression(
                SimpleMemberAccessExpression,
                equalityComparer,
                IdentifierName(defaultMemberName));
        }

        protected ExpressionSyntax[] GenerateEqualsExpressions(string otherVariableName)
        {
            // either
            // Prop == other.Prop
            // or
            // EqualityComparer<TProp>.Default.Equals(Prop, other.Prop)
            return Descriptor.Entries.Select(GenerateEqualityCheck).ToArray();

            ExpressionSyntax GenerateEqualityCheck(RecordDescriptor.Entry property)
            {
                var typeQualifiedName = property.TypeSymbol.GetQualifiedName().TrimEnd('?');
                var thisMemberValueAccess = IdentifierName(property.Identifier.Text);
                var otherMemberValueAccess =
                    MemberAccessExpression(
                        SimpleMemberAccessExpression,
                        IdentifierName(otherVariableName),
                        thisMemberValueAccess);

                if (OperatorEqualityTypes.Contains(typeQualifiedName))
                {
                    return BinaryExpression(EqualsExpression, thisMemberValueAccess, otherMemberValueAccess);
                }
                return
                    InvocationExpression(
                        MemberAccessExpression(
                            SimpleMemberAccessExpression,
                            GenerateEqualityComparerDefaultExpression(property.Type),
                            IdentifierName(EqualsMethodName)))
                    .AddArgumentListArguments(
                        Argument(thisMemberValueAccess),
                        Argument(otherMemberValueAccess));
            }
        }
    }

    internal sealed class ObjectEqualsGenerator : EqualityPartialGeneratorBase
    {
        private const string objVariableName = "obj";

        public ObjectEqualsGenerator(RecordDescriptor descriptor, CancellationToken token)
            : base(descriptor, token) { }

        protected override Features TriggeringFeatures => Features.ObjectEquals;

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            return new ObjectEqualsGenerator(descriptor, cancellationToken).GenerateTypeDeclaration();
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new[]
            {
                GenerateObjectEqualsSignature()
                .WithBody(
                    Descriptor.Features.HasFlag(Features.EquatableEquals)
                        ? GenerateForwardToEquatableEquals()
                        : GenerateStandaloneObjectEquals()),
                GenerateGetHashCode()
            });
        }

        public MethodDeclarationSyntax GenerateObjectEqualsSignature()
        {
            // public override bool Equals(object obj)
            return
                MethodDeclaration(
                    PredefinedType(
                        Token(BoolKeyword)),
                    EqualsMethodName)
                .AddModifiers(
                    Token(PublicKeyword),
                    Token(OverrideKeyword))
                .AddParameterListParameters(
                    Parameter(
                        Identifier(objVariableName))
                    .WithType(
                        PredefinedType(
                            Token(ObjectKeyword))));
        }

        public BlockSyntax GenerateStandaloneObjectEquals()
        {
            // return obj is MyRecord other && PropA == other.PropB && ...;
            const string otherVariableName = "other";
            return
                Block(
                    ReturnStatement(
                        GenerateEqualsExpressions(otherVariableName)
                        .Aggregate(
                            (ExpressionSyntax)IsPatternExpression(
                                IdentifierName(objVariableName),
                                DeclarationPattern(
                                    Descriptor.Type,
                                    SingleVariableDesignation(
                                        Identifier(otherVariableName)))),
                            (prev, next) => BinaryExpression(LogicalAndExpression, prev, next))));
        }

        public BlockSyntax GenerateForwardToEquatableEquals()
        {
            // return this.Equals(obj as MyRecord);
            return
                Block(
                    ReturnStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName(EqualsMethodName)))
                        .AddArgumentListArguments(
                            Argument(
                                BinaryExpression(
                                    AsExpression,
                                    IdentifierName(objVariableName),
                                    Descriptor.Type)))));
        }

        public MemberDeclarationSyntax GenerateGetHashCode()
        {
            const string varKeyWord = "var";
            const string hashCodeVariableName = "hashCode";
            const int hashCodeInitialValue = 2085527896;
            const int hashCodeMultiplicationValue = 1521134295;
            // public override int GetHashCode() {
            //   var hashCode = 2085527896;
            //   hashCode = hashCode * -1521134295 + EqualityComparer<TProp>.Default.GetHashCode(Prop);
            //   ...
            //   return hashCode;
            // }
            return
                MethodDeclaration(
                    PredefinedType(
                        Token(IntKeyword)),
                    Identifier(GetHashCodeMethodName))
                .AddModifiers(
                    PublicKeyword,
                    OverrideKeyword)
                .AddBodyStatements(
                    CheckedStatement(UncheckedStatement)
                    .AddBlockStatements(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName(varKeyWord))
                            .AddVariables(
                                VariableDeclarator(
                                    Identifier(hashCodeVariableName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(
                                            NumericLiteralExpression,
                                            Literal(hashCodeInitialValue)))))))
                    .AddBlockStatements(
                        Descriptor.Entries.Select(EntryHashCodeRecalculation)
                        .ToArray())
                    .AddBlockStatements(
                        ReturnStatement(
                            IdentifierName(hashCodeVariableName))));

            StatementSyntax EntryHashCodeRecalculation(RecordDescriptor.Entry property)
            {
                var defaultEqualityComparer = GenerateEqualityComparerDefaultExpression(property.Type);

                var getHashCodeInvocation =
                    InvocationExpression(
                        MemberAccessExpression(
                            SimpleMemberAccessExpression,
                            defaultEqualityComparer,
                            IdentifierName(GetHashCodeMethodName)))
                    .AddArgumentListArguments(
                        Argument(
                            IdentifierName(property.Identifier.Text)));
                // hashCode = hashCode * -1521134295 + EqualityComparer<TProp>.Default.GetHashCode(prop);
                return
                    ExpressionStatement(
                        AssignmentExpression(
                            SimpleAssignmentExpression,
                            IdentifierName(hashCodeVariableName),
                            BinaryExpression(
                                AddExpression,
                                BinaryExpression(
                                    MultiplyExpression,
                                    IdentifierName(hashCodeVariableName),
                                    PrefixUnaryExpression(
                                        UnaryMinusExpression,
                                        LiteralExpression(
                                            NumericLiteralExpression,
                                            Literal(hashCodeMultiplicationValue)))),
                                getHashCodeInvocation)));
            }
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
            // MyRecord : System.IEquatable<MyRecord>
            var equatable =
                QualifiedName(
                    IdentifierName(Names.SystemNamespace),
                    GenericName(Names.IEquatableName)
                    .AddTypeArgumentListArguments(Descriptor.Type));
            return
                BaseList(
                    SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(equatable)));
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return SingletonList(GenerateEquatableEquals());
        }

        public MemberDeclarationSyntax GenerateEquatableEquals()
        {
            // public bool Equals(MyRecord other) {
            //   return other != null && PropA == other.PropA && ...;
            // }
            const string otherVariableName = "other";
            return
                MethodDeclaration(
                    PredefinedType(
                        Token(BoolKeyword)),
                    EqualsMethodName)
                .AddModifiers(
                    Token(PublicKeyword))
                .AddParameterListParameters(
                    Parameter(
                        Identifier(otherVariableName))
                    .WithType(Descriptor.Type))
                .AddBodyStatements(
                    ReturnStatement(
                        GenerateEqualsExpressions(otherVariableName)
                        .Aggregate(
                            BinaryExpression(
                                NotEqualsExpression,
                                IdentifierName(otherVariableName),
                                LiteralExpression(NullLiteralExpression)),
                            (prev, next) => BinaryExpression(LogicalAndExpression, prev, next))));
        }
    }

    internal sealed class OperatorEqualityPartialGenerator : EqualityPartialGeneratorBase
    {
        private const string rightVariableName = "right";
        private const string leftVariableName = "left";

        public OperatorEqualityPartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken)
            : base(descriptor, cancellationToken) { }

        protected override Features TriggeringFeatures => Features.OperatorEquals;

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            return new OperatorEqualityPartialGenerator(descriptor, cancellationToken).GenerateTypeDeclaration();
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List(new MemberDeclarationSyntax[]
            {
                GenerateOperatorDeclarationBase(EqualsEqualsToken)
                .WithBody(
                    GenerateEqualsEqualsBody()),
                GenerateOperatorDeclarationBase(ExclamationEqualsToken)
                .WithBody(
                    GenerateExclamationEqualsBody()),
            });
        }

        private OperatorDeclarationSyntax GenerateOperatorDeclarationBase(SyntaxKind token)
        {
            // public static bool operator [token](MyRecord left, MyRecord right)
            return
                OperatorDeclaration(
                    PredefinedType(
                        Token(BoolKeyword)),
                    Token(token))
                .AddModifiers(
                    Token(PublicKeyword),
                    Token(StaticKeyword))
                .AddParameterListParameters(
                    Parameter(
                        Identifier(leftVariableName))
                    .WithType(Descriptor.Type),
                    Parameter(
                        Identifier(rightVariableName))
                    .WithType(Descriptor.Type));
        }

        private BlockSyntax GenerateExclamationEqualsBody()
        {
            // return !(left == right);
            return
                Block(
                    ReturnStatement(
                        PrefixUnaryExpression(
                            LogicalNotExpression,
                            ParenthesizedExpression(
                                BinaryExpression(
                                    EqualsExpression,
                                    IdentifierName(leftVariableName),
                                    IdentifierName(rightVariableName))))));
        }

        private BlockSyntax GenerateEqualsEqualsBody()
        {
            // return System.Collections.Generic.EqualityComparer<MyRecord>.Default.Equals(left, right);
            return
                Block(
                    ReturnStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                GenerateEqualityComparerDefaultExpression(Descriptor.Type),
                                IdentifierName(EqualsMethodName)))
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(leftVariableName)),
                            Argument(
                                IdentifierName(rightVariableName)))));
        }
    }
}
