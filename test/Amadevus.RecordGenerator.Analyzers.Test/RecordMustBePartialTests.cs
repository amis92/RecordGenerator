using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using TestHelper;
using Xunit;

namespace Amadevus.RecordGenerator.Analyzers.Test
{
    public class RecordMustBePartialTests : CodeFixVerifier
    {
        //Diagnostic and CodeFix both triggered and checked for
        [Theory]
        [ClassData(typeof(TestCases))]
        public void Given_Source_Then_Verify_Diagnostics_And_CodeFix(
#pragma warning disable xUnit1026
            string description,
#pragma warning restore xUnit1026
            string oldSource, DiagnosticResult[] diagnosticResults, string newSource)
        {
            VerifyCSharpDiagnostic(oldSource, diagnosticResults ?? new DiagnosticResult[0]);
            if (newSource == null)
            {
                return;
            }
            VerifyCSharpFix(oldSource, newSource);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new RecordMustBePartialFixer();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new RecordMustBePartial();

        private class TestCases : TheoryDataProvider
        {
            public override IEnumerable<ITheoryDatum> GetDataSets()
            {
                string filename = $"{DefaultFilePathPrefix}.{CSharpDefaultFileExt}";
                yield return new GeneratorTheoryData
                {
                    Description = "empty source",
                    OldSource = ""
                };
                yield return new GeneratorTheoryData
                {
                    Description = "partial [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            partial class RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent()
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-partial [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            class RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    NewSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            partial class RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    ExpectedDiagnostics = new DiagnosticResult(Descriptors.X1000_RecordMustBePartial)
                        {
                            Locations =  new DiagnosticResultLocation(filename, 4, 11).ToSingletonArray()
                        }.ToSingletonArray()
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-partial nested [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            class OuterClass
                            {
                                [Record]
                                class RecordClass
                                {
                                    public string Name { get; }
                                }
                            }
                        }".CropRawIndent(),
                    NewSource = @"
                        namespace TestApplication
                        {
                            partial class OuterClass
                            {
                                [Record]
                                partial class RecordClass
                                {
                                    public string Name { get; }
                                }
                            }
                        }".CropRawIndent(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(Descriptors.X1000_RecordMustBePartial)
                        {
                            Locations = new DiagnosticResultLocation(filename, 3, 11).ToSingletonArray()
                        },
                        new DiagnosticResult(Descriptors.X1000_RecordMustBePartial)
                        {
                            Locations = new DiagnosticResultLocation(filename, 6, 15).ToSingletonArray()
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "partial [Record] interface",
                    OldSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            partial interface RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent()
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-partial [Record] interface",
                    OldSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            interface RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                };
                yield return new GeneratorTheoryData
                {
                    Description = "partial [Record] struct",
                    OldSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            partial struct RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent()
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-partial [Record] struct",
                    OldSource = @"
                        namespace TestApplication
                        {
                            [Record]
                            struct RecordClass
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                };
            }
        }

        public class GeneratorTheoryData : ITheoryDatum
        {
            public string Description { get; set; }

            public string OldSource { get; set; }

            public DiagnosticResult[] ExpectedDiagnostics { get; set; }

            public string NewSource { get; set; }

            public object[] ToParameterArray()
            {
                return
                    new object[]
                    {
                        Description,
                        OldSource,
                        ExpectedDiagnostics,
                        NewSource
                    };
            }
        }
    }
}