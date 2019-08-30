using System.Collections.Generic;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record]
    public sealed partial class GenericRecord<T>
    {
        public T Thing { get; }

        public System.Collections.Immutable.ImmutableArray<T> Things { get; }

        // regression test for a bug that crashed generation for the type below (#32)
        public System.Collections.Immutable.ImmutableArray<IReadOnlyList<Item>> RecordTree { get; }
    }
}
