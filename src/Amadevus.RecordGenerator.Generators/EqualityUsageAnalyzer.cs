using System.Collections.Generic;
using Amadevus.RecordGenerator.Analyzers;
using Microsoft.CodeAnalysis;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class EqualityUsageAnalyzer
    {
        public static IEnumerable<Diagnostic> GenerateDiagnostics(RecordDescriptor descriptor, Features features)
        {
            if (!descriptor.Symbol.IsSealed
                && (features & (Features.EquatableEquals | Features.ObjectEquals)) != 0)
            {
                yield return Diagnostic.Create(
                    Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled,
                    descriptor.TypeDeclarationLocation,
                    descriptor.TypeIdentifier.Text);
            }
        }
    }
}
