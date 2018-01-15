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
    }
}
