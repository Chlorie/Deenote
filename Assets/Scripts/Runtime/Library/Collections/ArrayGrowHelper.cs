#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Deenote.Library.Collections;
internal static class ArrayGrowHelper
{
    public static void FreeManaged<T>(T[] array)
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Array.Clear(array, 0, array.Length);
    }

    public static void FreeManaged<T>(T[] array, Range range)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;

        var (off, len) = range.GetOffsetAndLength(array.Length);
        Array.Clear(array, off, len);
    }

    public static void FreeManaged<T>(T[] array, int index, int length)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            return;

        Array.Clear(array, index, length);
    }

    public static void Grow<T>(ref T[] array, int expectedLength, int copyLength)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(copyLength <= array.Length);

        var originalArray = array;
        array = new T[GetNewLength(array.Length, expectedLength, 2146435071)];
        Array.Copy(originalArray, array, copyLength);
    }

    public static void GrowNonMove<T>(ref T[] array, int expectedLength)
        => GrowNonMove(ref array, expectedLength, 2146435071);
    public static void GrowNonMove<T>(ref T[] array, int expectedLength, int maxLength)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(expectedLength <= maxLength);
        array = new T[GetNewLength(array.Length, expectedLength, maxLength)];
    }

    public static void GrowForInsertion<T>(ref T[] array, int expectedLength, int copyCount, int insertIndex, int insertCount)
    {
        Debug.Assert(expectedLength > array.Length);
        Debug.Assert(insertIndex <= array.Length);
        Debug.Assert(insertIndex >= 0);
        Debug.Assert(insertCount >= 0);

        var originalArray = array;
        GrowNonMove(ref array, expectedLength);
        if (insertIndex > 0) {
            Array.Copy(originalArray, array, insertIndex);
        }
        if (insertIndex < copyCount) {
            Array.Copy(originalArray, insertIndex, array, insertIndex + insertCount, copyCount - insertIndex);
        }
    }

    private static int GetNewLength(int length, int expectedLength, int maxLength)
    {
        Debug.Assert(length < expectedLength);

        int newLen;
        if (length == 0)
            newLen = Math.Min(4, maxLength);
        else
            newLen = Math.Min(length * 2, maxLength);

        if (newLen < expectedLength)
            newLen = expectedLength;
        return newLen;
    }
}
