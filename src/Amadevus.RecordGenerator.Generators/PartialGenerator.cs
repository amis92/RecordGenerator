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
        public static IPartialGenerator Create(Func<RecordDescriptor, Features, PartialGenerationResult> generator) =>
            new DelegatingPartialGenerator(generator);

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, Features, PartialGenerationResult> generator) =>
            Create(generator).IntersectFeatures(implementedFeatures);

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, PartialGenerationResult> generator) =>
            Create(implementedFeatures, (descriptor, _) => generator(descriptor));

        public static IPartialGenerator IntersectFeatures(this IPartialGenerator generator, Features features) =>
            Create((rd, fs) => (fs & features) != Features.None
                             ? generator.Generate(rd, fs)
                             : PartialGenerationResult.Empty);

        public static IPartialGenerator Member(Features features, Func<RecordDescriptor, MemberDeclarationSyntax> member) =>
            Create(features, descriptor => PartialGenerationResult.Empty.AddMember(member(descriptor)));

        public static IPartialGenerator Combine(params IPartialGenerator[] generators) =>
            Create((descriptor, features) =>
                generators.Aggregate(PartialGenerationResult.Empty, (r, g) => r.Add(g.Generate(descriptor, features) ?? PartialGenerationResult.Empty)));

        private sealed class DelegatingPartialGenerator : IPartialGenerator
        {
            private readonly Func<RecordDescriptor, Features, PartialGenerationResult> generator;

            public DelegatingPartialGenerator(Func<RecordDescriptor, Features, PartialGenerationResult> generator)
            {
                this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
            }

            public PartialGenerationResult Generate(RecordDescriptor descriptor, Features features) =>
                generator(descriptor, features);
        }
    }
}
