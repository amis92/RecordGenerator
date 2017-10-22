using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;

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

        public static string CreateMessageDiff(SyntaxNode before, SyntaxNode after)
        {
            return CreateMessageDiff(before.NormalizeWhitespace().ToFullString(), after.NormalizeWhitespace().ToFullString());
        }
    
        public static string CreateMessageDiff(string before, string after)
        {
            var diffBuilder = new DiffPlex.DiffBuilder.InlineDiffBuilder(new DiffPlex.Differ());
            var diff = diffBuilder.BuildDiffModel(before.ToString(), after.ToString());

            StringWriter writer = new StringWriter();
            var maxLineCountChars = diff.Lines.Select(line => line.Position).Max().ToString().Length;
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case DiffPlex.DiffBuilder.Model.ChangeType.Deleted:
                        WriteLineWithChangeSymbol("-");
                        break;
                    case DiffPlex.DiffBuilder.Model.ChangeType.Inserted:
                        WriteLineWithChangeSymbol("+");
                        break;
                    case DiffPlex.DiffBuilder.Model.ChangeType.Modified:
                        WriteLineWithChangeSymbol("~");
                        break;
                    default:
                        break;
                }

                void WriteLineWithChangeSymbol(string symbol)
                {
                    var positionString = (line.Position?.ToString() ?? "").PadLeft(maxLineCountChars);
                    writer.Write(positionString);
                    writer.Write($": {symbol} ");
                    writer.WriteLine(line.Text);
                }
            }
            return writer.ToString();
        }
    }
}
