using Microsoft.CodeAnalysis;

namespace Amadevus.RecordGenerator
{
    internal class RecordPartialInvalidDiagnostic
    {
        public const string DiagnosticId = Properties.DiagnosticIdPrefix + "0003";
        private static readonly string Title = "Invalid generated record partial";
        private static readonly string MessageFormat = "Type '{0}' marked as [Record] has generated partial that requires re-generation. Diff:\n\n{1}";
        private static readonly string Description = "Generated record partial is invalid and requires re-generation.";

        public static DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                Properties.AnalyzerCategory,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: Description);

        public static Diagnostic Create(Location location, params object[] messageArgs)
        {
            return Diagnostic.Create(Descriptor, location, messageArgs);
        }
    }
}
