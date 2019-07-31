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
            public Entry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax)
            {
                this.Identifier = Identifier;
                this.Type = Type;
                this.PropertySyntax = PropertySyntax;
                var id = (string)Identifier.Value;
                var camelized = char.ToLowerInvariant(id[0]) + id.Substring(1);
                IdentifierInCamelCase = SyntaxFactory.Identifier(CSharpKeyword.Is(camelized) ? "@" + camelized : camelized);
            }

            public SyntaxToken Identifier { get; }

            public SyntaxToken IdentifierInCamelCase { get; }

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
