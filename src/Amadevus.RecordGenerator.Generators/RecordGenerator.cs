using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Amadevus.RecordGenerator.Generators
{
    public class RecordGenerator : ICodeGenerator
    {
        private readonly RecordGeneratorOptions options;

        public RecordGenerator(AttributeData attributeData)
        {
            options = RecordGeneratorOptions.FromAttributeData(attributeData);
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var generatedMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
            if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration)
            {
                var descriptor = classDeclaration.ToRecordDescriptor();
                generatedMembers = generatedMembers.AddRange(GenerateRecordPartials(descriptor));
            }
            return Task.FromResult(generatedMembers);

            IEnumerable<MemberDeclarationSyntax> GenerateRecordPartials(RecordDescriptor descriptor)
            {
                if (descriptor.Entries.IsEmpty)
                {
                    yield break;
                }
                yield return RecordPartialGenerator.Generate(descriptor, cancellationToken);
                yield return BuilderPartialGenerator.Generate(descriptor, cancellationToken);
                yield return DeconstructPartialGenerator.Generate(descriptor, cancellationToken);
                yield return EqualityPartialGenerator.Generate(descriptor, cancellationToken);
                yield break;
            }
        }
    }
}
