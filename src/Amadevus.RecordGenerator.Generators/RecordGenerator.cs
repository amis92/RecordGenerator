using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amadevus.RecordGenerator.Generators
{
    public class RecordGenerator : ICodeGenerator
    {
        private readonly AttributeData attributeData;

        public RecordGenerator(AttributeData attributeData)
        {
            this.attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var generatedMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
            var features = GetFeatures();

            if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration)
            {
                var descriptor = classDeclaration.ToRecordDescriptor(features);
                generatedMembers = generatedMembers
                    .AddRange(
                        GenerateRecordPartials(descriptor)
                        .Where(x => x != null));
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
                yield break;
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
