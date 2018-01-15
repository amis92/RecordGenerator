using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator
{
    internal abstract class PartialGeneratorBase
    {
        protected PartialGeneratorBase(RecordDescriptor descriptor, CancellationToken cancellationToken)
        {
            Descriptor = descriptor;
            CancellationToken = cancellationToken;
        }

        protected RecordDescriptor Descriptor { get; }

        protected CancellationToken CancellationToken { get; }

        public virtual TypeDeclarationSyntax GenerateTypeDeclaration()
        {
            return
                ClassDeclaration(
                    GenerateTypeIdentifier())
                .WithBaseList(
                    GenerateBaseList())
                .WithModifiers(
                    GenerateModifiers())
                .WithMembers(
                    GenerateMembers());
        }

        protected abstract SyntaxToken GenerateTypeIdentifier();

        protected virtual SyntaxTokenList GenerateModifiers()
        {
            return TokenList(Token(SyntaxKind.PartialKeyword));
        }

        protected virtual SyntaxList<MemberDeclarationSyntax> GenerateMembers()
        {
            return List<MemberDeclarationSyntax>();
        }

        protected virtual BaseListSyntax GenerateBaseList()
        {
            return null;
        }
    }
}
