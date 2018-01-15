using System.Collections.Immutable;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record]
    public partial class ContainerCore
    {
        public string Id { get; }
        
        public string Name { get; }
        
        public ImmutableArray<ItemCore> Items { get; }
    }
}
