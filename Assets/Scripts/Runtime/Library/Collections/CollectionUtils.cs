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
    }
}