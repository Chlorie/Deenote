#nullable enable

using System.Diagnostics;
using UnityEngine;

namespace Deenote.Entities
{
    [DebuggerDisplay("Pos:{Position}, Time:{Time}")]
    public struct NoteCoord
    {
        public float Position;
        public float Time;

        public NoteCoord(float position, float time)
        {
            Position = position;
            Time = time;
        }

        #region Clamp

        public const float StageMaxPosition = 2f;
        public const float StageMaxPositionWidth = 2 * StageMaxPosition;

        public static NoteCoord ClampPosition(NoteCoord coord) => ClampPosition(coord.Position, coord.Time);

        public static NoteCoord ClampTime(NoteCoord coord, float maxTime)
            => coord with { Time = EntityArgs.ClampTime(coord.Time, maxTime) };

        public static NoteCoord ClampPosition(float position, float time)
            => new(EntityArgs.ClampPosition(position), time);

        public static NoteCoord Clamp(NoteCoord coord, float maxTime)
            => new(EntityArgs.ClampPosition(coord.Position), EntityArgs.ClampTime(coord.Time, maxTime));

        #endregion

        public static NoteCoord operator -(NoteCoord left, NoteCoord right) =>
            new(left.Position - right.Position, left.Time - right.Time);

        public static NoteCoord operator +(NoteCoord left, NoteCoord right) =>
            new(left.Position + right.Position, left.Time + right.Time);

        public override readonly string ToString() => $"{{ Pos:{Position}, Time:{Time} }}";
    }
}