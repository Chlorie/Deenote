#nullable enable

using CommunityToolkit.Diagnostics;

namespace Trarizon.Library.Numerics;
public static partial class TraNumber
{
    /// <summary>
    /// if (value < 0) value = ~value, this method is useful with return value of <c>Search</c>s
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public static void FlipNegative(ref int value)
    {
        if (value < 0)
            value = ~value;
    }

    public static void ValidateSliceArgs(int start, int sliceLength, int count)
    {
        Guard.IsGreaterThanOrEqualTo(start, 0);
        Guard.IsGreaterThanOrEqualTo(sliceLength, 0);
        Guard.IsLessThanOrEqualTo(start + sliceLength, count);
    }

}