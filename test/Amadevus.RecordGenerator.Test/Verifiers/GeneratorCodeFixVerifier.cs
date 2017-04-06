using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestHelper;

namespace TestHelper
{
    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with generator codefixes.
    /// Contains methods used to verify correctness of generator codefixes
    /// </summary>
    public abstract partial class GeneratorCodeFixVerifier : CodeFixVerifier
    {

        /// <summary>
        /// Called to test a C# codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="sources">Source package to operate on</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        protected void VerifyCSharpGeneratorFix(GeneratorSourcePackage sources, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyGeneratorFix(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), sources, codeFixIndex, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// Called to test a VB codefix when applied on the inputted string as a source
        /// </summary>
        /// <param name="sources">Source package to operate on</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        protected void VerifyBasicGeneratorFix(GeneratorSourcePackage sources, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            VerifyGeneratorFix(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), sources, codeFixIndex, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// General verifier for codefixes.
        /// Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
        /// Then gets the string after the codefix is applied and compares it with the expected result.
        /// Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="language">The language the source code is in</param>
        /// <param name="analyzer">The analyzer to be applied to the source code</param>
        /// <param name="codeFixProvider">The codefix to be applied to the code wherever the relevant Diagnostic is found</param>
        /// <param name="sources">Source package to operate on</param>
        /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
        /// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
        private void VerifyGeneratorFix(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, GeneratorSourcePackage sources, int? codeFixIndex, bool allowNewCompilerDiagnostics)
        {
            var compiledSources = (sources.AdditionalSources?.ToImmutableList() ?? ImmutableList<string>.Empty).Insert(0, sources.OldSource).ToArray();
            var documents = GetDocuments(compiledSources, language);
            var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, documents.Take(1).ToArray());
            var compilerDiagnostics = GetCompilerDiagnostics(documents[0]);
            var attempts = analyzerDiagnostics.Length;

            var documentPackage = new GeneratorDocumentPackage
            {
                FixedDocument = documents[0]
            };
            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                if (!codeFixProvider.FixableDiagnosticIds.Contains(analyzerDiagnostics[0].Id))
                {
                    continue;
                }
                var context = new CodeFixContext(documentPackage.FixedDocument, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    documentPackage = ApplyGeneratorFix(documentPackage.FixedDocument, actions.ElementAt((int)codeFixIndex));
                    break;
                }

                documentPackage = ApplyGeneratorFix(documentPackage.FixedDocument, actions.ElementAt(0));
                analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { documentPackage.FixedDocument });

                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(documentPackage.FixedDocument));

                //check if applying the code fix introduced any new compiler diagnostics
                if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                {
                    // Format and get the compiler diagnostics again so that the locations make sense in the output
                    documentPackage.FixedDocument = documentPackage.FixedDocument.WithSyntaxRoot(
                        Formatter.Format(
                            documentPackage.FixedDocument.GetSyntaxRootAsync().Result,
                            Formatter.Annotation,
                            documentPackage.FixedDocument.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(documentPackage.FixedDocument));

                    Assert.IsTrue(false,
                        string.Format("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
                            string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
                            documentPackage.FixedDocument.GetSyntaxRootAsync().Result.ToFullString()));
                }

                //check if there are analyzer diagnostics left after the code fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            //after applying all of the code fixes, compare the resulting string to the inputted one
            var actualFixed = GetStringFromDocument(documentPackage.FixedDocument);
            Assert.AreEqual(sources.FixedSource, actualFixed);
            if (sources.AddedSource != null)
            {
                Assert.IsNotNull(documentPackage.AddedDocument, "Expected added (new) document, none found in results.");
                var actualAdded = GetStringFromDocument(documentPackage.AddedDocument);
                Assert.AreEqual(sources.AddedSource, actualAdded);
            }
            if (sources.ChangedSource != null)
            {
                Assert.IsNotNull(documentPackage.ChangedDocument, "Expected changed (except the fixed one) document, none found in results.");
                var actualChanged = GetStringFromDocument(documentPackage.ChangedDocument);
                Assert.AreEqual(sources.ChangedSource, actualChanged);
            }
        }

        protected class GeneratorDocumentPackage
        {
            public Document FixedDocument { get; set; }
            public Document AddedDocument { get; set; }
            public Document ChangedDocument { get; set; }
        }

        protected class GeneratorSourcePackage
        {
            /// <summary>
            /// Gets or sets source to apply codefix to. Must not be null.
            /// </summary>
            public string OldSource { get; set; }
            /// <summary>
            /// Gets or sets additional sources to include in codefixed project.
            /// </summary>
            public string[] AdditionalSources { get; set; }
            /// <summary>
            /// Gets or sets source from <see cref="OldSource"/> after codefix. Must not be null.
            /// </summary>
            public string FixedSource { get; set; }
            /// <summary>
            /// Gets or sets source of document that was added to project during codefix. May be null.
            /// </summary>
            public string AddedSource { get; set; }
            /// <summary>
            /// Gets or sets source of document that was changed during hotfix (but it's not the same as <see cref="FixedSource"/>).
            /// May be null.
            /// </summary>
            public string ChangedSource { get; set; }
        }
    }
}
