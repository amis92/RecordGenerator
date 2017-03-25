using Microsoft.CodeAnalysis;

namespace Amadevus.RecordGenerator
{
    internal class DifferentGeneratorVersionDiagnostic
    {
        public const string DiagnosticId = Properties.DiagnosticIdPrefix + "0004";
        private static readonly string Title = "Record partial was generated with different version of the tool";
        private static readonly string MessageFormat = "Type '{0}' has record partial generated with RecordGenerator v{1} but v{2} is active in the project.";

        public static DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                Properties.AnalyzerCategory,
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public static Diagnostic Create(Location location, params object[] messageArgs)
        {
            return Diagnostic.Create(Descriptor, location, messageArgs);
        }
    }
}
