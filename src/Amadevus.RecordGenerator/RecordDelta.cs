using System;

namespace Amadevus.RecordGenerator
{
    /// <summary>
    /// For testing purposes if creating With mutator with all properties wrapped like that may make sense.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal struct RecordDelta<T>
    {
        private readonly bool _isNotDefault;
        private readonly T _value;

        public RecordDelta(T value)
        {
            _isNotDefault = true;
            _value = value;
        }

        public bool IsDefault => !_isNotDefault;

        public T Value => IsDefault ? throw new InvalidOperationException($"{nameof(Value)} was not set") : _value;

        public static implicit operator RecordDelta<T>(T value)
        {
            return new RecordDelta<T>(value);
        }
    }
}
