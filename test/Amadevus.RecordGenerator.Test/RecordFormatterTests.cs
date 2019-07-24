using System.Collections.Immutable;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    using System.Collections.Generic;
    using TestsBase;

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

        [Fact]
        public void With_TreeProperty()
        {
            const string foo = nameof(foo);
            const string bar = nameof(bar);

            var builder = new GenericRecord<string>.Builder
            {
                Thing      = foo + bar,
                Things     = ImmutableArray.Create(foo, bar),
                RecordTree = ImmutableArray.Create((IReadOnlyList<Item>)ImmutableArray.Create(CreateItem())),
            };

            var record = builder.ToImmutable();

            var shape = new
            {
                record.Thing,
                record.Things,
                record.RecordTree,
            };

            Assert.Equal(shape.ToString(), record.ToString());
        }
    }
}