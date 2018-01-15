using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator
{
    internal class BuilderCorePartialGenerator : CorePartialGeneratorBase
    {
        protected BuilderCorePartialGenerator(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        public static TypeDeclarationSyntax Generate(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            var generator = new BuilderCorePartialGenerator(descriptor, cancellationToken);
            return generator.GenerateTypeDeclaration();
        }

        protected override SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return
                SingletonList<MemberDeclarationSyntax>(
                    GenerateToBuilderMethod())
                .Add(GenerateBuilder());
        }

        protected override BaseListSyntax GenerateBaseList()
        {
            return
                BaseList(
                    SingletonSeparatedList(
                        CreateIBuildableBaseTypeSyntax()));
            BaseTypeSyntax CreateIBuildableBaseTypeSyntax()
            {
                return
                    SimpleBaseType(
                        GenericName(
                            Identifier(Names.IBuildable),
                            TypeArgumentList(
                                SeparatedList(
                                    new TypeSyntax[]
                                    {
                                        IdentifierName(Descriptor.TypeIdentifier),
                                        QualifiedName(
                                            IdentifierName(Descriptor.TypeIdentifier),
                                            IdentifierName(Names.Builder))
                                    }))));
            }
        }

        private ClassDeclarationSyntax GenerateBuilder()
        {
            return
                ClassDeclaration(Names.Builder)
                .WithAttributeLists(GetBuilderClassAttributes())
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                .WithMembers(GenerateBuilderMembers());

            SyntaxList<MemberDeclarationSyntax> GenerateBuilderMembers()
            {
                return List<MemberDeclarationSyntax>()
                    .AddRange(Descriptor.Entries.SelectMany(GetPropertyMembers))
                    .Add(GetBuilderToImmutableMethod());
            }
            SyntaxList<AttributeListSyntax> GetBuilderClassAttributes()
            {
                var attributes = Descriptor.TypeDeclaration.AttributeLists.SelectMany(list => list.Attributes);
                var xmlAttributes = attributes.Where(att => att.IsNamed(Names.XmlType));
                return List(xmlAttributes.Select(att => AttributeList(SingletonSeparatedList(att))));
            }
        }

        private IEnumerable<MemberDeclarationSyntax> GetPropertyMembers(RecordDescriptor.Entry entry)
        {
            return entry is RecordDescriptor.CollectionEntry collectionEntry
                ? CreateArrayProperty()
                : CreateSimpleProperty();

            IEnumerable<PropertyDeclarationSyntax> CreateSimpleProperty()
            {
                yield return
                    PropertyDeclaration(
                        entry.Type,
                        entry.Identifier)
                    .WithAttributeLists(GetPropertyAttributes())
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(
                        AccessorList(
                            List(new[]
                            {
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration(
                                    SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken)),
                        })));
            }
            IEnumerable<MemberDeclarationSyntax> CreateArrayProperty()
            {
                var propertyName = collectionEntry.Identifier.ValueText;
                var fieldName = $"_{Char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";

                yield return
                    FieldDeclaration(
                        VariableDeclaration(
                            collectionEntry.ToListOfBuilderType())
                        .AddVariables(
                            VariableDeclarator(fieldName)))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PrivateKeyword)));

                yield return PropertyDeclaration(
                        collectionEntry.ToListOfBuilderType(),
                        collectionEntry.Identifier)
                    .WithAttributeLists(GetPropertyAttributes())
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(
                        AccessorList(
                            List(new[]
                            {
                            CreateGetter(),
                            CreateSetter()
                            })));
                AccessorDeclarationSyntax CreateGetter()
                {
                    return
                        AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                BinaryExpression(SyntaxKind.CoalesceExpression,
                                IdentifierName(fieldName),
                                ParenthesizedExpression(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName(fieldName),
                                        ObjectCreationExpression(
                                            collectionEntry.ToListOfBuilderType())
                                        .WithArgumentList(
                                            ArgumentList()))))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken));
                }
                AccessorDeclarationSyntax CreateSetter()
                {
                    return
                        AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration)
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName(fieldName),
                                    IdentifierName("value"))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken));
                }
            }
            SyntaxList<AttributeListSyntax> GetPropertyAttributes()
            {
                var attributes = entry.PropertySyntax.AttributeLists.SelectMany(list => list.Attributes);
                var xmlAttributes = attributes.Where(att => att.IsNamed(Names.XmlAttribute) || att.IsNamed(Names.XmlArray));
                return List(xmlAttributes.Select(att => AttributeList(SingletonSeparatedList(att))));
            }
        }

        private MethodDeclarationSyntax GetBuilderToImmutableMethod()
        {
            return MethodDeclaration(
                    Descriptor.Type,
                    Names.ToImmutable)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    Block(
                        ReturnStatement(
                            ObjectCreationExpression(
                                IdentifierName(Descriptor.TypeIdentifier))
                            .WithArgumentList(
                                CreateArgumentList()))));
            ArgumentListSyntax CreateArgumentList()
            {
                return ArgumentList(
                        SeparatedList(
                            Descriptor.Entries.Select(
                                entry => !(entry is RecordDescriptor.CollectionEntry collectionEntry)
                                ? Argument(IdentifierName(entry.Identifier))
                                : CreateCollectionArgument(collectionEntry))));
                ArgumentSyntax CreateCollectionArgument(RecordDescriptor.CollectionEntry entry)
                {
                    return
                        Argument(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(entry.Identifier),
                                    IdentifierName(Names.ToImmutableRecursive))));
                }
            }
        }

        private MethodDeclarationSyntax GenerateToBuilderMethod()
        {
            return MethodDeclaration(
                    IdentifierName(Names.Builder),
                    Names.ToBuilder)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    Block(
                        List(
                            CreateStatements())));
            IEnumerable<StatementSyntax> CreateStatements()
            {
                yield return
                    ReturnStatement(
                        ObjectCreationExpression(
                            IdentifierName(Names.Builder))
                        .WithInitializer(
                            InitializerExpression(
                                SyntaxKind.ObjectInitializerExpression,
                                SeparatedList(
                                    CreateInitializerExpressions()))));
            }
            IEnumerable<ExpressionSyntax> CreateInitializerExpressions()
            {
                return
                    Descriptor.Entries
                    .Select(entry =>
                    {
                        return entry is RecordDescriptor.CollectionEntry collectionEntry
                            ? CreateInitializerForCollectionEntry(collectionEntry)
                            : CreateInitializerForEntry(entry);
                    });
            }
            ExpressionSyntax CreateInitializerForEntry(RecordDescriptor.Entry entry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(entry.Identifier),
                        IdentifierName(entry.Identifier));
            }
            ExpressionSyntax CreateInitializerForCollectionEntry(RecordDescriptor.CollectionEntry collectionEntry)
            {
                return
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(collectionEntry.Identifier),
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(collectionEntry.Identifier),
                                IdentifierName(Names.ToBuildersList))));
            }
        }
    }
}
