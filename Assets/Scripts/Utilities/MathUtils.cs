#nullable enable

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Utilities
{
    public static class MathUtils
    {
        public static Vector2Int RoundToInt(this Vector2 vector) =>
            new(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));

        /// <summary>
        /// Abs x and y to positive
        /// </summary>
        public static Vector2 Abs(Vector2 vector) => new(Mathf.Abs(vector.x), Mathf.Abs(vector.y));

        public static float MapTo(float fromMin, float fromMax, float value, float toMin, float toMax)
        {
            var lerp = (value - fromMin) / (fromMax - fromMin);
            return (toMax - toMin) * lerp + toMin;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static float InverseLerpUnclamped(float min, float max, float value)
        {
            if (min == max) return 0;
            return (value - min) / (max - min);
        }

        public static bool IncAndTryWrap(this ref float value, float delta, float max)
        {
            value += delta;
            if (value <= max) return false;
            value -= max;
            return true;
        }
    }
}