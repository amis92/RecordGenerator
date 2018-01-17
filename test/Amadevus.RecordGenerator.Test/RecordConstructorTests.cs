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
            new Item(Id: ItemId, Name: ItemName);
        }

        [Fact]
        public void Ctor_InvokesValidate_Throwing()
        {
            Assert.Throws<ArgumentNullException>(nameof(ValidatingRecord.Name),() => new ValidatingRecord(null));
        }

        [Fact]
        public void Ctor_InvokesValidate_Passing()
        {
            Assert.NotNull(new ValidatingRecord(ItemName));
        }
    }
}
