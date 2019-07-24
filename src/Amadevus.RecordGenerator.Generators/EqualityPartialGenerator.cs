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

        private NameSyntax ClassIdentifier
        {
            get => SyntaxExtensions.GetTypeSyntax(descriptor.TypeDeclaration);
        }

        private IEnumerable<RecordDescriptor.Entry> Entries
        {
            get => descriptor.Entries;
        }

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
        private const string varIdentifierName = "var";

        private const string equalityComparerName = "EqualityComparer";
        private const string equalityComparerDefaultProperty = "Default";

        public BaseListSyntax GenerateBaseListSyntax(NameSyntax identifier)
        {
            const string equotableInterfaceName = "System.IEquatable";

            return BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(
                        QualifiedNameGenerator.GenerateGenericQualifiedName(
                            equotableInterfaceName, new[] { identifier }))));
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

            var asExpression = BinaryExpression(
                SyntaxKind.AsExpression,
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
                var equalityComparerAccess = QualifiedNameGenerator.GenerateGenericQualifiedName(
                    "System.Collections.Generic.EqualityComparer", new[] { property.Type });
                var defaultEqualityComparerAccess = MemberAccessGenerator.GenerateMemberAccess(
                    equalityComparerAccess, "Default");

                return MemberAccessGenerator.GenerateInvocation(
                    defaultEqualityComparerAccess,
                    equalsMethodName,
                    new ExpressionSyntax[]
                    {
                        IdentifierName(property.Identifier.Text),
                        MemberAccessGenerator.GenerateMemberAccess(
                            IdentifierName(otherVariableName),
                            property.Identifier.Text),
                    }); 
            }).Cast<ExpressionSyntax>().ToList();

            var notNullCheck = BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                IdentifierName("other"),
                LiteralExpression(SyntaxKind.NullLiteralExpression));

            equalsExpressions.Insert(0, notNullCheck);

            var checks = notNullCheck;  
            foreach (var equalsExpression in equalsExpressions)
            {
                checks = BinaryExpression(
                    SyntaxKind.LogicalAndExpression,
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

            var hashCodeVariableDeclaration =
                localVariableDeclarationGenerator.GenerateLocalVariableDeclaration(
                    hashCodeVariableName,
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(hashCodeInitialValue)));

            var hashCodeAssignments = propertiesToCompare.Select(property =>
            {
                var equalityComparer = QualifiedNameGenerator.GenerateGenericQualifiedName(
                    "System.Collections.Generic.EqualityComparer", new[] { property.Type });
                var defaultEqualityComparer = MemberAccessGenerator.GenerateMemberAccess(
                    equalityComparer, "Default");
                var getHashCodeInvocation = MemberAccessGenerator.GenerateInvocation(
                    defaultEqualityComparer,
                    getHashCodeMethodName,
                    new[] { IdentifierName(property.Identifier.Text) });

                return ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(hashCodeVariableName),
                        BinaryExpression(
                            SyntaxKind.AddExpression,
                            BinaryExpression(
                                SyntaxKind.MultiplyExpression,
                                IdentifierName(hashCodeVariableName),
                                PrefixUnaryExpression(
                                    SyntaxKind.UnaryMinusExpression,
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(hashCodeMultiplicationValue)))),
                            getHashCodeInvocation)));
            }).Cast<StatementSyntax>();

            var returnStatement = ReturnStatement(
                IdentifierName(hashCodeVariableName));

            var innerBody = hashCodeAssignments.ToList();
            innerBody.Insert(0, hashCodeVariableDeclaration);
            innerBody.Add(returnStatement);

            var body = Block(
                SingletonList<StatementSyntax>(
                    CheckedStatement(
                        SyntaxKind.UncheckedStatement,
                        Block(innerBody))));

            methodBuilder.Body = body;

            return methodBuilder.Create();
        }
    }

}
