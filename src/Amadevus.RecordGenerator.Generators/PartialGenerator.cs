using System;
using System.Linq;

namespace Amadevus.RecordGenerator.Generators
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal interface IPartialGenerator
    {
        Generation Generate(RecordDescriptor descriptor, Features features);
    }

    static class PartialGenerator
    {
        public static IPartialGenerator Create(Func<RecordDescriptor, Features, Generation> generator) =>
            new DelegatingPartialGenerator(generator);

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, Features, Generation> generator) =>
            Create(generator).IntersectFeatures(implementedFeatures);

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, Generation> generator) =>
            Create(implementedFeatures, (descriptor, _) => generator(descriptor));

        public static IPartialGenerator IntersectFeatures(this IPartialGenerator generator, Features features) =>
            Create((rd, fs) => (fs & features) != Features.None
                             ? generator.Generate(rd, fs)
                             : null);

        public static IPartialGenerator Member(Features features, Func<RecordDescriptor, MemberDeclarationSyntax> member) =>
            Create(features, descriptor => Generation.Empty.AddMember(member(descriptor)));

        public static IPartialGenerator Combine(params IPartialGenerator[] generators) =>
            Create((descriptor, features) =>
                generators.Aggregate(Generation.Empty, (r, g) => r.Add(g.Generate(descriptor, features) ?? Generation.Empty)));

        private sealed class DelegatingPartialGenerator : IPartialGenerator
        {
            private readonly Func<RecordDescriptor, Features, Generation> generator;

            public DelegatingPartialGenerator(Func<RecordDescriptor, Features, Generation> generator)
            {
                this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
            }

            public Generation Generate(RecordDescriptor descriptor, Features features) =>
                generator(descriptor, features);
        }
    }
}
