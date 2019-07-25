using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Amadevus.RecordGenerator.Generators
{
    public class RecordGeneratorOptions
    {
        public bool SkipEquality { get; }
        public bool SkipBuilder { get; } 
        public bool SkipDeconstruct { get; }

        public RecordGeneratorOptions(
            bool skipEquality, bool skipBuilder, bool skipDeconstruct)
        {
            SkipEquality = skipEquality;
            SkipBuilder = skipBuilder;
            SkipDeconstruct = skipDeconstruct;
        }

        public static RecordGeneratorOptions FromAttributeData(
            AttributeData attributeData)
        {
            return new RecordGeneratorOptionsReader().ReadOptions(attributeData); 
        }
    }

    public class RecordGeneratorOptionsReader
    {
        public RecordGeneratorOptions ReadOptions(AttributeData attributeData)
        {
            var attributeArguments = attributeData.AttributeClass.Constructors.Single()
                .Parameters.Select((parameter, index) => new
                {
                    parameter.Name,
                    attributeData.ConstructorArguments[index].Value
                });

            var optionsConstructor = typeof(RecordGeneratorOptions).GetConstructors().Single();
            var optionsConstructorParameters = optionsConstructor.GetParameters();

            return (RecordGeneratorOptions)optionsConstructor.Invoke(
                optionsConstructorParameters
                    .Select(p => attributeArguments.Single(a => a.Name.Equals(p.Name)).Value)
                    .ToArray()); 
        }
    }
}
