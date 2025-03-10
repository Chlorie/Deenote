#nullable enable
#define NETSTANDARD
#define NETSTANDARD2_1

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Trarizon.Library.Collections;
public static partial class TraList
{
#if NETSTANDARD
    /// <remarks>
    /// As CollectionsMarshal doesnt exists on .NET Standard 2.0, this use a very tricky way
    /// to get the underlying array. Actually I'm not sure if this works correctly in all runtime...
    /// (at leat it works on Unity
    /// </remarks>
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        return Utils<T>.GetUnderlyingArray(list).AsSpan(..list.Count);
    }
#endif

    private static class Utils<T>
    {
#if NET9_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_version")]
        public static extern ref int GetVersion(List<T> list);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
        public static extern ref T[] GetUnderlyingArray(List<T> list);
#else
        public static ref T[] GetUnderlyingArray(List<T> list)
        {
            var arr = Unsafe.As<List<T>, StrongBox<T[]>>(ref list);
            Debug.Assert(arr.Value is T[]);
            return ref arr.Value;
        }
#endif
    }
}