using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal class MethodBuilder
    {
        public string Identifier { get; set; }
        public TypeSyntax ReturnType { get; set; }
        public IList<SyntaxKind> Modifiers { get; } = new List<SyntaxKind>();
        public IDictionary<string, TypeSyntax> Parameters { get; } = new Dictionary<string, TypeSyntax>();
        public BlockSyntax Body { get; set; }

        public MethodDeclarationSyntax Create()
        {
            var method = MethodDeclaration(
                ReturnType,
                Identifier);

            method = method.WithModifiers(
                TokenList(Modifiers.Select(m => Token(m)).ToArray()));

            method = method.WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        Parameters.Select(
                            kvp => (SyntaxNodeOrToken)Parameter(Identifier(kvp.Key)).WithType(kvp.Value))
                        .AddBetween(Token(SyntaxKind.CommaToken))
                        .ToArray())));

            method = method.WithBody(Body);

            return method;
        }

    }

    internal static class MemberAccessGenerator
    {
        public static InvocationExpressionSyntax GenerateInvocation(
            ExpressionSyntax @object, string memberName, ExpressionSyntax[] arguments)
        {

            var methodArguments = arguments
                .Select(a => (SyntaxNodeOrToken)Argument(a))
                .AddBetween(Token(SyntaxKind.CommaToken))
                .ToArray();

            return InvocationExpression(
                GenerateMemberAccess(@object, memberName))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(methodArguments.ToArray())));
        }

        public static MemberAccessExpressionSyntax GenerateMemberAccess(
            ExpressionSyntax @object, string memberName)
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                @object,
                IdentifierName(memberName));
        }
    }


    internal static class QualifiedNameGenerator
    {
        public static QualifiedNameSyntax GenerateQualifiedName(string qualifiedName)
        {
            var (namespaces, type) = SplitNamespacePartsAndType(qualifiedName);
            var ns = GenerateQualifiedNamespaceAccess(namespaces);

            return QualifiedName(ns, IdentifierName(type));
        }

        public static QualifiedNameSyntax GenerateGenericQualifiedName(string qualifiedName, TypeSyntax[] typeArguments)
        {
            var (namespaces, type) = SplitNamespacePartsAndType(qualifiedName);
            var ns = GenerateQualifiedNamespaceAccess(namespaces);

            var genericType = GenericName(type)
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(typeArguments)));

            return QualifiedName(ns, genericType);
        }

        private static NameSyntax GenerateQualifiedNamespaceAccess(string[] namespaceParts)
        {
            if (namespaceParts.Length == 1) return IdentifierName(namespaceParts.Single()); 

            var namespaceAccess = QualifiedName(
                IdentifierName(namespaceParts.First()),
                IdentifierName(namespaceParts.Skip(1).First()));

            foreach (var namespacePart in namespaceParts.Skip(2))
            {
                namespaceAccess = QualifiedName(namespaceAccess, IdentifierName(namespacePart));
            }

            return namespaceAccess;
        }

        private static (string[],string) SplitNamespacePartsAndType(string qualifiedName)
        {
            var qualifiedNameParts = qualifiedName.Split('.');
            var namespaces = qualifiedNameParts.Take(qualifiedNameParts.Count() - 1);
            var type = qualifiedNameParts.Last();

            return (namespaces.ToArray(), type);
        }
    }

    internal static class localVariableDeclarationGenerator
    {
        public static LocalDeclarationStatementSyntax GenerateLocalVariableDeclaration(
            string variableName, ExpressionSyntax initializer)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(
                    IdentifierName("var"))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(variableName))
                        .WithInitializer(
                            EqualsValueClause(initializer)))));
        }
    }

    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> AddBetween<T>(this IEnumerable<T> input, T separator)
        {
            if (!input.Any()) yield break;
            yield return input.First();
            foreach(var inputItem in input.Skip(1))
            {
                yield return separator;
                yield return inputItem;
            } 
        }
    }
}
