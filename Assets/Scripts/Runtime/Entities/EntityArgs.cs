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
    }
}