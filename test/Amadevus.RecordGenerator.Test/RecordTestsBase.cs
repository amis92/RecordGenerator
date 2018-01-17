using Amadevus.RecordGenerator.TestsBase;
using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.Test
{
    public abstract class RecordTestsBase
    {
        public const string ContainerId = "cont1";
        public const string ContainerName = "container name";
        public const string ItemId = "item1";
        public const string ItemName = "item name";

        public static Container CreateContainerEmpty()
        {
            return new Container.Builder
            {
                Id = ContainerId,
                Name = ContainerName
            }.ToImmutable();
        }

        public static Container CreateContainerWithOneItem()
        {
            return new Container.Builder
            {
                Id = ContainerId,
                Name = ContainerName,
                Items = ImmutableArray.Create(CreateItem())
            }.ToImmutable();
        }

        public static Item CreateItem()
        {
            return new Item.Builder
            {
                Id = ItemId,
                Name = ItemName
            }.ToImmutable();
        }
    }
}
