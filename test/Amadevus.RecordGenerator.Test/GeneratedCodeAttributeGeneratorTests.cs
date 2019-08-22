using System;
using System.CodeDom.Compiler;
using System.Linq.Expressions;
using System.Reflection;
using Amadevus.RecordGenerator.TestsBase;
using FluentAssertions;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class GeneratedCodeAttributeGeneratorTests : RecordTestsBase
    {
        [Fact]
        public void ReflectedClass_DoesNotHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);

            recordType.Should()
                .NotBeDecoratedWith(GeneratedCodeAttributeWithCorrectToolNameAndVersion);
        }

        [Fact]
        public void ReflectedClassConstructor_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var constructor = recordType.GetConstructor(new[] { typeof(string), typeof(string) });

            constructor.Should()
                .BeDecoratedWith(GeneratedCodeAttributeWithCorrectToolNameAndVersion);
        }

        [Fact]
        public void ReflectedClassMethod_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var method = recordType.GetMethod(nameof(Item.Update));

            method.Should()
                .BeDecoratedWith(GeneratedCodeAttributeWithCorrectToolNameAndVersion);
        }

        [Fact]
        public void ReflectedClassOperator_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var method = recordType.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static);

            method.Should()
                .BeDecoratedWith(GeneratedCodeAttributeWithCorrectToolNameAndVersion);
        }

        [Fact]
        public void ReflectedClassNestedType_DoesNotHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var nestedType = recordType.GetNestedType(nameof(Item.Builder));

            nestedType.Should()
                .NotBeDecoratedWith(GeneratedCodeAttributeWithCorrectToolNameAndVersion);
        }

        [Fact]
        public void ReflectedClassNestedTypeProperty_DoesHaveGeneratedCodeAttribute()
        {
            var recordType = typeof(Item);
            var nestedTypeMember = recordType.GetNestedType(nameof(Item.Builder))
                .GetProperty(nameof(Item.Builder.Name));

            nestedTypeMember.Should()
                .BeDecoratedWith(GeneratedCodeAttributeWithCorrectToolNameAndVersion);
        }

        private static Expression<Func<GeneratedCodeAttribute, bool>> GeneratedCodeAttributeWithCorrectToolNameAndVersion =>
            attribute => attribute.Tool == "Amadevus.RecordGenerator"
            && attribute.Version == ThisAssembly.AssemblyVersion;
    }
}
