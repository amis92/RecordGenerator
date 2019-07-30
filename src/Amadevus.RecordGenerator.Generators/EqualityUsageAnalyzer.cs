using Amadevus.RecordGenerator;
using Amadevus.RecordGenerator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amadevus.RecordGenerator.Generators
{
    internal sealed class EqualityUsageAnalyzer
    {
        public static IEnumerable<Diagnostic> GenerateDiagnostics(RecordDescriptor descriptor)
        {
            if (descriptor.TypeDeclaration.Modifiers.Any(t => t.IsKind(SyntaxKind.SealedKeyword))) yield break;

            yield return Diagnostic.Create(
                Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled,
                descriptor.TypeDeclarationLocation,
                descriptor.TypeIdentifier.Text);
        }
        
    }
}
