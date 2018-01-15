using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator
{
    internal static class RecordDescriptorExtensions
    {
        public static RecordDescriptor.Entry ToRecordEntry(this PropertyDeclarationSyntax property)
        {
            return property.IsImmutableArrayType()
                ? (RecordDescriptor.Entry)  new RecordDescriptor.CollectionEntry(
                    property.Identifier.WithoutTrivia(),
                    (GenericNameSyntax)property.Type.WithoutTrivia(),
                    property.WithoutTrivia())
                : new RecordDescriptor.SimpleEntry(
                property.Identifier.WithoutTrivia(),
                property.Type.WithoutTrivia(),
                property.WithoutTrivia());
        }

        public static RecordDescriptor ToRecordDescriptor(this ClassDeclarationSyntax typeDeclaration)
        {
            return new RecordDescriptor(
                typeDeclaration.GetTypeSyntax().WithoutTrivia(),
                typeDeclaration.Identifier.WithoutTrivia(),
                typeDeclaration.GetRecordProperties(),
                typeDeclaration.WithoutTrivia());
        }

        public static RecordDescriptor ToRecordDescriptor(this StructDeclarationSyntax typeDeclaration)
        {
            return new RecordDescriptor(
                typeDeclaration.GetTypeSyntax().WithoutTrivia(),
                typeDeclaration.Identifier.WithoutTrivia(),
                typeDeclaration.GetRecordProperties(),
                typeDeclaration.WithoutTrivia());
        }

        private static ImmutableArray<RecordDescriptor.Entry> GetRecordProperties(this TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.Members.GetRecordProperties().AsRecordEntries();
        }

        private static ImmutableArray<PropertyDeclarationSyntax> GetRecordProperties(this SyntaxList<MemberDeclarationSyntax> members)
        {
            return members
                .OfType<PropertyDeclarationSyntax>()
                .Where(
                    propSyntax => propSyntax.IsRecordViable())
                .ToImmutableArray();
        }

        private static ImmutableArray<RecordDescriptor.Entry> AsRecordEntries(this IEnumerable<PropertyDeclarationSyntax> properties)
        {
            return properties
                .Select(p => p.ToRecordEntry())
                .ToImmutableArray();
        }

        public static string GetNodeTypeName(this RecordDescriptor descriptor)
        {
            return descriptor.TypeIdentifier.ValueText.GetNodeTypeNameCore();
        }

        public static IdentifierNameSyntax GetNodeTypeIdentifierName(this RecordDescriptor descriptor)
        {
            return IdentifierName(descriptor.GetNodeTypeName());
        }

        public static IdentifierNameSyntax GetNodeTypeIdentifierName(this RecordDescriptor.CollectionEntry entry)
        {
            return IdentifierName(entry.CollectionTypeParameter.ToString().GetNodeTypeNameCore());
        }

        public static string GetNodeTypeNameCore(this string typeName)
        {
            return typeName.StripSuffixes() + Names.NodeSuffix;
        }

        public static string StripSuffixes(this string typeName)
        {
            return typeName.StripSuffix(Names.CoreSuffix).StripSuffix(Names.NodeSuffix);
        }

        private static string StripSuffix(this string text, string suffix)
        {
            return text.EndsWith(suffix) ? text.Substring(0, text.Length - suffix.Length) : text;
        }

        public static QualifiedNameSyntax ToNestedBuilderType(this NameSyntax type)
        {
            return QualifiedName(
                    type,
                    IdentifierName(Names.Builder));
        }

        public static TypeSyntax ToListOfBuilderType(this RecordDescriptor.CollectionEntry entry)
        {
            return entry.CollectionTypeParameter.ToListOfBuilderType();
        }

        public static TypeSyntax ToListOfBuilderType(this NameSyntax nameSyntax)
        {
            return GenericName(Names.ListGeneric)
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList<TypeSyntax>(
                            nameSyntax.ToNestedBuilderType())))
                .WithNamespace(Names.ListGenericNamespace);
        }

        public static TypeSyntax ToIEnumerableType(this TypeSyntax typeArgument)
        {
            return GenericName(Names.IEnumerableGeneric)
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList(typeArgument)))
                .WithNamespace(Names.IEnumerableGenericNamespace);
        }

        public static TypeSyntax ToImmutableArrayType(this TypeSyntax typeArgument)
        {
            return GenericName(Names.ImmutableArray)
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList(typeArgument)))
                .WithNamespace(Names.ImmutableArrayNamespace);
        }

        public static TypeSyntax ToNodeListType(this TypeSyntax typeArgument)
        {
            return GenericName(Names.NodeList)
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList(typeArgument)));
        }

        public static string ToLowerFirstLetter(this string name)
        {
            return string.IsNullOrEmpty(name)
                ? name
                : $"{char.ToLowerInvariant(name[0])}{name.Substring(1)}";
        }
    }
}
