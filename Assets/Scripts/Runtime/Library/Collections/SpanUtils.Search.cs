#nullable enable

using System.Collections.Generic;
using System;

namespace Deenote.Library.Collections;
public static partial class SpanUtils
{
    public static int LinearSearch<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => ((ReadOnlySpan<T>)span).LinearSearch(new ComparisonUtils.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearch<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => span.LinearSearch(new ComparisonUtils.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearch<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
        => AlgorithmUtils.LinearSearch<T, TComparable>(span, item);

    public static int LinearSearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        => AlgorithmUtils.LinearSearch(span, item);

    public static int LinearSearchFromEnd<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => ((ReadOnlySpan<T>)span).LinearSearchFromEnd(new ComparisonUtils.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearchFromEnd<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => span.LinearSearchFromEnd(new ComparisonUtils.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearchFromEnd<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
        => AlgorithmUtils.LinearSearchFromEnd<T, TComparable>(span, item);

    public static int LinearSearchFromEnd<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        => AlgorithmUtils.LinearSearchFromEnd(span, item);
}