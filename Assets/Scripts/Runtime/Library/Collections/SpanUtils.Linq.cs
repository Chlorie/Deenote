#nullable enable

using System;
using System.Runtime.CompilerServices;

namespace Deenote.Library.Collections;
public static partial class SpanUtils
{
    #region Linq

    public static OfTypeIterator<T, TResult> OfType<T, TResult>(this ReadOnlySpan<T> values) where TResult : T
        => new(values);

    #endregion

    public ref struct OfTypeIterator<T, TResult> where TResult : T
    {
        private readonly ReadOnlySpan<T> _span;
        private int _index;

        internal OfTypeIterator(ReadOnlySpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        public readonly OfTypeIterator<T, TResult> GetEnumerator() => this;

        public readonly ref readonly TResult Current => ref Unsafe.As<T, TResult>(ref Unsafe.AsRef(in _span[_index]));

        public bool MoveNext()
        {
            int index = _index + 1;
            while (index < _span.Length) {
                if (_span[index] is TResult) {
                    _index = index;
                    return true;
                }
                index++;
            }
            _index = index;
            return false;
        }
    }
}