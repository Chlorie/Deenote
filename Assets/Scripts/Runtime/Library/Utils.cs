#nullable enable

using System;
using System.Collections.Generic;

namespace Deenote.Library
{
    public static class Utils
    {
        public static bool EndsWithOneOf(this string str, ReadOnlySpan<string> ends)
        {
            foreach (var end in ends) {
                if (str.EndsWith(end))
                    return true;
            }
            return false;
        }

        public static bool SetField<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            return true;
        }

        public static bool SetField<T>(ref T field, T value, out T originalValue)
        {
            originalValue = field;
            return SetField(ref field, value);
        }
    }
}