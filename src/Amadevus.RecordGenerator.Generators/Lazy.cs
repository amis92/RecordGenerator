using System;

namespace Amadevus.RecordGenerator.Generators
{
    static class Lazy
    {
        /// <remarks>
        /// This method assumes that <paramref name="factory"/> is idempotent.
        /// That is, it will produce the same resulting value of type
        /// <typeparamref name="TResult"/> given the same argument of
        /// <paramref name="arg"/>. The function for <paramref name="factory"/>
        /// must be prepared to be called more than once if initialization
        /// occurs in parallel on more than one thread. Once initialized, all
        /// thread will see the same resulting value.
        /// </remarks>

        public static TResult EnsureInitialized<TArg, TResult>(
            ref (bool, TResult) target,
            TArg arg,
            Func<TArg, TResult> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            var (initialized, value) = target;
            if (!initialized)
                target = (true, value = factory(arg));
            return value;
        }
    }
}
