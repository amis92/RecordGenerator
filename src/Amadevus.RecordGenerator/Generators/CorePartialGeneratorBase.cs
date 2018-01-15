using Microsoft.CodeAnalysis;
using System.Threading;

namespace Amadevus.RecordGenerator
{
    internal abstract class CorePartialGeneratorBase : PartialGeneratorBase
    {
        protected CorePartialGeneratorBase(RecordDescriptor descriptor, CancellationToken cancellationToken) : base(descriptor, cancellationToken)
        {
        }

        protected override SyntaxToken GenerateTypeIdentifier()
        {
            return Descriptor.TypeIdentifier.WithoutTrivia();
        }
    }
}
