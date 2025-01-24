#nullable enable

using System;
using System.Collections.Immutable;

namespace Deenote.Entities
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
        public static readonly ImmutableArray<string> DropdownOptions = ImmutableArray.Create(
            Difficulty.Easy.ToDisplayString(),
            Difficulty.Normal.ToDisplayString(),
            Difficulty.Hard.ToDisplayString(),
            Difficulty.Extra.ToDisplayString());

        public static Difficulty FromInt32Index(int value) => (Difficulty)value;

        public static int ToInt32Index(this Difficulty difficulty) => (int)difficulty;

        public static int ToDropdownIndex(this Difficulty difficulty) => (int)difficulty;

        public static Difficulty FromDropdownIndex(int index) => (Difficulty)index;

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