using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Analyzers.CodeActions
{
    internal static class Actions
    {
        private class MakePartialCodeAction : CodeAction
        {
            public MakePartialCodeAction(Document document, SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
            {
                Document = document;
                Root = root;
                TypeDeclaration = typeDeclaration;
            }

            public override string Title => "Make Partial";
            public override string EquivalenceKey => Title;

            public Document Document { get; }
            public SyntaxNode Root { get; }
            public TypeDeclarationSyntax TypeDeclaration { get; }

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var modifiedNode = TypeDeclaration.AddModifiers(Token(SyntaxKind.PartialKeyword));
                return Task.FromResult(Document.WithSyntaxRoot(Root.ReplaceNode(TypeDeclaration, modifiedNode)));
            }
        }

        public static CodeAction MakePartial(Document document, SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
        {
            return new MakePartialCodeAction(document, root, typeDeclaration);
        }
    }
}