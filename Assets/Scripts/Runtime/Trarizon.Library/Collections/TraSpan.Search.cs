#nullable enable

using System.Collections.Generic;
using System;

namespace Trarizon.Library.Collections;
public static partial class TraSpan
{
    public static int LinearSearch<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearch((ReadOnlySpan<T>)span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearch<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearch(span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearch<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearch<T, TComparable>(span, item);

    public static int LinearSearch<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearch(span, item);

    public static int LinearSearchFromEnd<T, TComparer>(this Span<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearchFromEnd((ReadOnlySpan<T>)span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearchFromEnd<T, TComparer>(this ReadOnlySpan<T> span, T item, TComparer comparer) where TComparer : IComparer<T>
        => LinearSearchFromEnd(span, new TraComparison.ComparerComparable<T, TComparer>(item, comparer));

    public static int LinearSearchFromEnd<T, TComparable>(this Span<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearchFromEnd<T, TComparable>(span, item);

    public static int LinearSearchFromEnd<T, TComparable>(this ReadOnlySpan<T> span, TComparable item) where TComparable : IComparable<T>
        => TraAlgorithm.LinearSearchFromEnd(span, item);
}