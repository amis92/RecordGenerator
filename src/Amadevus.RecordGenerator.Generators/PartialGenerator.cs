using System;
using System.Linq;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Amadevus.RecordGenerator.Generators
{
    internal interface IPartialGenerator
    {
        GenerationResult Generate(RecordDescriptor descriptor, Features features);
    }

    static class PartialGenerator
    {
        public static IPartialGenerator Create(Func<RecordDescriptor, Features, GenerationResult> generator) =>
            new DelegatingPartialGenerator(generator);

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, Features, GenerationResult> generator) =>
            Create(generator).IntersectFeatures(implementedFeatures);

        public static IPartialGenerator Create(Features implementedFeatures, Func<RecordDescriptor, GenerationResult> generator) =>
            Create(implementedFeatures, (descriptor, _) => generator(descriptor));

        public static IPartialGenerator IntersectFeatures(this IPartialGenerator generator, Features features) =>
            Create((rd, fs) => (fs & features) != Features.None
                             ? generator.Generate(rd, fs)
                             : null);

        public static IPartialGenerator Member(Features features, Func<RecordDescriptor, MemberDeclarationSyntax> member) =>
            Create(features, descriptor => GenerationResult.Empty.AddMember(member(descriptor)));

        public static IPartialGenerator Combine(params IPartialGenerator[] generators) =>
            Create((descriptor, features) =>
                generators.Aggregate(GenerationResult.Empty, (r, g) => r.Add(g.Generate(descriptor, features) ?? GenerationResult.Empty)));

        private sealed class DelegatingPartialGenerator : IPartialGenerator
        {
            private readonly Func<RecordDescriptor, Features, GenerationResult> generator;

            public DelegatingPartialGenerator(Func<RecordDescriptor, Features, GenerationResult> generator)
            {
                this.generator = generator ?? throw new ArgumentNullException(nameof(generator));
            }

            public GenerationResult Generate(RecordDescriptor descriptor, Features features) =>
                generator(descriptor, features);
        }
    }
}
