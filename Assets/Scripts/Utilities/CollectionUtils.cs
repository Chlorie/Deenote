using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Deenote.Utilities
{
    public static class CollectionUtils
    {
        #region Modifiers

        public static void AddRange<T>(this List<T> list, ReadOnlySpan<T> collection)
        {
            foreach (var item in collection) {
                list.Add(item);
            }
        }

        public static void RemoveRange<T>(this List<T> list, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            list.RemoveRange(offset, length);
        }

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

        public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex)
            => list.AsSpan().MoveTo(fromIndex, toIndex);

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

        #region Views

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            // The underlying array is the first field of List<T>, so if we
            // force cast it into another reference type whose first field
            // is T[], we can get the underlying array.

            // I'm not sure if the layout of List<T> is the same for all runtimes,
            // so please do test after changing the runtime
            var provider = Unsafe.As<StrongBox<T[]>>(list);
            var array = provider.Value;
            return array.AsSpan(0, list.Count);
        }

        /// <summary>
        /// Returns a view through which modifying the list will keep elements in order.
        /// <br/>
        /// Make sure your list is in order
        /// </summary>
        public static SortedModifier<T> GetSortedModifier<T>(this List<T> list, IComparer<T>? comparer = null)
            => new(list, comparer ?? Comparer<T>.Default);

        #endregion

        #region Search

        public static int Find<T>(this IReadOnlyList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++) {
                if (predicate(list[i]))
                    return i;
            }
            return -1;
        }

        public static bool IsSameForAll<T, TValue>(this ReadOnlySpan<T> list,
            Func<T, TValue> valueGetter, out TValue? value, IEqualityComparer<TValue>? comparer = null)
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

        public static int LinearSearch<T>(this ReadOnlySpan<T> span, T item, IComparer<T>? comparer)
            => LinearSearch<T, IComparer<T>>(span, item, comparer ?? Comparer<T>.Default);

        public static int LinearSearch<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
            => LinearSearch(span, new ComparerComparable<T, TComparer>(item, comparer));

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
           => LinearSearchFromEnd(span, new ComparerComparable<T, TComparer>(item, comparer));

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

        #endregion


        #region SpanLinq

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

        public readonly struct SortedModifier<T>
        {
            private readonly List<T> _list;
            private readonly IComparer<T> _comparer;

            internal SortedModifier(List<T> list, IComparer<T> comparer)
            {
                _list = list;
                _comparer = comparer;
            }

            public List<T> List => _list;

            public int Count => _list.Count;

            public T this[int index] => _list[index];

            public void Add(T item)
            {
                var index = _list.BinarySearch(item, _comparer);
                if (index >= 0) {
                    _list.Insert(index, item);
                }
                else {
                    _list.Insert(~index, item);
                }
            }

            /// <summary>
            /// Remove <paramref name="item"/> in collection if found, and returns the
            /// original index of the removed item in collection
            /// </summary>
            /// <returns>
            /// The original index of <paramref name="item"/> in collection if removed,
            /// else return bitwise complement of index of the next element that larger than <paramref name="item"/>
            /// </returns>
            public int Remove(T item)
            {
                var index = _list.BinarySearch(item, _comparer);
                if (index >= 0) {
                    _list.RemoveAt(index);
                }
                return index;
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

            public int CompareTo(T? other) => _comparer.Compare(_value, other);
        }
    }
}