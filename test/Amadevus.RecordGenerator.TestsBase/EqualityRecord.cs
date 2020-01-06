using System.Text.RegularExpressions;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record(Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class EqualityRecordClassWithEquatableAndObjectEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.ObjectEquals | Features.OperatorEquals)]
    public sealed partial class EqualityRecordClassWithObjectAndOperatorEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.Equality)]
    public sealed partial class EqualityRecordClassWithEquality
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.EquatableEquals | Features.ObjectEquals)]
    public partial struct EqualityRecordStructWithEquatableAndObjectEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.ObjectEquals | Features.OperatorEquals)]
    public partial struct EqualityRecordStructWithObjectAndOperatorEquals
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }

    [Record(Features.Equality)]
    public partial struct EqualityRecordStructWithEquality
    {
        public string StringProperty { get; }
        public Regex RegexProperty { get; }
    }
}
