using Amadevus.RecordGenerator.TestsBase;
using FluentAssertions;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class FeaturesTests
    {
        [Fact]
        public void Given_Constructor_then_ctor_is_generated()
        {
            var ctors = typeof(FeatureContainer.FeatureConstructor).GetConstructors();

            ctors.Should().ContainSingle(x => x.GetParameters().Length == 1, "generated ctor has 1 parameter");
        }

        [Theory]
        [InlineData(Features.Constructor, ".ctor")]
        [InlineData(Features.WithPerProperty, "WithName")]
        [InlineData(Features.WithPerProperty, "Update")]
        [InlineData(Features.Builder, "Builder")]
        [InlineData(Features.Builder, "ToBuilder")]
        [InlineData(Features.ToString, "ToString")]
        [InlineData(Features.Deconstruct, "Deconstruct")]
        public void Given_Feature_then_member_is_generated(Features feature, string memberName)
        {
            var containerType = typeof(FeatureContainer);
            var featureType = containerType.GetNestedType("Feature" + feature.ToString());

            var members = featureType.GetMember(memberName);

            members.Should().HaveCount(1);
            members.Should().ContainSingle(x => x.DeclaringType == featureType, "inherited members don't count");
        }
    }
}
