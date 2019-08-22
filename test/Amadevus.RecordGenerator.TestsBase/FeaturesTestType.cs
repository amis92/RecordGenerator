using System;

namespace Amadevus.RecordGenerator.TestsBase
{
    public partial class FeatureContainer
    {
        [Record(Features.Constructor)]
        public partial class FeatureConstructor
        {
            public string Name { get; }

            // If Validate has no implementation it'll be removed
            // and we won't see it in reflection anyway.
            // But since we have to implement it, it's already testing
            // that the generated partial declaration exists during compilation.
            partial void OnConstructed()
            {
                if (Name is null) throw new System.ArgumentNullException(nameof(Name));
            }
        }

        [Record(Features.Withers)]
        public partial class FeatureWithers
        {
            // ctor needed for Update method to compile
            public FeatureWithers(string name) { }

            public string Name { get; }
        }

        [Record(Features.ToString)]
        public partial class FeatureToString
        {
            public string Name { get; }
        }

        [Record(Features.Builder)]
        public partial class FeatureBuilder
        {
            // ctor needed for ToImmutable method to compile
            public FeatureBuilder(string name) { }

            public string Name { get; }
        }

        [Record(Features.Deconstruct)]
        public partial class FeatureDeconstruct
        {
            public string Name { get; }
        }

        [Record(Features.EquatableEquals)]
        public sealed partial class FeatureEquatableEquals
        {
            public string Name { get; }
        }

        [Record(Features.ObjectEquals)]
        public sealed partial class FeatureObjectEquals
        {
            public string Name { get; }
        }

        [Record(Features.OperatorEquals)]
        public sealed partial class FeatureOperatorEquals
        {
            public string Name { get; }

            // Prevent warning CS0661
            public override bool Equals(object obj) => true;

            // Prevent warning CS0661
            public override int GetHashCode() => 0;
        }
    }
}
