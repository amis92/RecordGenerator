using Amadevus.RecordGenerator.TestsBase;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class GeneratedCodeAttributeGeneratorTests : RecordTestsBase
    {
        [Fact]
        public void ReflectedClass_DoesNotHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);

            var result = HasCorrectCodeGeneratedAttribute(recordType.GetCustomAttributesData());

            Assert.False(result);
        }

        [Fact]
        public void ReflectedClassMethod_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var method = recordType.GetMethod(nameof(Item.Update));

            var result = HasCorrectCodeGeneratedAttribute(method.GetCustomAttributesData());

            Assert.True(result);
        }

        [Fact]
        public void ReflectedClassNestedType_DoesNotHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var nestedType = recordType.GetNestedType(nameof(Item.Builder));

            var result = HasCorrectCodeGeneratedAttribute(nestedType.GetCustomAttributesData());

            Assert.False(result);
        }

        [Fact]
        public void ReflectedClassNestedTypeProperty_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var nestedTypeMethod = recordType.GetNestedType(nameof(Item.Builder))
                .GetProperty(nameof(Item.Builder.Name));

            var result = HasCorrectCodeGeneratedAttribute(nestedTypeMethod.GetCustomAttributesData());

            Assert.True(result);
        }

        [Fact]
        public void ReflectedClassConstructor_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var constructor = recordType.GetConstructor(new[] { typeof(string), typeof(string) });

            var result = HasCorrectCodeGeneratedAttribute(constructor.GetCustomAttributesData());

            Assert.True(result);
        }

        private bool HasCorrectCodeGeneratedAttribute(IList<CustomAttributeData> attributeData)
        {
            var codeGeneratedAttributeData = attributeData
                .Where(d => d.AttributeType.Equals(typeof(GeneratedCodeAttribute)));

            if (!codeGeneratedAttributeData.Any()) return false;

            var codeGeneratedAttribute = codeGeneratedAttributeData.Single();

            var toolName = codeGeneratedAttribute.ConstructorArguments.First().Value;
            var expectedToolName = "Amadevus.RecordGenerator";
            if (!toolName.Equals(expectedToolName)) return false;

            var toolVersion = codeGeneratedAttribute.ConstructorArguments.Skip(1).First().Value;
            var expectedToolVersion = GetType().Assembly.GetName().Version.ToString();
            if (!toolVersion.Equals(expectedToolVersion)) return false;

            return true;
        }
    }
}
