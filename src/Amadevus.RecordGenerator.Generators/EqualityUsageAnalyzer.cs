using System.Collections.Generic;
using System.Linq;
using Amadevus.RecordGenerator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class EqualityUsageAnalyzer
    {
        public static IEnumerable<Diagnostic> GenerateDiagnostics(RecordDescriptor descriptor)
        {
            bool isSealed = descriptor.TypeDeclaration.Modifiers.Any(t => t.IsKind(SyntaxKind.SealedKeyword));
            if (!isSealed && (descriptor.Features & (Features.EquatableEquals | Features.ObjectEquals)) != 0)
            {
                yield return Diagnostic.Create(
                    Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled,
                    descriptor.TypeDeclarationLocation,
                    descriptor.TypeIdentifier.Text);
            }
        }
    }
}
