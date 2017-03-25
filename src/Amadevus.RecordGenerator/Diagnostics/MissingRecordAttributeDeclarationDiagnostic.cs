using Microsoft.CodeAnalysis;

namespace Amadevus.RecordGenerator
{
    internal static class MissingRecordAttributeDeclarationDiagnostic
    {
        public const string DiagnosticId = Properties.DiagnosticIdPrefix + "0001";
        private static readonly string Title = "Missing RecordAttribute declaration";
        private static readonly string MessageFormat = "Type {0} has [{1}] attribute but no such attribute is defined.";
        private static readonly string Description = "No RecordAttribute id defined.";

        public static DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                Properties.AnalyzerCategory,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: Description);

        public static Diagnostic Create(Location location, params object[] messageArgs)
        {
            return Diagnostic.Create(Descriptor, location, messageArgs);
        }
    }
}