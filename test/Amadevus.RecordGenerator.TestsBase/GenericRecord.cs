using System;
using System.Collections.Generic;
using System.Text;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record]
    public partial class GenericRecord<T>
    {
        public T Thing { get; }

        public System.Collections.Immutable.ImmutableArray<T> Things { get; }

        // regression test for a bug that crashed generation for the type below (#32)
        public System.Collections.Immutable.ImmutableArray<IReadOnlyList<Item>> RecordTree { get; }
    }
}
