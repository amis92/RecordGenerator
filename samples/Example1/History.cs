using Amadevus.RecordGenerator;

namespace Example
{
    public partial class History
    {
        [Record]
        private partial struct Entry
        {
            public int Id { get; }

            public string Name { get; }

            public string Details { get; }
        }
    }
}