#nullable enable

using System;

namespace Deenote.Library.Numerics
{
    public static class NumberUtils
    {
        /// <summary>
        /// If value is negative, do bitwise-not on it
        /// </summary>
        /// <param name="value"></param>
        public static void FlipNegative(ref int value)
        {
            if (value < 0)
                value = ~value;
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

        public static void SortAsc(ref float left, ref float right)
        {
            if (left > right)
                (left, right) = (right, left);
        }
    }
}