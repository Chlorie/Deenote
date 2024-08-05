using System;
using System.Buffers;

namespace Deenote.Utilities.Robustness
{
    public readonly ref struct PooledSpan<T>
    {
        private readonly ArrayPool<T> _pool;
        private readonly T[] _array;
        private readonly int _length;

        public PooledSpan(ArrayPool<T> pool, int length)
        {
            _pool = pool;
            _array = _pool.Rent(length);
            _length = length;
        }

        public PooledSpan(int length) : this(ArrayPool<T>.Shared, length) { }

        public Span<T> Span => _length == 0 ? Span<T>.Empty : new Span<T>(_array, 0, _length);

        public void Dispose()
        {
            _pool?.Return(_array);
        }

        public ReadOnlyView ToReadOnly() => new(this);

        public readonly ref struct ReadOnlyView
        {
            private readonly PooledSpan<T> _span;

            public ReadOnlyView(PooledSpan<T> span) => _span = span;

            public ReadOnlySpan<T> Span => _span.Span;

            public void Dispose() => _span.Dispose();
        }
    }
}