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
        private readonly EqualityPartialCodeGenerator generator;

        //private IdentifierNameSyntax ClassIdentifier => IdentifierName(descriptor.TypeIdentifier.Text);

        private NameSyntax ClassIdentifier
        {
            get => SyntaxExtensions.GetTypeSyntax(descriptor.TypeDeclaration);
        }

        private IEnumerable<RecordDescriptor.Entry> Entries => descriptor.Entries;

        public EqualityPartialGenerator(
            RecordDescriptor descriptor,
            CancellationToken cancellationToken) 
            : base(descriptor, cancellationToken)
        {
            this.descriptor = descriptor;
            this.generator = new EqualityPartialCodeGenerator(); 
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new EqualityPartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
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

        private const string systemNamespaceName = "System";
        private const string collectionsNamespaceName = "Collections";
        private const string genericNamespaceName = "Generic";
        private const string equalityComparerName = "EqualityComparer";
        private const string equalityComparerDefaultProperty = "Default";

        public BaseListSyntax GenerateBaseListSyntax(NameSyntax identifier)
        {
            const string equotableInterfaceName = "IEquatable";

            return BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(
                        QualifiedName(
                            IdentifierName(systemNamespaceName),
                            GenericName(
                                Identifier(equotableInterfaceName))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        identifier))))))); 
        }
       
        public MemberDeclarationSyntax GenerateGenericEquals(NameSyntax identifier)
        {
            const string objVariableName = "obj";

            var method = MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier(equalsMethodName));

            var modifiers = TokenList(
                new[] {
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)});

            var parameterList = ParameterList(
                SingletonSeparatedList(
                    Parameter(
                        Identifier(objVariableName))
                    .WithType(
                        PredefinedType(
                            Token(SyntaxKind.ObjectKeyword)))));

            var equotableEqualsInvocation = InvocationExpression(
                    IdentifierName(equalsMethodName))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                BinaryExpression(
                                    SyntaxKind.AsExpression,
                                    IdentifierName(objVariableName),
                                    identifier)))));

            var body = Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(equotableEqualsInvocation)));

            return method
                .WithModifiers(modifiers)
                .WithParameterList(parameterList)
                .WithBody(body);
        }

        public MemberDeclarationSyntax GenerateObjectEquals(
            NameSyntax identifierName,
            IEnumerable<RecordDescriptor.Entry> propertiesToCompare)
        {
            const string otherVariableName = "other";

            var method = MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier(equalsMethodName));

            var modifiers = TokenList(Token(SyntaxKind.PublicKeyword));

            var parameterList = ParameterList(
                SingletonSeparatedList(
                    Parameter(
                        Identifier(otherVariableName))
                    .WithType(identifierName)));

            var equalsExpressions = propertiesToCompare.Select(property =>
            {
               return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(systemNamespaceName),
                                        IdentifierName(collectionsNamespaceName)),
                                    IdentifierName(genericNamespaceName)),
                                GenericName(
                                    Identifier(equalityComparerName))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList(
                                            property.Type)))),
                            IdentifierName(equalityComparerDefaultProperty)),
                        IdentifierName(equalsMethodName)))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]{
                                Argument(
                                    IdentifierName(property.Identifier.Text)),
                                Token(SyntaxKind.CommaToken),
                                Argument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(otherVariableName),
                                        IdentifierName(property.Identifier.Text)))})));
            }).ToList();  

            var chainedAndExpression = BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                IdentifierName("other"),
                LiteralExpression(
                    SyntaxKind.NullLiteralExpression));

            foreach (var equalsExpression in equalsExpressions)
            {
                chainedAndExpression = BinaryExpression(
                    SyntaxKind.LogicalAndExpression,
                    chainedAndExpression,
                    equalsExpression);
            }

            var body = Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(chainedAndExpression)));

            return method
                .WithModifiers(modifiers)
                .WithParameterList(parameterList)
                .WithBody(body);
        }

        public MemberDeclarationSyntax GenerateGetHashCode(
            IEnumerable<RecordDescriptor.Entry> propertiesToCompare)
        {
            const string getHashCodeMethodName = "GetHashCode";
            const string hashCodeVariableName = "hashCode";
            const int hashCodeInitialValue = 2085527896;
            const int hashCodeMultiplicationValue = 1521134295;

            var method = MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.IntKeyword)),
                        Identifier(getHashCodeMethodName));

            var modifiers = TokenList(
                new []{
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword)});

            var hashCodeVariableDeclaration = LocalDeclarationStatement(
                VariableDeclaration(
                    IdentifierName(varIdentifierName))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(hashCodeVariableName))
                        .WithInitializer(
                            EqualsValueClause(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(hashCodeInitialValue)))))));

            var hashCodeAssignments = propertiesToCompare.Select(property =>
            {
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
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName(systemNamespaceName),
                                                    IdentifierName(collectionsNamespaceName)),
                                                IdentifierName(genericNamespaceName)),
                                            GenericName(
                                                Identifier(equalityComparerName))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList(property.Type)))),
                                        IdentifierName(equalityComparerDefaultProperty)),
                                    IdentifierName(getHashCodeMethodName))).WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            IdentifierName(property.Identifier.Text))))))));
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

            return method
                .WithModifiers(modifiers)
                .WithBody(body);
        }
    }
}
