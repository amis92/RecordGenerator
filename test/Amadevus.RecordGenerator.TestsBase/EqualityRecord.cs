using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record(Features.Equality)]
    public sealed partial class EqualityRecord
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }
}
