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
        Deconstruct = 0b_1_0000,

        /// <summary>
        /// Default feature set.
        /// </summary>
        Default = Constructor | Withers | ToString | Builder | Deconstruct,

        /// <summary>
        /// <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode()"/> overrides
        /// - currently this requires the record class to be sealed.
        /// <see cref="object.Equals(object)"/> override compares all record properties with
        /// the corresponding record properties of the other object using either
        /// <see langword="=="/> for integral value types, or
        /// <see cref="System.Collections.Generic.EqualityComparer{T}.Default"/> for the others.
        /// <see cref="object.GetHashCode()"/> override calculates a hash code using all record properties.
        /// </summary>
        /// <remarks>
        /// <see cref="object.Equals(object)"/> returns <see langword="true" /> if and only if:
        /// other object is not <see langword="null" />, is of the same type, and all record
        /// properties' values match.
        /// If feature <see cref="EquatableEquals"/> is used as well, the implementation of
        /// <see cref="object.Equals(object)"/> will forward into <see cref="IEquatable{T}.Equals(T)"/>.
        /// </remarks>
        ObjectEquals = 0b_10_0000,

        /// <summary>
        /// <see cref="IEquatable{T}.Equals(T)"/> implementation that provides a call to the 
        /// <see cref="object.Equals(object)"/> method in a type safe way - currently this
        /// requires the record class to be sealed.
        /// </summary>
        EquatableEquals = 0b_100_0000,

        /// <summary>
        /// Overrides the <see langword="==" /> and <see langword="!=" /> operator by using
        /// the <see cref="object.Equals(object)"/> method.
        /// </summary>
        OperatorEquals = 0b_1000_0000,

        /// <summary>
        /// Equality feature set which inlcudes 
        /// <see cref="ObjectEquals"/>, <see cref="EquatableEquals"/> and <see cref="OperatorEquals"/>.
        /// </summary>
        Equality = ObjectEquals | EquatableEquals | OperatorEquals
    }
}