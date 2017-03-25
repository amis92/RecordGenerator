using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator
{
    internal class StructRecordPartialGenerator : RecordPartialGenerator
    {
        public StructRecordPartialGenerator(StructDeclarationSyntax declaration, CancellationToken c) : base(declaration, c)
        {
            TypeDeclaration = declaration;
        }

        protected new StructDeclarationSyntax TypeDeclaration { get; }

        protected override Document GenerateRecordPartial(Document document, INamedTypeSymbol typeSymbol)
        {
            return GenerateDocument(document, typeSymbol);
        }

        protected override TypeDeclarationSyntax GenerateTypeDeclaration()
        {
            TypeDeclarationSyntax newDeclaration = TypeDeclaration
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
