using Amadevus.RecordGenerator.TestsBase;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class RecordClassTests : RecordTestsBase
    {
        [Fact]
        public void ReflectedClass_HasNo_RecordAttribute()
        {
            var recordType = typeof(Item);

            var recordAttributes = recordType.GetCustomAttributes(typeof(RecordAttribute), false);

            Assert.Empty(recordAttributes);
        }
    }
}
