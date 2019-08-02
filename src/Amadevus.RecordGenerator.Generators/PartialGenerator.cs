using System;
using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator.Generators
{
    internal interface IPartialGenerator
    {
        PartialGenerationResult Generate(RecordDescriptor descriptor, Features features);
    }

    static class PartialGenerator
    {
        /// <summary>
        /// Creates a partial generator given a generator function.
        /// </summary>

        public static IPartialGenerator Create(Func<RecordDescriptor, Features, PartialGenerationResult> generator) =>
            new DelegatingPartialGenerator(generator);

        /// <summary>
        /// Creates a partial generator given a generator function that is
        /// invoked if the features it implements intersect with the requested
        /// feature set at time of generation. Otherwise it returns an empty
        /// result.
        /// </summary>

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, Features, PartialGenerationResult> generator) =>
            Create(generator).IntersectFeatures(implementedFeatures);

        /// <summary>
        /// Creates a partial generator given a generator function that is
        /// invoked if the features it implements intersect with the requested
        /// feature set at time of generation. Otherwise it returns an empty
        /// result. The generator function only receives the record
        /// descriptor as its sole input argument.
        /// </summary>

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, PartialGenerationResult> generator) =>
            Create(implementedFeatures, (descriptor, _) => generator(descriptor));

        /// <summary>
        /// Creates a partial generator that invokes another if the features
        /// given features intersect with the requested feature set at time of
        /// generation. Otherwise it returns an empty result.
        /// </summary>

        public static IPartialGenerator IntersectFeatures(this IPartialGenerator generator, Features features) =>
            Create((rd, fs) => (fs & features) != Features.None
                             ? generator.Generate(rd, fs)
                             : PartialGenerationResult.Empty);

        /// <summary>
        /// Creates a partial generator that generates only a single member.
        /// </summary>

        public static IPartialGenerator Member(Features features, Func<RecordDescriptor, MemberDeclarationSyntax> member) =>
            Create(features, descriptor => PartialGenerationResult.Empty.AddMember(member(descriptor)));

        /// <summary>
        /// Combines several partial generators into one by aggregating their
        /// results.
        /// </summary>

        public static IPartialGenerator Combine(params IPartialGenerator[] generators) =>
            Create((descriptor, features) =>
                generators.Aggregate(PartialGenerationResult.Empty, (r, g) => r.Add(g.Generate(descriptor, features) ?? PartialGenerationResult.Empty)));

        private sealed class DelegatingPartialGenerator : IPartialGenerator
        {
            private readonly Func<RecordDescriptor, Features, PartialGenerationResult> generator;

            public DelegatingPartialGenerator(Func<RecordDescriptor, Features, PartialGenerationResult> generator) =>
                this.generator = generator ?? throw new ArgumentNullException(nameof(generator));

            public PartialGenerationResult Generate(RecordDescriptor descriptor, Features features) =>
                generator(descriptor, features);
        }
    }
}
