﻿using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Amadevus.RecordGenerator.Analyzers.CodeActions;

namespace Amadevus.RecordGenerator.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class RecordMustBePartialFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(Descriptors.X1000_RecordMustBePartial.Id);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
            var document = context.Document;
            context.RegisterCodeFix(
                Actions.MakePartial(document, root, classDeclaration),
                context.Diagnostics);
        }
    }
}