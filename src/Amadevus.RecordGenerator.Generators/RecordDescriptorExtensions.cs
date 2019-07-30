using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal static class RecordDescriptorExtensions
    {
        public static RecordDescriptor.Entry ToRecordEntry(this PropertyDeclarationSyntax property)
        {
            return new RecordDescriptor.SimpleEntry(
                property.Identifier.WithoutTrivia(),
                property.Type.WithoutTrivia(),
                property.WithoutTrivia());
        }

        public static RecordDescriptor ToRecordDescriptor(this ClassDeclarationSyntax typeDeclaration, Features features)
        {
            return new RecordDescriptor(
                features,
                typeDeclaration.GetTypeSyntax().WithoutTrivia(),
                typeDeclaration.Identifier.WithoutTrivia(),
                typeDeclaration.GetRecordProperties(),
                typeDeclaration.WithoutTrivia());
        }

        public static RecordDescriptor ToRecordDescriptor(this StructDeclarationSyntax typeDeclaration, Features features)
        {
            return new RecordDescriptor(
                features,
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

        public static QualifiedNameSyntax ToNestedBuilderType(this NameSyntax type)
        {
            return QualifiedName(
                    type,
                    IdentifierName(Names.Builder));
        }

        public static string ToLowerFirstLetter(this string name)
        {
            return string.IsNullOrEmpty(name)
                ? name
                : $"{char.ToLowerInvariant(name[0])}{name.Substring(1)}";
        }
    }
}
