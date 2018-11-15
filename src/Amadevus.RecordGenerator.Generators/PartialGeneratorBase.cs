using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    internal abstract class PartialGeneratorBase
    {
        private const string MissingXmlWarning = "CS1591";
        private const string MissingXmlComment = "// Missing XML comment for publicly visible type or member";

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
                .WithOpenBraceToken(
                    GeneratePragmaToken(MissingXmlWarning, MissingXmlComment, isRestore: false))
                .WithMembers(
                    GenerateMembers())
                .WithCloseBraceToken(
                    GeneratePragmaToken(MissingXmlWarning, MissingXmlComment, isRestore: true));
        }

        private SyntaxToken GeneratePragmaToken(string warning, string comment, bool isRestore)
        {
            return
                Token(
                    TriviaList(
                        Trivia(
                            PragmaWarningDirectiveTrivia(
                                Token(isRestore ? SyntaxKind.RestoreKeyword : SyntaxKind.DisableKeyword),
                                true)
                            .WithErrorCodes(
                                SingletonSeparatedList<ExpressionSyntax>(
                                    IdentifierName(
                                        Identifier(
                                            TriviaList(),
                                            warning,
                                            TriviaList(
                                                Comment(comment)))))))),
                    isRestore ? SyntaxKind.CloseBraceToken : SyntaxKind.OpenBraceToken,
                    TriviaList());
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
