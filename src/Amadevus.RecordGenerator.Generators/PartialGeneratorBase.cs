using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
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

        public virtual ClassDeclarationSyntax GenerateTypeDeclaration()
        {
            return
                ClassDeclaration(
                    GenerateTypeIdentifier())
                .WithTypeParameterList(
                    GenerateTypeParameterList())
                .WithBaseList(
                    GenerateBaseList())
                .WithModifiers(
                    GenerateModifiers())
                .WithMembers(
                    GenerateMembers());
        }

        protected virtual TypeParameterListSyntax GenerateTypeParameterList()
        {
            return Descriptor.TypeDeclaration.TypeParameterList?.WithoutTrivia();
        }

        protected virtual SyntaxToken GenerateTypeIdentifier()
        {
            return Descriptor.TypeIdentifier.WithoutTrivia();
        }

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
