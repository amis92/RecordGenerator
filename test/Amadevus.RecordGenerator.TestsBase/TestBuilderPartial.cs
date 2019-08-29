namespace Amadevus.RecordGenerator.TestsBase
{
    // test to check builder is partial, won't compile otherwise
    [Record]
    sealed partial class TestBuilderPartial
    {
        public string Name { get; }

        partial class Builder
        {

        }

        private void CallBuilder()
        {
            var builder = new Builder
            {
                Name = "test"
            };
        }
    }
}
