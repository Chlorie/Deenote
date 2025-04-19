#nullable enable

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.Library
{
    internal static class ThrowUtils
    {
        [DoesNotReturn]
        internal static void KeyNotFound<T>(T key)
           => throw new KeyNotFoundException($"Cannot find key '{key}' in collection.");

        [DoesNotReturn]
        internal static void CollectionModified()
           => throw new InvalidOperationException("Collection has been modified.");

        [DoesNotReturn]
        internal static void NoElement()
            => throw new InvalidOperationException("Collection has no element.");
     
        public static void ValidateSliceArgs(int start, int sliceLength, int count)
        {
            Guard.IsGreaterThanOrEqualTo(start, 0);
            Guard.IsGreaterThanOrEqualTo(sliceLength, 0);
            Guard.IsLessThanOrEqualTo(start + sliceLength, count);
        }

    }
}