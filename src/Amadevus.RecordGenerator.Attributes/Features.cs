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
        Deconstruct = 0b_1000_0,

        /// <summary>
        /// Default feature set.
        /// </summary>
        Default = Constructor | Withers | ToString | Builder | Deconstruct,

        /// <summary>
        /// <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode()"/> overrides. 
        /// <see cref="object.Equals(object)"/> implements an equality comparison which compares 
        /// all record entries properties.
        /// of the other <see cref="object"/> with it's own values. 
        /// <see cref="object.GetHashCode()"/> override which calculates a hash code using all record properties.
        /// <see cref="object.GetHashCode()"/>
        /// </summary>
        /// <remarks>
        /// <see cref="object.Equals(object)"/> returns <see langword="true" /> if all values match 
        /// and returns <see langword="false"/> if <see langword="null" /> is passed, another type is passed or at least one value doesn't mach.
        /// </remarks>
        ObjectEquals = 0b_1000_00,

        /// <summary>
        /// <see cref="IEquatable{T}.Equals(T)"/> implementation that provides a call to the 
        /// <see cref="object.Equals(object)"/> method in a type safe way. 
        /// </summary>
        EquatableEquals = 0b_1000_000,

        /// <summary>
        /// Overrides the <see langword="==" /> operator by using the <see cref="object.Equals(object)"/> method.
        /// Is dependent on the feature <see cref="ObjectEquals"/>.
        /// </summary>
        OperatorEquals = 0b_1000_0000,

        /// <summary>
        /// Equality feature set which inlcudes 
        /// <see cref="ObjectEquals"/>, <see cref="EquatableEquals"/> and <see cref="OperatorEquals"/>.
        /// </summary>
        Equality = ObjectEquals | EquatableEquals | OperatorEquals
    }
}