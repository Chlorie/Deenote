#nullable enable

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        public const int MinNoteSpeed = 1;
        public const int MaxNoteSpeed = 99;

        public const int MinMusicSpeed = 1;
        public const int MaxMusicSpeed = 30;

        public static float ConvertToActualNoteSpeed(int noteSpeed) => noteSpeed / 10f;
        public static float ConvertToActualMusicSpeed(int musicSpeed) => musicSpeed / 10f;
    }
}