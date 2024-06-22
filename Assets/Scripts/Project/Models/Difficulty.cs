namespace Deenote.Project.Models
{
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Extra = 3,
    }

    public static class DifficultyExt
    {
        public static Difficulty FromInt32(int value) => (Difficulty)value;

        public static int ToInt32(this Difficulty difficulty) => (int)difficulty;

        public static string ToDisplayString(this Difficulty difficulty) => difficulty.ToString();
    }
}