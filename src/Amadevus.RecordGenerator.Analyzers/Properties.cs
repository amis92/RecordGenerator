using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Amadevus.RecordGenerator.Analyzers.Test")]

namespace Amadevus.RecordGenerator.Analyzers
{
    public static class Properties
    {
        public const string AnalyzerName = "RecordGenerator";

        private static string _versionString;

        public static string VersionString => _versionString ?? (_versionString = GetVersionString());

        private static string GetVersionString()
        {
            var assembly = typeof(Properties).GetTypeInfo().Assembly;
            return assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "";
        }
    }
}
