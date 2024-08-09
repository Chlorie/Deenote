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
            foreach (var end in ends)
            {
                if (str.EndsWith(end))
                    return true;
            }
            return false;
        }

        public static void RemoveRange<T>(this List<T> list, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(list.Count);
            list.RemoveRange(offset, length);
        }

        public static void RemoveAt<T>(this List<T> list, Index index)
        {
            list.RemoveAt(index.GetOffset(list.Count));
        }

        public static T[] Array<T>(int length) => length == 0 ? System.Array.Empty<T>() : new T[length];

        public static bool IsSameForAll<T, TValue>(this ListReadOnlyView<T> list,
            Func<T, TValue> valueGetter, out TValue? value, IEqualityComparer<TValue>? comparer = null)
        {
            switch (list.Count)
            {
                case 0:
                    value = default;
                    return true;
                case 1:
                    value = valueGetter(list[0]);
                    return true;
            }

            value = valueGetter(list[0]);
            comparer ??= EqualityComparer<TValue>.Default;

            for (int i = 0; i < list.Count; i++)
            {
                if (!comparer.Equals(value, valueGetter(list[i])))
                {
                    value = default;
                    return false;
                }
            }

            return true;
        }
    }
}