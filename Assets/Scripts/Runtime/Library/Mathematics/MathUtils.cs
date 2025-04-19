#nullable enable

using UnityEngine;

namespace Deenote.Library.Mathematics
{
    public static class MathUtils
    {
        public static Vector2Int RoundToInt(Vector2 vector) =>
            new(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));

        /// <summary>
        /// Abs x and y to positive
        /// </summary>
        public static Vector2 Abs(Vector2 vector) => new(Mathf.Abs(vector.x), Mathf.Abs(vector.y));

        public static float MapTo(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var lerp = (value - fromMin) / (fromMax - fromMin);
            return (toMax - toMin) * lerp + toMin;
        }

        public static bool IncAndTryWrap(this ref float value, float delta, float max)
        {
            value += delta;
            if (value <= max) return false;
            value -= max;
            return true;
        }

        /// <summary>
        /// Fuck floating-point error
        /// </summary>
        public static int SafeFloorToInt(float value)
        {
            int result = Mathf.FloorToInt(value);
            if (Mathf.Approximately(result + 1, value))
                return result + 1;
            else
                return result;
        }

        /// <summary>
        /// Fuck floating-point error
        /// </summary>
        public static int SafeCeilToInt(float value)
        {
            int result = Mathf.CeilToInt(value);
            if (Mathf.Approximately(result - 1, value))
                return result - 1;
            else
                return result;
        }
    }
}