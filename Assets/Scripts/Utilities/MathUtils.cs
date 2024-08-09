namespace Deenote.Utilities
{
    public static class MathUtils
    {
        public static float InverseLerpUnclamped(float min, float max, float value)
        {
            if (min == max)
                return 0;

            return (value - min) / (max - min);
        }
    }
}