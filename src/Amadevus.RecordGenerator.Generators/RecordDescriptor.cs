using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator.Generators
{
    internal class RecordDescriptor
    {
        public RecordDescriptor(TypeSyntax typeSyntax, SyntaxToken typeIdentifier, ImmutableArray<Entry> entries, Location typeDeclarationLocation, bool isTypeSealed, SyntaxKind typeDeclarationSyntaxKind)
        {
            TypeSyntax = typeSyntax;
            TypeIdentifier = typeIdentifier;
            Entries = entries;
            TypeDeclarationLocation = typeDeclarationLocation;
            IsTypeSealed = isTypeSealed;
            TypeDeclarationSyntaxKind = typeDeclarationSyntaxKind;
        }

        public TypeSyntax TypeSyntax { get; }

        public SyntaxToken TypeIdentifier { get; }

        public ImmutableArray<Entry> Entries { get; }

        public Location TypeDeclarationLocation { get; }

        public bool IsTypeSealed { get; }

        public SyntaxKind TypeDeclarationSyntaxKind { get; }

        internal class Entry
        {
            public Entry(SyntaxToken identifier, SyntaxToken identifierInCamelCase, TypeSyntax typeSyntax, string qualifiedTypeName)
            {
                Identifier = identifier;
                TypeSyntax = typeSyntax;
                IdentifierInCamelCase = identifierInCamelCase;
                QualifiedTypeName = qualifiedTypeName;
            }

            public SyntaxToken Identifier { get; }

            public SyntaxToken IdentifierInCamelCase { get; }

            public TypeSyntax TypeSyntax { get; }

            public string QualifiedTypeName { get; }
        }
    }
}
