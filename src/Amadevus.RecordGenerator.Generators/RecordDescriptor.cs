using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.Generators
{
    internal partial class RecordDescriptor
    {
        public RecordDescriptor(TypeSyntax Type, SyntaxToken TypeIdentifier, ImmutableArray<Entry> Entries, TypeDeclarationSyntax TypeDeclaration, Location TypeDeclarationLocation, SemanticModel SemanticModel)
        {
            this.Type = Type;
            this.TypeIdentifier = TypeIdentifier;
            this.Entries = Entries;
            this.TypeDeclaration = TypeDeclaration;
            this.TypeDeclarationLocation = TypeDeclarationLocation;
            this.SemanticModel = SemanticModel;
        }

        public TypeSyntax Type { get; }

        public SyntaxToken TypeIdentifier { get; }

        public ImmutableArray<Entry> Entries { get; }

        public TypeDeclarationSyntax TypeDeclaration { get; }

        public Location TypeDeclarationLocation { get; }

        public SemanticModel SemanticModel { get; }

        internal abstract class Entry
        {
            public Entry(SyntaxToken Identifier, TypeSyntax Type, PropertyDeclarationSyntax PropertySyntax, ISymbol TypeSymbol)
            {
                this.Identifier = Identifier;
                this.Type = Type;
                this.PropertySyntax = PropertySyntax;
                this.TypeSymbol = TypeSymbol;
            }

            public SyntaxToken Identifier { get; }

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
