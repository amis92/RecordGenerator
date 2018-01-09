namespace TestHelper
{
    public static class SourceExtensions
    {
        public const string GeneratorVersionToken = "GENERATOR_VERSION";

        public static string ReplaceRecordGeneratorVersion(this string source, string version)
        {
            return source.Replace(GeneratorVersionToken, version);
        }
    }
}
