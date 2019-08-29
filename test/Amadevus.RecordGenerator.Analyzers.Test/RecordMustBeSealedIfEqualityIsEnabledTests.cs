using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Amadevus.RecordGenerator.Analyzers.Test
{
    public class RecordMustBeSealedIfEqualityIsEnabledTests : CodeFixVerifier
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

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new RecordMustBeSealedIfEqualityIsEnabledFixer();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new RecordMustBeSealedIfEqualityIsEnabled();

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
                    Description = "sealed [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            sealed class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent()
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed non-[Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            public class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent()
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed [Record] struct",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            struct RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed [Record] interface",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            interface RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    NewSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            sealed class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled, "RecordType")
                        {
                            Locations = new DiagnosticResultLocation(filename, 6, 11).ToSingletonArray()
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed [Record(Features.Equality)] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record(Features.Equality)]
                            class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    NewSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record(Features.Equality)]
                            sealed class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled, "RecordType")
                        {
                            Locations = new DiagnosticResultLocation(filename, 6, 11).ToSingletonArray()
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed public partial [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            public partial class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    NewSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            [Record]
                            public sealed partial class RecordType
                            {
                                public string Name { get; }
                            }
                        }".CropRawIndent(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled, "RecordType")
                        {
                            Locations = new DiagnosticResultLocation(filename, 6, 26).ToSingletonArray()
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "non-sealed nested [Record] class",
                    OldSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            class OuterClass
                            {
                                [Record]
                                class RecordType
                                {
                                    public string Name { get; }
                                }
                            }
                        }".CropRawIndent(),
                    NewSource = @"
                        namespace TestApplication
                        {
                            using Amadevus.RecordGenerator;

                            class OuterClass
                            {
                                [Record]
                                sealed class RecordType
                                {
                                    public string Name { get; }
                                }
                            }
                        }".CropRawIndent(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(Descriptors.X1001_RecordMustBeSealedIfEqualityIsEnabled, "RecordType")
                        {
                            Locations = new DiagnosticResultLocation(filename, 8, 15).ToSingletonArray()
                        }
                    }
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