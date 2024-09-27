using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;

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

        public static ReadOnlySpan<T> AsReadOnlySpan<T>(this List<T> list)
            => list.AsSpan();

        public static Memory<T> AsMemory<T>(this List<T> list)
        {
            var provider = Unsafe.As<StrongBox<T[]>>(list);
            var array = provider.Value;
            return array.AsMemory(0, list.Count);
        }

        public static void InsertionSortAsc<T>(Span<T> values, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            for (int i = 1; i < values.Length; i++) {
                int j = i - 1;
                for (; j >= 0; j--) {
                    var left = values[j];
                    var right = values[i];
                    if (comparer.Compare(left, right) <= 0) {
                        break;
                    }
                }
                values.MoveTo(i, j + 1);
            }
        }

        public static int FindLowerBoundIndex<T>(this ReadOnlySpan<T> values, T key, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            for (int i = 0; i < values.Length; i++) {
                if (comparer.Compare(key, values[i]) <= 0) {
                    return i;
                }
            }
            return values.Length;
        }

        public static int FindUpperBoundIndex<T>(this ReadOnlySpan<T> values,T key,IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            for(int i=0; i < values.Length; i++) {
                if (comparer.Compare(key, values[i]) < 0) {
                    return i;
                }
            }
            return values.Length;
        }
    }
}