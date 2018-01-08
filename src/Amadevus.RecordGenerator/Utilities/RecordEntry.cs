using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    [Record]
    internal class RecordEntry
    {
        public RecordEntry(SyntaxToken identifier, TypeSyntax type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        public SyntaxToken Identifier { get; }

        public TypeSyntax Type { get; }
    }
}
