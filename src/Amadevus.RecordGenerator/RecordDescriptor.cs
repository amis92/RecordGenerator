using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Amadevus.RecordGenerator
{
    internal partial class RecordDescriptor
    {
        public RecordDescriptor(TypeSyntax Type, SyntaxToken TypeIdentifier, ImmutableArray<Entry> Entries, TypeDeclarationSyntax TypeDeclaration)
        {
            this.Type = Type;
            this.TypeIdentifier = TypeIdentifier;
            this.Entries = Entries;
            this.TypeDeclaration = TypeDeclaration;
        }

        public TypeSyntax Type { get; }

        public SyntaxToken TypeIdentifier { get; }

        public ImmutableArray<Entry> Entries { get; }

        public TypeDeclarationSyntax TypeDeclaration { get; }

        internal abstract class Entry
        {
            public Entry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax)
            {
                this.Identifier = Identifier;
                this.Type = Type;
                this.PropertySyntax = PropertySyntax;
            }

            public SyntaxToken Identifier { get; }

            public TypeSyntax Type { get; }

            public PropertyDeclarationSyntax PropertySyntax { get; }
        }

        internal class SimpleEntry : Entry
        {
            public SimpleEntry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax) : base(Identifier, Type, PropertySyntax)
            {
            }
        }

        internal class CollectionEntry : Entry
        {
            public CollectionEntry(SyntaxToken Identifier, GenericNameSyntax Type, PropertyDeclarationSyntax PropertySyntax)
                : base(Identifier, Type, PropertySyntax)
            {
                CollectionGenericName = Type;
                CollectionTypeParameter = (IdentifierNameSyntax)CollectionGenericName.TypeArgumentList.Arguments[0];
            }

            public GenericNameSyntax CollectionGenericName { get; }

            public IdentifierNameSyntax CollectionTypeParameter { get; }
        }
    }
}
