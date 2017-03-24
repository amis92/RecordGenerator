using Microsoft.CodeAnalysis;

namespace Amadevus.RecordGenerator
{
    internal static class MissingRecordPartialDiagnostic
    {
        public const string DiagnosticId = RecordGeneratorProperties.DiagnosticIdPrefix + "0002";
        private static readonly string Title = "Missing record partial";
        private static readonly string MessageFormat = "Type '{0}' is marked as [Record] but no appropriate partial was found (with constructor and/or mutators).";
        private static readonly string Description = "Record type has no generated partial (with constructor and/or mutators).";

        public static DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                RecordGeneratorProperties.AnalyzerCategory,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: Description);

        public static Diagnostic Create(Location location, params object[] messageArgs)
        {
            return Diagnostic.Create(Descriptor, location, messageArgs);
        }
    }
}