using CodeGeneration.Roslyn;
using System;
using System.Diagnostics;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
[CodeGenerationAttribute("Amadevus.RecordGenerator.Generators.RecordGenerator, Amadevus.RecordGenerator.Generators")]
[Conditional("CodeGeneration")]
public sealed class RecordAttribute : Attribute
{
    public bool SkipEquality { get; }
    public bool SkipBuilder { get; } 
    public bool SkipDeconstruct { get; }

    public RecordAttribute(
        bool skipEquality = false,
        bool skipBuilder = false,
        bool skipDeconstruct = false)
    {
        SkipEquality = skipEquality;
        SkipBuilder = skipBuilder;
        SkipDeconstruct = skipDeconstruct;
    }
}
