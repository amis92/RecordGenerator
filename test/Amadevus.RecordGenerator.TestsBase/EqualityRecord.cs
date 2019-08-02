using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record(Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class EqualityRecordWithEquatableAndObjectEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.ObjectEquals)]
    public sealed partial class EqualityRecordWithObjectEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }
}
