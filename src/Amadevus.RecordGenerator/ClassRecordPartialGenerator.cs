using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal class ClassRecordPartialGenerator : RecordPartialGenerator
    {
        public ClassRecordPartialGenerator(Document document, ClassDeclarationSyntax declaration, CancellationToken c) : base(document, declaration, c)
        {
            TypeDeclaration = declaration;
        }

        protected new ClassDeclarationSyntax TypeDeclaration { get; }

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
