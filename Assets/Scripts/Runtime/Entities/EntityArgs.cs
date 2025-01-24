#nullable enable

using Deenote.Entities.Models;
using UnityEngine;

namespace Deenote.Entities
{
    public static class EntityArgs
    {
        public const float StageMaxPosition = 2;
        public const float StageMaxPositionWidth = 2 * StageMaxPosition;

        private const float NoteTimeCollisionThreshold = 0.001f;
        private const float NotePositionCollisionThreshold = 0.01f;

        private const float MinNoteSize = 0.1f;
        private const float MaxNoteSize = 5f;
        private const float MinNoteSpeed = 0.1f;
        private const float MaxNoteSpeed = 20f;

        public static float ClampTime(float time, float maxTime) => Mathf.Clamp(time, 0f, maxTime);

        public static float ClampPosition(float position) => Mathf.Clamp(position, -StageMaxPosition, StageMaxPosition);

        public static float ClampSize(float size) => Mathf.Clamp(size, MinNoteSize, MaxNoteSize);

        public static float ClampNoteSpeed(float speed) => Mathf.Clamp(speed, MinNoteSpeed, MaxNoteSpeed);

        private static bool IsTimeCollided(float left, float right)
            => Mathf.Abs(right - left) <= NoteTimeCollisionThreshold;

        public static bool IsTimeCollided(NoteModel left, NoteModel right)
            => IsTimeCollided(left.Time, right.Time);

        private static bool IsPositionCollided(float left, float right)
            => Mathf.Abs(right - left) <= NotePositionCollisionThreshold;

        public static bool IsPositionCollided(NoteModel left, NoteModel right)
            => IsPositionCollided(left.Position, right.Position);

        public static bool IsCollided(NoteCoord left, NoteCoord right)
            => IsTimeCollided(left.Time, right.Time) && IsPositionCollided(left.Position, right.Position);

        public static bool IsVisibleOnStage(this NoteModel note)
            => note.Position is >= -StageMaxPosition and <= StageMaxPosition;

        #region Speed属性的一些备用工具

        // NOTE: This change is for speed property of note, currently has no use

        const float CurrentMusicTime = 0f, StageNoteAheadTime = 0f;

        // TODO：NoteModel基于这个值排序，
        public static float NoteActualAppearTime(float time, float noteSpeed) => time - StageNoteAheadTime / noteSpeed;

        // 这个时间主要用于显示
        // 形象地说，Note在出现之前以1速下落，出现之后以NoteSpeed速度下落，在判定线以下以1速下落
        // 基于这个PseudoTime排列，Note能够以其在界面上的位置排序
        public static float NoteTimeToPseudoTime(float time, float noteSpeed)
        {
            var currentTime = CurrentMusicTime;
            if (time <= currentTime)
                return time;
            else if (time < currentTime + StageNoteAheadTime)
                return (time - currentTime) * noteSpeed;
            else
                return (time - currentTime) - StageNoteAheadTime / noteSpeed;
        }

        #endregion
    }
}