#nullable enable

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
        public static readonly string[] DropdownOptions = new[] {
            Difficulty.Easy.ToDisplayString(), Difficulty.Normal.ToDisplayString(), Difficulty.Hard.ToDisplayString(),
            Difficulty.Extra.ToDisplayString(),
        };

        public static Difficulty FromInt32(int value) => (Difficulty)value;

        public static int ToInt32(this Difficulty difficulty) => (int)difficulty;

        public static string ToDisplayString(this Difficulty difficulty) => difficulty.ToString();

        public static string ToLowerCaseString(this Difficulty difficulty)
        {
            return difficulty switch {
                Difficulty.Easy => "easy",
                Difficulty.Normal => "normal",
                Difficulty.Hard => "hard",
                Difficulty.Extra => "expert",
                _ => "unknown",
            };
        }
    }
}