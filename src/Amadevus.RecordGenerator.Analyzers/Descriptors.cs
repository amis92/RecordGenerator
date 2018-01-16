using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Amadevus.RecordGenerator.Analyzers.Category;

namespace Amadevus.RecordGenerator.Analyzers
{
    enum Category
    {
        Usage
    }

    static class Descriptors
    {
        const string IdPrefix = "RecordGen";

        static DiagnosticDescriptor Rule(
            int id, string title, Category category, DiagnosticSeverity defaultSeverity,
            string messageFormat, string description = null)
        {
            var isEnabledByDefault = true;
            return new DiagnosticDescriptor(
                IdPrefix + id, title, messageFormat, category.ToString(), defaultSeverity, isEnabledByDefault, description);
        }

        public static DiagnosticDescriptor X1000_RecordMustBePartial { get; } =
            Rule(1000, "Record must be partial", Usage, Error, "Add partial modifier to type declaration");
    }
}
