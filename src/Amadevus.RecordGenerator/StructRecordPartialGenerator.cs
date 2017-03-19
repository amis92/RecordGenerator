using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal class StructRecordPartialGenerator : RecordPartialGenerator
    {
        public StructRecordPartialGenerator(Document document, StructDeclarationSyntax declaration, CancellationToken c) : base(document, declaration, c)
        {
            TypeDeclaration = declaration;
        }

        protected new StructDeclarationSyntax TypeDeclaration { get; }

        public override Task<Document> GenerateRecordPartialAsync()
        {
            return GenerateDocumentAsync();
        }

        protected override Task<TypeDeclarationSyntax> TypeDeclarationAsync()
        {
            TypeDeclarationSyntax newDeclaration = TypeDeclaration
                .WithAttributeLists(
                    SyntaxFactory.List(new[] {
                        GeneratedCodeAttribute()
                    }))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(GenerateMembers(TypeDeclaration.Identifier, RecordProperties));
            return Task.FromResult(newDeclaration);
        }

        protected override string TypeName()
        {
            return TypeDeclaration.Identifier.ValueText;
        }
    }
}
