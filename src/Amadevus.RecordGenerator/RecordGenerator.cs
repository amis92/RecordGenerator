using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amadevus.RecordGenerator
{
    public class RecordGenerator : ICodeGenerator
    {
        public RecordGenerator(AttributeData attributeData)
        {

        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var generatedMembers = SyntaxFactory.List<MemberDeclarationSyntax>();
            if (context.ProcessingMember is ClassDeclarationSyntax classDeclaration)
            {
                var descriptor = classDeclaration.ToRecordDescriptor();
                generatedMembers = generatedMembers.AddRange(GenerateRecordPartials(descriptor));
            }
            return Task.FromResult(generatedMembers);
            IEnumerable<MemberDeclarationSyntax> GenerateRecordPartials(RecordDescriptor descriptor)
            {
                yield return RecordCorePartialGenerator.Generate(descriptor, cancellationToken);
                yield return BuilderCorePartialGenerator.Generate(descriptor, cancellationToken);
            }
        }
    }
}
