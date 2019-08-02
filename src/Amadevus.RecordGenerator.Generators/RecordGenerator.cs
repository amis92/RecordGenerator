using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Amadevus.RecordGenerator.Generators
{
    public class RecordGenerator : ICodeGenerator
    {
        private readonly AttributeData attributeData;

        private static readonly ImmutableArray<IPartialGenerator> PartialGenerators =
            ImmutableArray.Create(RecordPartialGenerator.Instance,
                                  BuilderPartialGenerator.Instance,
                                  DeconstructPartialGenerator.Instance);

        public RecordGenerator(AttributeData attributeData)
        {
            this.attributeData = attributeData;
        }

        static readonly Task<SyntaxList<MemberDeclarationSyntax>> EmptyResultTask =
            Task.FromResult(List<MemberDeclarationSyntax>());

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            return context.ProcessingNode is ClassDeclarationSyntax cds
                 ? GenerateAsync(cds)
                 : EmptyResultTask;

            Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(ClassDeclarationSyntax classDeclaration)
            {
                var descriptor = classDeclaration.ToRecordDescriptor();
                if (descriptor.Entries.IsEmpty)
                    return EmptyResultTask;

                var generatedMembers = new List<MemberDeclarationSyntax>();
                var features = GetFeatures();
                var partialKeyword = Token(SyntaxKind.PartialKeyword);

                var partials =
                    from g in PartialGenerators
                    select g.Generate(descriptor, features)
                    into g
                    where !g.IsEmpty
                    select
                        ClassDeclaration(classDeclaration.Identifier.WithoutTrivia())
                            .WithTypeParameterList(classDeclaration.TypeParameterList?.WithoutTrivia())
                            .WithBaseList(g.BaseTypes.IsEmpty ? null : BaseList(SeparatedList(g.BaseTypes)))
                            .WithModifiers(TokenList(g.Modifiers.Except(new[] {partialKeyword})
                                .Append(partialKeyword)))
                            .WithMembers(List(g.Members))
                            .AddGeneratedCodeAttributeOnMembers();

                foreach (var partial in partials)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    generatedMembers.Add(partial);
                }

                return generatedMembers.Count > 0
                    ? Task.FromResult(List(generatedMembers))
                    : EmptyResultTask;
            }
        }

        private Features GetFeatures()
        {
            return attributeData.ConstructorArguments.Length > 0
                ? (Features)attributeData.ConstructorArguments[0].Value
                : Features.Default;
        }
    }
}
