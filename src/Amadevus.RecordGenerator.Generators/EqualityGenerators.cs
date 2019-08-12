using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Amadevus.RecordGenerator.Analyzers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class EqualityPartialGenerator
    {
        private const string EqualsMethodName = "Equals";
        private const string GetHashCodeMethodName = "GetHashCode";

        private static readonly ImmutableArray<string> OperatorEqualityTypes =
            ImmutableArray.CreateRange(
                from t in new[]
                {
                    typeof(bool),
                    typeof(sbyte), typeof(byte),
                    typeof(char),
                    typeof(int), typeof(uint),
                    typeof(long), typeof(ulong),
                    typeof(short), typeof(ushort),
                    typeof(string)
                }
                select t.FullName);

        private static MemberAccessExpressionSyntax GenerateEqualityComparerDefaultExpression(TypeSyntax comparedType)
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

        private static ExpressionSyntax[] GenerateEqualsExpressions(RecordDescriptor descriptor, string otherVariableName)
        {
            // either
            // Prop == other.Prop
            // or
            // EqualityComparer<TProp>.Default.Equals(Prop, other.Prop)
            return descriptor.Entries.Select(GenerateEqualityCheck).ToArray();

            ExpressionSyntax GenerateEqualityCheck(RecordDescriptor.Entry property)
            {
                var typeQualifiedName = property.QualifiedTypeName.TrimEnd('?');
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
                            GenerateEqualityComparerDefaultExpression(property.TypeSyntax),
                            IdentifierName(EqualsMethodName)))
                    .AddArgumentListArguments(
                        Argument(thisMemberValueAccess),
                        Argument(otherMemberValueAccess));
            }
        }

        private const string objVariableName = "obj";

        public static readonly IPartialGenerator ObjectEqualsGenerator =
            PartialGenerator.Create(Features.ObjectEquals,
                (descriptor, features) =>
                    descriptor.IsTypeSealed
                    ? PartialGenerationResult.Empty
                      .AddMembers(GenerateObjectEqualsSignature()
                                  .WithBody(features.HasFlag(Features.EquatableEquals)
                                            ? GenerateForwardToEquatableEquals(descriptor)
                                            : GenerateStandaloneObjectEquals(descriptor)),
                                  GenerateGetHashCode(descriptor))
                    : PartialGenerationResult.Empty
                      .AddDiagnostic(
                          descriptor.CreateDiagnostic(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled)));

        private static MethodDeclarationSyntax GenerateObjectEqualsSignature()
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

        private static BlockSyntax GenerateStandaloneObjectEquals(RecordDescriptor descriptor)
        {
            // return obj is MyRecord other && PropA == other.PropB && ...;
            const string otherVariableName = "other";
            return
                Block(
                    ReturnStatement(
                        GenerateEqualsExpressions(descriptor, otherVariableName)
                        .Aggregate(
                            (ExpressionSyntax)IsPatternExpression(
                                IdentifierName(objVariableName),
                                DeclarationPattern(
                                    descriptor.TypeSyntax,
                                    SingleVariableDesignation(
                                        Identifier(otherVariableName)))),
                            (prev, next) => BinaryExpression(LogicalAndExpression, prev, next))));
        }

        private static BlockSyntax GenerateForwardToEquatableEquals(RecordDescriptor descriptor)
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
                                    descriptor.TypeSyntax)))));
        }

        private static MemberDeclarationSyntax GenerateGetHashCode(RecordDescriptor descriptor)
        {
            const string varKeyWord = "var";
            const string hashCodeVariableName = "hashCode";
            const int hashCodeInitialValue = 2085527896;
            const int hashCodeMultiplicationValue = 1521134295;
            var statement = descriptor.Entries.Length == 1
                ? SinglePropertyGetHashCode()
                : MultiplePropertiesGetHashCode();
            return
                MethodDeclaration(
                    PredefinedType(
                        Token(IntKeyword)),
                    Identifier(GetHashCodeMethodName))
                .AddModifiers(
                    PublicKeyword,
                    OverrideKeyword)
                .AddBodyStatements(statement);
            
            StatementSyntax SinglePropertyGetHashCode()
            {
                // public override int GetHashCode() {
                //   return EqualityComparer<TProp>.Default.GetHashCode(Prop);
                // }
                return
                    ReturnStatement(
                        HashCodeInvocation(descriptor.Entries[0]));
            }

            StatementSyntax MultiplePropertiesGetHashCode()
            {
                // public override int GetHashCode() {
                //   var hashCode = 2085527896;
                //   hashCode = hashCode * -1521134295 + EqualityComparer<TProp>.Default.GetHashCode(Prop);
                //   ...
                //   return hashCode;
                // }
                return
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
                        descriptor.Entries.Select(EntryHashCodeRecalculation)
                        .ToArray())
                    .AddBlockStatements(
                        ReturnStatement(
                            IdentifierName(hashCodeVariableName)));
            }

            StatementSyntax EntryHashCodeRecalculation(RecordDescriptor.Entry property)
            {
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
                                HashCodeInvocation(property))));
            }

            InvocationExpressionSyntax HashCodeInvocation(RecordDescriptor.Entry property)
            {
                // EqualityComparer<TProp>.Default.GetHashCode(prop);

                var defaultEqualityComparer = GenerateEqualityComparerDefaultExpression(property.TypeSyntax);

                return
                    InvocationExpression(
                        MemberAccessExpression(
                            SimpleMemberAccessExpression,
                            defaultEqualityComparer,
                            IdentifierName(GetHashCodeMethodName)))
                    .AddArgumentListArguments(
                        Argument(
                            IdentifierName(property.Identifier.Text)));
            }
        }

        public static readonly IPartialGenerator EquatableEqualsPartialGenerator =
            PartialGenerator.Create(Features.EquatableEquals,
                descriptor =>
                {
                    if (!descriptor.IsTypeSealed)
                        return PartialGenerationResult.Empty
                               .AddDiagnostic(descriptor.CreateDiagnostic(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled));

                    // MyRecord : System.IEquatable<MyRecord>
                    var equatable =
                        QualifiedName(
                            IdentifierName(Names.SystemNamespace),
                            GenericName(Names.IEquatableName)
                            .AddTypeArgumentListArguments(descriptor.TypeSyntax));

                    return PartialGenerationResult.Empty
                           .WithBaseList(
                               ImmutableArray.Create<BaseTypeSyntax>(SimpleBaseType(equatable)))
                           .AddMember(
                               GenerateEquatableEquals(descriptor));
                });

        private static MemberDeclarationSyntax GenerateEquatableEquals(RecordDescriptor descriptor)
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
                    .WithType(descriptor.TypeSyntax))
                .AddBodyStatements(
                    ReturnStatement(
                        GenerateEqualsExpressions(descriptor, otherVariableName)
                        .Aggregate(
                            BinaryExpression(
                                NotEqualsExpression,
                                IdentifierName(otherVariableName),
                                LiteralExpression(NullLiteralExpression)),
                            (prev, next) => BinaryExpression(LogicalAndExpression, prev, next))));
        }

        private const string rightVariableName = "right";
        private const string leftVariableName = "left";

        public static readonly IPartialGenerator OperatorEqualityPartialGenerator =
            PartialGenerator.Create(Features.OperatorEquals,
                descriptor =>
                    PartialGenerationResult.Empty
                    .AddMembers(GenerateOperatorDeclarationBase(descriptor, EqualsEqualsToken)
                                .WithBody(
                                    GenerateEqualsEqualsBody(descriptor)),
                                GenerateOperatorDeclarationBase(descriptor, ExclamationEqualsToken)
                                .WithBody(
                                    GenerateExclamationEqualsBody())));

        private static OperatorDeclarationSyntax GenerateOperatorDeclarationBase(RecordDescriptor descriptor, SyntaxKind token)
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
                    .WithType(descriptor.TypeSyntax),
                    Parameter(
                        Identifier(rightVariableName))
                    .WithType(descriptor.TypeSyntax));
        }

        private static BlockSyntax GenerateExclamationEqualsBody()
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

        private static BlockSyntax GenerateEqualsEqualsBody(RecordDescriptor descriptor)
        {
            // return System.Collections.Generic.EqualityComparer<MyRecord>.Default.Equals(left, right);
            return
                Block(
                    ReturnStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                GenerateEqualityComparerDefaultExpression(descriptor.TypeSyntax),
                                IdentifierName(EqualsMethodName)))
                        .AddArgumentListArguments(
                            Argument(
                                IdentifierName(leftVariableName)),
                            Argument(
                                IdentifierName(rightVariableName)))));
        }
    }
}
