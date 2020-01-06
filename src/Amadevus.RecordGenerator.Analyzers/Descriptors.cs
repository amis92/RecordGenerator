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
        const string HelpUriBase = "https://amis92.github.io/RecordGenerator/analyzers/rules/";

        static DiagnosticDescriptor Rule(
            int id, string title, Category category, DiagnosticSeverity defaultSeverity,
            string messageFormat, string description = null)
        {
            var isEnabledByDefault = true;
            var fullId = IdPrefix + id;
            var helpLinkUri = HelpUriBase + fullId;
            return new DiagnosticDescriptor(
                fullId, title, messageFormat, category.ToString(), defaultSeverity, isEnabledByDefault, description, helpLinkUri);
        }

        public static DiagnosticDescriptor X1000_RecordMustBePartial { get; } =
            Rule(1000, "Record type and all containing types must be partial", Usage, Error, "Add partial modifier to type declaration");

        public static DiagnosticDescriptor X1001_RecordMustBeSealedIfEqualityIsEnabled { get; } =
            Rule(1001, "Record must be sealed if equality generation is enabled", Usage, Warning, "Add sealed modifier to type '{0}'");

        public static DiagnosticDescriptor X1002_RecordEntriesMustDifferCaseInsensitive { get; } =
            Rule(1002, "Record properties cannot differ only by case", Usage, Error, "Property name '{0}' differs from '{1}' only by case. Change property names to be case-insensitive different.");
    }
}
