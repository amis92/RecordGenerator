using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.Generators
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
            private (bool, SyntaxToken) identifierInCamelCase;

            public Entry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax)
            {
                this.Identifier = Identifier;
                this.Type = Type;
                this.PropertySyntax = PropertySyntax;
            }

            public SyntaxToken Identifier { get; }

            public SyntaxToken IdentifierInCamelCase =>
                Lazy.EnsureInitialized(ref identifierInCamelCase, this, self =>
                {
                    var id = (string)self.Identifier.Value;
                    var id2 = char.ToLowerInvariant(id[0]) + id.Substring(1);
                    return SyntaxFactory.Identifier(CSharpKeyword.Is(id2) ? "@" + id2 : id2);
                });

            public TypeSyntax Type { get; }

            public PropertyDeclarationSyntax PropertySyntax { get; }
        }

        internal class SimpleEntry : Entry
        {
            public SimpleEntry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax) : base(Identifier, Type, PropertySyntax)
            {
            }
        }
    }
}
