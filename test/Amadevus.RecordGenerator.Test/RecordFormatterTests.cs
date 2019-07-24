using System.Collections.Immutable;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class RecordFormatterTests : RecordTestsBase
    {
        [Fact]
        public void With_SimpleProperty()
        {
            var item = CreateItem();
            var shape = new
            {
                Id = ItemId,
                Name = ItemName
            };
            Assert.Equal(shape.ToString(), item.ToString());
        }

        [Fact]
        public void With_CollectionProperty()
        {
            var item = CreateItem();
            var container = CreateContainerEmpty();
            var items = new[] { item }.ToImmutableArray();
            container.WithItems(items);
            var shape = new
            {
                Id = ContainerId,
                Name = ContainerName,
                Items = items.ToString(),
            };
            Assert.Equal(shape.ToString(), container.ToString());
        }
    }
}