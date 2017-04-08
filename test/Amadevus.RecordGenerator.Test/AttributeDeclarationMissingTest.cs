using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using TestHelper;
using Xunit;
using System;
using System.Collections;
using System.Linq;

namespace Amadevus.RecordGenerator.Test
{
    public class AttributeDeclarationMissingTest : GeneratorCodeFixVerifier
    {
        //No diagnostics expected to show up
        [Fact]
        public void Given_Empty_Verify_NoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [Theory]
        [ClassData(typeof(TestCases))]
        public void Given_Source_Then_Verify_Diagnostics_And_CodeFix(
            string description, GeneratorSourcePackage sourcePackage, DiagnosticResult[] diagnosticResults)
        {
            VerifyCSharpDiagnostic(sourcePackage.GetInputSources(), diagnosticResults);
            VerifyCSharpGeneratorFix(sourcePackage);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new GenerateRecordAttributeDeclarationCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new RecordGeneratorAnalyzer();
        }

        private class TestCases : TheoryDataProvider
        {
            public override IEnumerable<ITheoryDatum> GetDataSets()
            {
                const string @namespace = "ConsoleApplication1";
                const string typeName = "TypeName";
                yield return new GeneratorTheoryData
                {
                    Description = "class with [Record]",
                    SourcePackage = new GeneratorSourcePackage
                    {
                        OldSource = GetBasicClassDeclaration(typeName, @namespace, "Record"),
                        AddedSource = GenerateRecordAttributeDeclarationCodeFixProvider.RecordAttributeDeclarationSource(@namespace)
                    }.AndFixedSameAsOld(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(RecordAttributeDeclarationMissingDiagnostic.Descriptor, typeName, "Record")
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 4, 10)
                            }
                        },
                        new DiagnosticResult(RecordPartialMissingDiagnostic.Descriptor, typeName)
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 5, 15)
                            }
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "class with [record]",
                    SourcePackage = new GeneratorSourcePackage
                    {
                        OldSource = GetBasicClassDeclaration(typeName, @namespace, "record"),
                        AddedSource = GenerateRecordAttributeDeclarationCodeFixProvider.RecordAttributeDeclarationSource(@namespace)
                    }.AndFixedSameAsOld(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(RecordAttributeDeclarationMissingDiagnostic.Descriptor, typeName, "record")
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 4, 10)
                            }
                        },
                        new DiagnosticResult(RecordPartialMissingDiagnostic.Descriptor, typeName)
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 5, 15)
                            }
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "struct with [Record]",
                    SourcePackage = new GeneratorSourcePackage
                    {
                        OldSource = GetBasicStructDeclaration(typeName, @namespace, "Record"),
                        AddedSource = GenerateRecordAttributeDeclarationCodeFixProvider.RecordAttributeDeclarationSource(@namespace)
                    }.AndFixedSameAsOld(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(RecordAttributeDeclarationMissingDiagnostic.Descriptor, typeName, "Record")
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 4, 10)
                            }
                        },
                        new DiagnosticResult(RecordPartialMissingDiagnostic.Descriptor, typeName)
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 5, 16)
                            }
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "struct with [record]",
                    SourcePackage = new GeneratorSourcePackage
                    {
                        OldSource = GetBasicStructDeclaration(typeName, @namespace, "record"),
                        AddedSource = GenerateRecordAttributeDeclarationCodeFixProvider.RecordAttributeDeclarationSource(@namespace)
                    }.AndFixedSameAsOld(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(RecordAttributeDeclarationMissingDiagnostic.Descriptor, typeName, "record")
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 4, 10)
                            }
                        },
                        new DiagnosticResult(RecordPartialMissingDiagnostic.Descriptor, typeName)
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 5, 16)
                            }
                        }
                    }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "interface with [Record] - no diagnostics expected",
                    SourcePackage = new GeneratorSourcePackage
                    {
                        OldSource = GetBasicInterfaceDeclaration(typeName, @namespace, "Record"),
                    }.AndFixedSameAsOld(),
                    ExpectedDiagnostics = new DiagnosticResult[] { }
                };
                yield return new GeneratorTheoryData
                {
                    Description = "attribute exists, only diagnostic to create record partial expected",
                    SourcePackage = new GeneratorSourcePackage
                    {
                        OldSource = GetBasicClassDeclaration(typeName, @namespace, "Record"),
                        AdditionalSources = new[] { GenerateRecordAttributeDeclarationCodeFixProvider.RecordAttributeDeclarationSource(@namespace) }
                    }.AndFixedSameAsOld(),
                    ExpectedDiagnostics = new[]
                    {
                        new DiagnosticResult(RecordPartialMissingDiagnostic.Descriptor, typeName)
                        {
                            Locations =
                            new[] {
                                new DiagnosticResultLocation("Test0.cs", 5, 15)
                            }
                        }
                    }
                };
            }

            private static string GetBasicClassDeclaration(string typeName, string @namespace, string attribute)
            {
                return $@"
    namespace {@namespace}
    {{
        [{attribute}]
        class {typeName}
        {{   
        }}
    }}";
            }
            private static string GetBasicStructDeclaration(string typeName, string @namespace, string attribute)
            {
                return $@"
    namespace {@namespace}
    {{
        [{attribute}]
        struct {typeName}
        {{   
        }}
    }}";
            }
            private static string GetBasicInterfaceDeclaration(string typeName, string @namespace, string attribute)
            {
                return $@"
    namespace {@namespace}
    {{
        [{attribute}]
        interface {typeName}
        {{   
        }}
    }}";
            }
        }

        public class GeneratorTheoryData : ITheoryDatum
        {
            public string Description { get; set; }

            public GeneratorSourcePackage SourcePackage { get; set; }

            public DiagnosticResult[] ExpectedDiagnostics { get; set; }

            public object[] ToParameterArray()
            {
                return
                    new object[]
                    {
                        Description,
                        SourcePackage,
                        ExpectedDiagnostics
                    };
            }
        }
    }
}