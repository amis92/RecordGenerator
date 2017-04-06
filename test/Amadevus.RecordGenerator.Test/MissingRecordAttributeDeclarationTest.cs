using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace Amadevus.RecordGenerator.Test
{
    [TestClass]
    public class MissingRecordAttributeDeclarationTest : GeneratorCodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void EmptyFile_NoDiagnostics()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void BasicClass_NoRecordAttributeDeclaration()
        {
            var test = @"
    namespace ConsoleApplication1
    {
        [Record]
        class TypeName
        {   
        }
    }";
            var expected1 = new DiagnosticResult(MissingRecordAttributeDeclarationDiagnostic.Descriptor, "TypeName", "Record")
            {
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 4, 10)
                        }
            };
            var expected2 = new DiagnosticResult(MissingRecordPartialDiagnostic.Descriptor, "TypeName")
            {
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 5, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected1, expected2);

            var recordAttribute = GenerateRecordAttributeDeclarationCodeFixProvider.RecordAttributeDeclarationSource("ConsoleApplication1");
            var sourcePackage = new GeneratorSourcePackage
            {
                AddedSource = recordAttribute,
                FixedSource = test,
                OldSource = test
            };
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
    }
}