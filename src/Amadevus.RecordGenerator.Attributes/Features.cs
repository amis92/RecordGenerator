using System;

namespace Amadevus.RecordGenerator
{
    /// <summary>
    /// Specifies record features that should be generated.
    /// </summary>
    [Flags]
    public enum Features
    {
        /// <summary>
        /// Zero value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Record constructor that takes and assigns all read-only properties.
        /// </summary>
        Constructor = 0b_1,

        /// <summary>
        /// With* method per property that creates new record instance with
        /// provided value assigned to the associated property. Also includes
        /// generation of <c>Update</c> method.
        /// </summary>
        Withers = 0b_10,

        /// <summary>
        /// <see cref="object.ToString"/> override that generates friendly,
        /// anonymous-type-like output.
        /// </summary>
        ToString = 0b_100,

        /// <summary>
        /// <c>Builder</c> nested class - a simple POCO with all record properties,
        /// but read-write (getter and setter). Also creates two mapping methods:
        /// <c>MyRecord.ToBuilder</c> and <c>MyRecord.Builder.ToImmutable</c>.
        /// </summary>
        Builder = 0b_1000,

        /// <summary>
        /// <c>Deconstruct</c> method:
        /// <see href="https://docs.microsoft.com/pl-pl/dotnet/csharp/deconstruct#deconstructing-user-defined-types"/>
        /// </summary>
        Deconstruct = 0b_10000,

        /// <summary>
        /// Default feature set.
        /// </summary>
        Default = Constructor | Withers | ToString | Builder | Deconstruct,

        //ObjectEquals = 0b_100000,
        //EquatableEquals = 0b_1000000,
        //OperatorEquals = 0b_10000000,
        //Equality = ObjectEquals | EquatableEquals | OperatorEquals
    }
}