#nullable enable

#pragma warning disable CS8604 // 引用类型参数可能为 null。

using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class ComparisonUtils
{
    internal readonly struct ComparerComparable<T, TComparer> : IComparable<T> where TComparer : IComparer<T>
    {
        private readonly T _value;
        private readonly TComparer _comparer;

        public ComparerComparable(T value, TComparer comparer)
        {
            _value = value;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other) => _comparer.Compare(_value, other);
    }

    /// <summary>
    /// A wrapper, if value == other, returns as value &lt; other, never return 0
    /// </summary>
    internal readonly struct GreaterOrNotComparable<T, TComparable> : IComparable<T> where TComparable : IComparable<T>
    {
        private readonly TComparable _value;

        public GreaterOrNotComparable(TComparable value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other)
        {
            var res = _value.CompareTo(other);
            if (res == 0)
                return -1;
            return res;
        }
    }

    /// <summary>
    /// A wrapper, if value == other, returns as value > other, never return 0
    /// </summary>
    internal readonly struct LessOrNotComparable<T, TComparable> : IComparable<T> where TComparable : IComparable<T>
    {
        private readonly TComparable _value;

        public LessOrNotComparable(TComparable value)
        {
            _value = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(T? other)
        {
            var res = _value.CompareTo(other);
            if (res == 0)
                return 1;
            return res;
        }
    }
}