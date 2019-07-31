using Amadevus.RecordGenerator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class EqualityUsageAnalyzer
    {
        public static IEnumerable<Diagnostic> GenerateDiagnostics(RecordDescriptor descriptor)
        {
            if (!descriptor.Features.HasFlag(Features.EquatableEquals) 
             && !descriptor.Features.HasFlag(Features.ObjectEquals)
             && !descriptor.Features.HasFlag(Features.OperatorEquals)
             || descriptor.TypeDeclaration.Modifiers.Any(t => t.IsKind(SyntaxKind.SealedKeyword)))
            {
                yield break;
            }

            yield return Diagnostic.Create(
                Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled,
                descriptor.TypeDeclarationLocation,
                descriptor.TypeIdentifier.Text);
        }
        
    }
}
