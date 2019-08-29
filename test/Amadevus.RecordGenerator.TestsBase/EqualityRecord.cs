using System.Text.RegularExpressions;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record(Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class EqualityRecordWithEquatableAndObjectEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.ObjectEquals | Features.OperatorEquals)]
    public sealed partial class EqualityRecordWithObjectAndOperatorEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }
}
