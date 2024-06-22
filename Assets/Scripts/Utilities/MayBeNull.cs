namespace Deenote.Utilities
{
    /// <summary>
    /// A monad used to explicitly mark if a value may be null,
    /// please use this when null is a invalid value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct MayBeNull<T> where T : class
    {
        private readonly T _value;

        public bool HasValue => _value is not null;

        public T Value => _value;

        public MayBeNull(T value) => _value = value;

        public static implicit operator MayBeNull<T>(T value) => new(value);
    }
}