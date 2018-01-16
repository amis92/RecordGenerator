using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.Analyzers.Test
{
    static class Extensions
    {
        public static IEnumerable<T> ToSingleton<T>(this T element) => element.ToSingletonArray();

        public static T[] ToSingletonArray<T>(this T element) => new[] { element };

        public static string CropRawIndent(this string raw, bool skipFirstLine = true)
        {
            var lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToImmutableArray();
            if (lines.Length == 0)
            {
                return raw;
            }
            var indent = lines[lines.Length - 1].TakeWhile(c => c == ' ').Count();
            var indentString = new string(' ', indent);
            return string.Join(Environment.NewLine, lines.Skip(skipFirstLine ? 1 : 0).Select(CropIndent));
            string CropIndent(string line)
            {
                if (line.Length < indent)
                {
                    return line;
                }
                return line.StartsWith(indentString) ? line.Substring(indent) : line;
            }
        }
    }
}