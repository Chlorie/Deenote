#nullable enable

using System.Collections.Generic;
using System;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int FindLowerBoundIndex<T, TComparer>(this Span<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindLowerBoundIndex((ReadOnlySpan<T>)span, new TraComparison.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindLowerBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindLowerBoundIndex(span, new TraComparison.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindLowerBoundIndex<T, TComparable>(this Span<T> span, TComparable key) where TComparable : IComparable<T>
        => FindLowerBoundIndex((ReadOnlySpan<T>)span, key);

    public static int FindLowerBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new TraComparison.GreaterOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }

    public static int FindUpperBoundIndex<T, TComparer>(this Span<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindUpperBoundIndex((ReadOnlySpan<T>)span, new TraComparison.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindUpperBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => FindUpperBoundIndex(span, new TraComparison.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindUpperBoundIndex<T, TComparable>(this Span<T> span, TComparable key) where TComparable : IComparable<T>
        => FindUpperBoundIndex((ReadOnlySpan<T>)span, key);

    public static int FindUpperBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new TraComparison.LessOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }
}