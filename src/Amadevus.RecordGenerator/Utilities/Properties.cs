using System.Reflection;

namespace Amadevus.RecordGenerator
{
    public static class Properties
    {
        public const string AnalyzerName = "RecordGenerator";

        public const string AnalyzerCategory = "RecordGenerator";

        public const string DiagnosticIdPrefix = "RG";

        private static string _versionString;

        public static string VersionString => _versionString ?? (_versionString = GetVersionString());

        private static string GetVersionString()
        {
            var assembly = typeof(Properties).GetTypeInfo().Assembly;
            return assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "";
        }
    }
}