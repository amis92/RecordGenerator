using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
[CodeGenerationAttribute("Amadevus.RecordGenerator.CodeGenerator")]
[Conditional("CodeGeneration")]
public sealed class RecordAttribute : Attribute
{
}
