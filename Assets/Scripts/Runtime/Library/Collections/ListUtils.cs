#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Trarizon.Library.Collections;

namespace Deenote.Library.Collections
{
    public static class ListUtils
    {
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

        public static void MoveTo<T>(this List<T> list, int fromIndex, int toIndex)
            => list.AsSpan().MoveTo(fromIndex, toIndex);

        public static bool TryAt<T>(this List<T> list, int index, [MaybeNullWhen(false)] out T result)
        {
            if ((uint)index < (uint)list.Count) {
                result = list[index];
                return true;
            }
            result = default;
            return false;
        }

        public static void EnsureCapacity<T>(this List<T> list, int minCapacity)
        {
            if (minCapacity > list.Capacity)
                list.Capacity = minCapacity;
        }

        public static Span<T> AsSpanOrEmpty<T>(this List<T>? list)
            => list is null ? Span<T>.Empty : list.AsSpan();
    }
}