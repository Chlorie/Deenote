#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Deenote.Library
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

        public static bool SetField<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            return true;
        }

        public static bool SetField<T>(ref T field, T value, out T originalValue)
        {
            originalValue = field;
            return SetField(ref field, value);
        }

        public static void SortAsc(ref float left, ref float right)
        {
            if (left > right)
                (left, right) = (right, left);
        }

        /// <summary>
        /// <see cref="Index.GetOffset(int)"/>, and check if the offset is in [0, <paramref name="length"/>),
        /// throw if out of range
        /// </summary>
        public static int GetCheckedOffset(this Index index, int length)
        {
            var offset = index.GetOffset(length);
            if ((uint)offset >= (uint)length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return offset;
        }
    }
}