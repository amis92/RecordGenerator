using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal class ClassRecordPartialGenerator : RecordPartialGenerator
    {
        public ClassRecordPartialGenerator(ClassDeclarationSyntax declaration, CancellationToken c) : base(declaration, c)
        {
            TypeDeclaration = declaration;
        }

        protected new ClassDeclarationSyntax TypeDeclaration { get; }

        protected override TypeDeclarationSyntax GenerateTypeDeclaration()
        {
            TypeDeclarationSyntax newDeclaration = TypeDeclaration
                .WithBaseList(null)
                .WithAttributeLists(
                    SyntaxFactory.List(new[] {
                        GeneratedCodeAttributeExtensions.CreateAttribute()
                    }))
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(GenerateMembers(TypeDeclaration.Identifier, RecordProperties));
            return newDeclaration;
        }

        protected override string TypeName()
        {
            return TypeDeclaration.Identifier.ValueText;
        }
    }
}
