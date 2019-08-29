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
                // per https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods#restrictions
                // 'partial' must be the last modifier - adding to the end is ok
                var modifiedNode = TypeDeclaration.AddModifiers(Token(SyntaxKind.PartialKeyword));
                return Task.FromResult(Document.WithSyntaxRoot(Root.ReplaceNode(TypeDeclaration, modifiedNode)));
            }
        }

        private class MakeSealedCodeAction : CodeAction
        {
            public MakeSealedCodeAction(Document document, SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
            {
                Document = document;
                Root = root;
                TypeDeclaration = typeDeclaration;
            }

            public override string Title => "Make Sealed";
            public override string EquivalenceKey => Title;

            public Document Document { get; }
            public SyntaxNode Root { get; }
            public TypeDeclarationSyntax TypeDeclaration { get; }

            protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                // per https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods#restrictions
                // 'partial' must be the last modifier - if there is one, we insert before it
                var newModifierList =
                    TypeDeclaration.Modifiers.Insert(GetSealedIndex(), Token(SyntaxKind.SealedKeyword));
                var modifiedNode = TypeDeclaration.WithModifiers(newModifierList);
                var result = Document.WithSyntaxRoot(Root.ReplaceNode(TypeDeclaration, modifiedNode));
                return Task.FromResult(result);
                int GetSealedIndex()
                {
                    if (TypeDeclaration.Modifiers.Count == 0)
                    {
                        return 0;
                    }
                    var partialIndex = TypeDeclaration.Modifiers.IndexOf(SyntaxKind.PartialKeyword);
                    return partialIndex >= 0 ? partialIndex : TypeDeclaration.Modifiers.Count - 1;
                }
            }
        }

        public static CodeAction MakePartial(Document document, SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
        {
            return new MakePartialCodeAction(document, root, typeDeclaration);
        }

        public static CodeAction MakeSealed(Document document, SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
        {
            return new MakeSealedCodeAction(document, root, typeDeclaration);
        }
    }
}