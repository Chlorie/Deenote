#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list)
            => GetUnderlyingArray(list).AsSpan(0, list.Count);

        public static Span<T> AsSpanOrEmpty<T>(this List<T>? list)
            => list is null ? Span<T>.Empty : AsSpan(list);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> AsMemory<T>(this List<T> list)
            => GetUnderlyingArray(list).AsMemory(0, list.Count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] GetUnderlyingArray<T>(List<T> list)
        {
            // The underlying array is the first field of List<T>, so if we
            // force cast it into another reference type whose first field
            // is T[], we can get the underlying array.

            // I'm not sure if the layout of List<T> is the same for all runtimes,
            // so please do test after changing the runtime
            var provider = Unsafe.As<StrongBox<T[]>>(list);
            var array = provider.Value;
            return array;
        }

        /// <summary>
        /// Returns a view through which modifying the list will keep elements in order.
        /// <br/>
        /// Make sure your list is already in order
        /// </summary>
        public static SortedModifier<T> GetSortedModifier<T>(this List<T> list, IComparer<T>? comparer = null)
            => new(list, comparer ?? Comparer<T>.Default);

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
            /// Search for add index from the end of collection, and insert <paramref name="item"/>
            /// </summary>
            public void AddFromEnd(T item)
            {
                var index = ((ReadOnlySpan<T>)_list.AsSpan()).LinearSearchFromEnd(item, _comparer);
                if (index < 0)
                    index = ~index;
                _list.Insert(index, item);
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

    }
}