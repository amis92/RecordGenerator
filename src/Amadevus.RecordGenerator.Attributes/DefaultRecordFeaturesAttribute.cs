using System;
using System.Diagnostics;

namespace Amadevus.RecordGenerator
{
    /// <summary>
    /// This attribute defines default <see cref="Features"/> across assembly
    /// for usages of the non-parametrized <see cref="RecordAttribute()"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public sealed class DefaultRecordFeaturesAttribute : Attribute
    {
        /// <summary>
        /// For the annotated assembly, overrides default value of <see cref="RecordAttribute.Features"/>.
        /// </summary>
        /// <param name="assemblyDefaultFeatures">Value for <see cref="AssemblyDefaultFeatures"/>.</param>
        public DefaultRecordFeaturesAttribute(Features assemblyDefaultFeatures)
        {
            AssemblyDefaultFeatures = assemblyDefaultFeatures;
        }

        /// <summary>
        /// <see cref="RecordAttribute.Features"/> default value override.
        /// </summary>
        public Features AssemblyDefaultFeatures { get; }
    }
}