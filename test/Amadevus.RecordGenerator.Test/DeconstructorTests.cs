using Amadevus.RecordGenerator.TestsBase;
using System.Collections.Immutable;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class DeconstructorTests : RecordTestsBase
    {
        [Fact]
        public void Deconstruct_ReturnsValidValues()
        {
            var container = CreateContainerWithOneItem();

            container.Deconstruct(out var id, out var name, out var items);

            AssertValuesAreInitial(id, name, items);
        }

        [Fact]
        public void DeconstructImplicit_ReturnsValidValues()
        {
            var container = CreateContainerWithOneItem();

            var (id, name, items) = container;

            AssertValuesAreInitial(id, name, items);
        }

        private static void AssertValuesAreInitial(string id, string name, ImmutableArray<Item> items)
        {
            Assert.Equal(ContainerId, id);
            Assert.Equal(ContainerName, name);
            Assert.Collection(
                items,
                x =>
                {
                    Assert.Equal(ItemId, x.Id);
                    Assert.Equal(ItemName, x.Name);
                });
        }
    }
}
