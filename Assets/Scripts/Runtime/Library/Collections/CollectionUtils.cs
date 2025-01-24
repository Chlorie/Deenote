#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Deenote.Library.Collections
{
    public static class CollectionUtils
    {
        #region Modifiers

        public static void MoveTo<T>(this Span<T> span, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return;

            var val = span[fromIndex];

            if (fromIndex > toIndex) {
                var len = fromIndex - toIndex;
                span.Slice(toIndex, len).CopyTo(span.Slice(toIndex + 1, len));
            }
            else {
                var len = toIndex - fromIndex;
                span.Slice(fromIndex + 1, len).CopyTo(span.Slice(fromIndex, len));
            }

            span[toIndex] = val;
        }

        /// <summary>
        /// Get a value from a dictionary or add it if it doesn't exist.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">
        /// A factory to produce the value if the specified key does not exist in the dictionary.
        /// </param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// If the key existed in the original dictionary, returns <see langword="true"/>;
        /// otherwise, returns <see langword="false"/>.
        /// </returns>
        public static bool GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            TKey key, Func<TValue> valueFactory, out TValue value) where TKey : notnull
        {
            if (dict.TryGetValue(key, out value)) return true;
            dict[key] = value = valueFactory();
            return false;
        }

        #endregion

        #region Search

        public static bool IsSameForAll<T, TValue>(this ReadOnlySpan<T> list,
            Func<T, TValue> valueGetter,out TValue? value, IEqualityComparer<TValue>? comparer = null)
        {
            switch (list) {
                case { Length: 0 }:
                    value = default;
                    return true;
                case { Length: 1 }:
                    value = valueGetter(list[0]);
                    return true;
            }

            value = valueGetter(list[0]);
            comparer ??= EqualityComparer<TValue>.Default;

            foreach (T v in list) {
                if (comparer.Equals(value, valueGetter(v))) continue;
                value = default;
                return false;
            }

            return true;
        }

        public static int LinearSearch<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
            => span.LinearSearch(new ComparerComparable<T, TComparer>(item, comparer));

        public static int LinearSearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        {
            for (int i = 0; i < span.Length; i++) {
                var res = item.CompareTo(span[i]);
                if (res < 0)
                    return ~i;
                if (res == 0)
                    return i;
            }
            return ~span.Length;
        }

        public static int LinearSearchFromEnd<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
           => span.LinearSearchFromEnd(new ComparerComparable<T, TComparer>(item, comparer));

        public static int LinearSearchFromEnd<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        {
            for (int i = span.Length - 1; i >= 0; i--) {
                var res = item.CompareTo(span[i]);
                if (res > 0)
                    return ~(i + 1);
                if (res == 0)
                    return i;
            }
            return ~0;
        }

        public static int LinearSearchFromEnd<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
            => LinearSearchFromEnd((ReadOnlySpan<T>)span, item);

        #endregion


        #region Linq

        public static SpanOfTypeIterator<T, TResult> OfType<T, TResult>(this ReadOnlySpan<T> values) where TResult : T
            => new(values);

        #endregion

        public ref struct SpanOfTypeIterator<T, TResult> where TResult : T
        {
            private readonly ReadOnlySpan<T> _span;
            private int _index;

            internal SpanOfTypeIterator(ReadOnlySpan<T> span)
            {
                _span = span;
                _index = -1;
            }

            public readonly SpanOfTypeIterator<T, TResult> GetEnumerator() => this;

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

        private readonly struct ComparerComparable<T, TComparer> : IComparable<T> where TComparer : IComparer<T>
        {
            private readonly T _value;
            private readonly TComparer _comparer;

            public ComparerComparable(T value, TComparer comparer)
            {
                _value = value;
                _comparer = comparer;
            }

            public int CompareTo(T? other) => _comparer.Compare(_value, other!);
        }
    }
}