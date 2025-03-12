#nullable enable

using System.Collections.Generic;
using System;

namespace Deenote.Library.Collections;
public static partial class SpanUtils
{
    public static int FindLowerBoundIndex<T, TComparer>(this Span<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => ((ReadOnlySpan<T>)span).FindLowerBoundIndex(new ComparisonUtils.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindLowerBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => span.FindLowerBoundIndex(new ComparisonUtils.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindLowerBoundIndex<T, TComparable>(this Span<T> span, TComparable key) where TComparable : IComparable<T>
        => ((ReadOnlySpan<T>)span).FindLowerBoundIndex(key);

    public static int FindLowerBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new ComparisonUtils.GreaterOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }

    public static int FindUpperBoundIndex<T, TComparer>(this Span<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => ((ReadOnlySpan<T>)span).FindUpperBoundIndex(new ComparisonUtils.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindUpperBoundIndex<T, TComparer>(this ReadOnlySpan<T> span, T key, TComparer comparer) where TComparer : IComparer<T>
        => span.FindUpperBoundIndex(new ComparisonUtils.ComparerComparable<T, TComparer>(key, comparer));

    public static int FindUpperBoundIndex<T, TComparable>(this Span<T> span, TComparable key) where TComparable : IComparable<T>
        => ((ReadOnlySpan<T>)span).FindUpperBoundIndex(key);

    public static int FindUpperBoundIndex<T, TComparable>(this ReadOnlySpan<T> span, TComparable key) where TComparable : IComparable<T>
    {
        var index = span.BinarySearch(new ComparisonUtils.LessOrNotComparable<T, TComparable>(key));
        return index < 0 ? ~index : index;
    }
}