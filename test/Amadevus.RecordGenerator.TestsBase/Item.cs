using System;

namespace Amadevus.RecordGenerator.TestsBase
{
    [Record(Features.Default | Features.Equality)]
    public sealed partial class Item
    {
        public string Id { get; }

        public string Name { get; }

        // below properties should not be included in record's Descriptor

        public string CalculatedDirectExpressionBody => Id + Name;

        public string CalculatedAccessorExpressionBody
        {
            get => Id + Name;
        }

        public string CalculatedAccessorBlockBody
        {
            get { return Id + Name; }
        }

        partial void OnConstructed()
        {
            if (Name is null) throw new ArgumentNullException(nameof(Name));
        }
    }
}
