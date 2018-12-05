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

    public partial class X
    {
        public X(string thing)
        {
            Validate(ref thing);
            Thing = thing;
        }

        public string Thing { get; }

        static partial void Validate(ref string thing);
        static partial void Validate(ref string thing)
        {
            if (thing is null) throw new System.Exception();
        }
    }

    partial class X
    {
    }
}
