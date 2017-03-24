﻿using System.Threading;
using System.Threading.Tasks;
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

        protected override Document GenerateRecordPartial(Document document)
        {
            return GenerateDocument(document);
        }

        protected override TypeDeclarationSyntax GenerateTypeDeclaration()
        {
            TypeDeclarationSyntax newDeclaration = TypeDeclaration
                .WithAttributeLists(
                    SyntaxFactory.List(new[] {
                        GeneratedCodeAttribute()
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