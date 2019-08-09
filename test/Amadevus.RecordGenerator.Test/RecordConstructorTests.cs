using Amadevus.RecordGenerator.TestsBase;
using System;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class RecordConstructorTests : RecordTestsBase
    {
        [Fact]
        public void Ctor_HasParametersNamedLikeProperties()
        {
            new Item(id: ItemId, name: ItemName);
        }

        [Fact]
        public void Ctor_InvokesOnConstructed_Throwing()
        {
            Assert.Throws<ArgumentNullException>(nameof(Item.Name), () => new ValidatingRecord(null, "a"));
        }

        [Fact]
        public void Ctor_InvokesOnConstructed_Passing()
        {
            Assert.NotNull(new ValidatingRecord(ItemName, "test"));
        }
    }
}
