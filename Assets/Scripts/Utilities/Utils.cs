using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using System.IO;

namespace Deenote.Utilities
{
    public static class Utils
    {
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

        public static bool IsValidFileName(string fileName)
            => fileName.IndexOfAny(_invalidFileNameChars) < 0;

        public static bool IsValidPath(string path)
            => path.IndexOfAny(_invalidPathChars) < 0;

        public static bool EndsWithOneOf(this string str, ReadOnlySpan<string> ends)
        {
            foreach (var end in ends) {
                if (str.EndsWith(end))
                    return true;
            }
            return false;
        }

        public static bool IncAndTryWrap(this ref float value, float delta, float max)
        {
            value += delta;
            if (value <= max) return false;
            value -= max;
            return true;
        }

        public static bool SequenceEqual<T>(this ReadOnlySpan<T> span, List<T> list)
        {
            if (span.Length != list.Count)
                return false;

            for (int i = 0; i < span.Length; i++) {
                if (!EqualityComparer<T>.Default.Equals(span[i], list[i]))
                    return false;
            }
            return true;
        }

        public static void RemoveRange<T>(this List<T> list, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            list.RemoveRange(offset, length);
        }

        public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return;

            var val = list[fromIndex];
            list.RemoveAt(fromIndex);
            list.Insert(toIndex, val);
        }

        public static T[] Array<T>(int length) => length == 0 ? System.Array.Empty<T>() : new T[length];

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

        public static bool IsSameForAll<T, TValue>(this ListReadOnlyView<T> list,
            Func<T, TValue> valueGetter, out TValue? value, IEqualityComparer<TValue>? comparer = null)
        {
            switch (list) {
                case { Count: 0 } or { IsNull: true }:
                    value = default;
                    return true;
                case { Count: 1 }:
                    value = valueGetter(list[0]);
                    return true;
            }

            value = valueGetter(list[0]);
            comparer ??= EqualityComparer<TValue>.Default;

            for (int i = 0; i < list.Count; i++) {
                if (comparer.Equals(value, valueGetter(list[i]))) continue;
                value = default;
                return false;
            }

            return true;
        }
    }
}