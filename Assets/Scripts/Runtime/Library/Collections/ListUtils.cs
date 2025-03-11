#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Deenote.Library.Collections
{
    public static partial class ListUtils
    {
        public static Span<T> AsSpan<T>(this List<T> list) 
            => Utils<T>.GetUnderlyingArray(list).AsSpan(..list.Count);

        public static Span<T> AsSpanOrEmpty<T>(this List<T>? list)
            => list is null ? Span<T>.Empty : list.AsSpan();

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

        private static class Utils<T>
        {
            public static ref T[] GetUnderlyingArray(List<T> list)
            {
                var arr = Unsafe.As<List<T>, StrongBox<T[]>>(ref list);
                Debug.Assert(arr.Value is T[]);
                return ref arr.Value;
            }
        }
    }
}