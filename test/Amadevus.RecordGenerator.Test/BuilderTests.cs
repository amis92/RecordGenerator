using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class BuilderTests : RecordTestsBase
    {
        [Fact]
        public void ToBuilder_CopiesSimpleProperty()
        {
            var item = CreateItem();
            var builder = item.ToBuilder();
            Assert.Equal(item.Name, builder.Name);
        }

        [Fact]
        public void ToBuilder_CopiesCollectionProperty()
        {
            var container = CreateContainerWithOneItem();
            var builder = container.ToBuilder();
            Assert.Equal(container.Items, builder.Items);
        }
    }
}
