#nullable enable

using CommunityToolkit.HighPerformance;
using System;
using System.Collections.Generic;

namespace Deenote.Library.Collections;
public static partial class AlgorithmUtils
{
    public static void BubbleSort<T>(Span<T> values, IComparer<T>? comparer)
        => BubbleSort<T, IComparer<T>>(values, comparer ?? Comparer<T>.Default);

    public static void BubbleSort<T, TComparer>(Span<T> values, TComparer comparer) where TComparer : IComparer<T>
    {
        if (values.Length <= 1)
            return;

        bool inOrder;

        for (int i = 0; i < values.Length; i++) {
            inOrder = true;
            for (int j = values.Length - 1; j > i; j--) {
                ref var left = ref values.DangerousGetReferenceAt(j - 1);
                ref var right = ref values.DangerousGetReferenceAt(j);
                if (comparer.Compare(left, right) > 0) {
                    (left, right) = (right, left);
                    inOrder = false;
                }
            }
            if (inOrder)
                return;
        }
    }

    public static int LinearSearch<T, TComparable>(ReadOnlySpan<T> items, TComparable item) where TComparable : IComparable<T>
    {
        for (int i = 0; i < items.Length; i++) {
            var res = item.CompareTo(items[i]);
            if (res < 0)
                return ~i;
            if (res == 0)
                return i;
        }
        return ~items.Length;
    }

    public static int LinearSearchFromEnd<T, TComparable>(ReadOnlySpan<T> items, TComparable item) where TComparable : IComparable<T>
    {
        for (int i = items.Length - 1; i >= 0; i--) {
            var res = item.CompareTo(items[i]);
            if (res > 0)
                return ~(i + 1);
            if (res == 0)
                return i;
        }
        return ~0;
    }

#if false
    // I copied this from .NET source code
    // BCL has provided a lot of BinarySearches, so this is just for maybe-upcoming future
    public static int BinarySearch<T>(ReadOnlySpan<T> span, T value, IComparer<T>? comparer)
    {
        int lo = 0;
        int hi = span.Length - 1;
        comparer ??= Comparer<T>.Default;

        while (lo <= hi) {
            int i = lo + ((hi - lo) >> 1);

            int c = comparer.Compare(span[i], value);
            if (c == 0) return i;
            if (c < 0) {
                lo = i + 1;
            }
            else {
                hi = i - 1;
            }
        }
        return ~lo;
    }
#endif
}