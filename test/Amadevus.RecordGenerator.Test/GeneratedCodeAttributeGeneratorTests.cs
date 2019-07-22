using Amadevus.RecordGenerator.TestsBase;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class GeneratedCodeAttributeGeneratorTests : RecordTestsBase
    {
        [Fact]
        public void ReflectedClass_HasGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);

            var attributeData = recordType.GetCustomAttributesData()
                .Single(d => d.AttributeType.Equals(typeof(GeneratedCodeAttribute)));
            var toolName = attributeData.ConstructorArguments.First().Value;
            var toolVersion = attributeData.ConstructorArguments.Skip(1).First().Value;

            var expectedName = "Amadevus.RecordGenerator";
            Assert.Equal(expectedName, toolName);
            var expectedVersion = GetType().Assembly.GetName().Version.ToString();
            Assert.Equal(expectedVersion, toolVersion);
        }
    }
}
