using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator.Generators
{
    internal class RecordDescriptor
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

        internal class Entry
        {
            public Entry(SyntaxToken Identifier, SyntaxToken IdentifierInCamelCase, TypeSyntax TypeSyntax, string QualifiedTypeName)
            {
                this.Identifier = Identifier;
                this.TypeSyntax = TypeSyntax;
                this.IdentifierInCamelCase = IdentifierInCamelCase;
                this.QualifiedTypeName = QualifiedTypeName;
            }

            public SyntaxToken Identifier { get; }

            public SyntaxToken IdentifierInCamelCase { get; }

            public TypeSyntax TypeSyntax { get; }

            public string QualifiedTypeName { get; }
        }
    }
}
