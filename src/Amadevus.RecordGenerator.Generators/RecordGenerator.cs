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
        private readonly AttributeData attributeData;

        public RecordGenerator(AttributeData attributeData)
        {
            this.attributeData = attributeData;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var generatedMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
            var features = GetFeatures(context);

            if (context.ProcessingNode is ClassDeclarationSyntax classDeclaration)
            {
                var descriptor = classDeclaration.ToRecordDescriptor(features);
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
                yield break;
            }
        }

        private Features GetFeatures(TransformationContext context)
        {
            if (attributeData.ConstructorArguments.Length > 0)
            {
                return (Features)attributeData.ConstructorArguments[0].Value;
            }
            return GetAssemblyDefault() ?? Features.Default;

            Features? GetAssemblyDefault()
            {
                foreach (var attribute in context.Compilation.Assembly.GetAttributes())
                {
                    if (attribute.AttributeClass.Name == nameof(DefaultRecordFeaturesAttribute)
                        && attribute.AttributeClass.ToDisplayString() == "Amadevus.RecordGenerator.DefaultRecordFeaturesAttribute")
                    {
                        return (Features)attribute.ConstructorArguments[0].Value;
                    }
                }
                return null;
            }
        }
    }
}
