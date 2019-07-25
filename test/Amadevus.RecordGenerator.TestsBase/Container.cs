using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record]
    public sealed partial class Container
    {
        public string Id { get; }
        
        public string Name { get; }
        
        public ImmutableArray<Item> Items { get; }
    }
}
