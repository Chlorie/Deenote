#nullable enable

using System.Diagnostics;
using UnityEngine;

namespace Deenote
{
    [DebuggerDisplay("Pos:{Position}, Time:{Time}")]
    public struct NoteCoord
    {
        public float Position;
        public float Time;

        public NoteCoord(float time, float position)
        {
            Position = position;
            Time = time;
        }

        public static NoteCoord ClampPosition(NoteCoord coord) => ClampPosition(coord.Time, coord.Position);

        public static NoteCoord ClampPosition(float time, float position) =>
            new(time, MainSystem.Args.ClampNotePosition(position));

        public static NoteCoord Clamp(NoteCoord coord, float maxTime) => new(Mathf.Clamp(coord.Time, 0f, maxTime),
            MainSystem.Args.ClampNotePosition(coord.Position));

        public static NoteCoord operator -(NoteCoord left, NoteCoord right) =>
            new(left.Time - right.Time, left.Position - right.Position);

        public static NoteCoord operator +(NoteCoord left, NoteCoord right) =>
            new(left.Time + right.Time, left.Position + right.Position);

        public override readonly string ToString() => $"{{ Time: {Time}, Pos: {Position} }}";
    }
}