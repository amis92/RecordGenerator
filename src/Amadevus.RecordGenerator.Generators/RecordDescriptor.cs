using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.Generators
{
    internal partial class RecordDescriptor
    {
        public RecordDescriptor(TypeSyntax TypeSyntax, SyntaxToken TypeIdentifier, ImmutableArray<Entry> Entries, Location TypeDeclarationLocation, bool IsTypeSealed)
        {
            this.TypeSyntax = TypeSyntax;
            this.TypeIdentifier = TypeIdentifier;
            this.Entries = Entries;
            this.TypeDeclarationLocation = TypeDeclarationLocation;
            this.IsTypeSealed = IsTypeSealed;
        }

        public TypeSyntax TypeSyntax { get; }

        public SyntaxToken TypeIdentifier { get; }

        public ImmutableArray<Entry> Entries { get; }

        public Location TypeDeclarationLocation { get; }

        public bool IsTypeSealed { get; }

        internal abstract class Entry
        {
            public Entry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax, ISymbol TypeSymbol)
            {
                this.Identifier = Identifier;
                this.Type = Type;
                this.PropertySyntax = PropertySyntax;
                var id = (string)Identifier.Value;
                var camelized = char.ToLowerInvariant(id[0]) + id.Substring(1);
                IdentifierInCamelCase = SyntaxFactory.Identifier(CSharpKeyword.Is(camelized) ? "@" + camelized : camelized);
                this.TypeSymbol = TypeSymbol;
            }

            public SyntaxToken Identifier { get; }

            public SyntaxToken IdentifierInCamelCase { get; }

            public TypeSyntax Type { get; }

            public PropertyDeclarationSyntax PropertySyntax { get; }

            public ISymbol TypeSymbol { get; }
        }

        internal class SimpleEntry : Entry
        {
            public SimpleEntry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax, ISymbol TypeSymbol) : base(Identifier, Type, PropertySyntax, TypeSymbol)
            {
            }
        }
    }
}
