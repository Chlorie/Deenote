using UnityEngine;

namespace Deenote
{
    public struct NoteCoord
    {
        public float Position;
        public float Time;

        public NoteCoord(float position, float time)
        {
            Position = position;
            Time = time;
        }

        public static NoteCoord ClampPosition(NoteCoord coord) => ClampPosition(coord.Position, coord.Time);

        public static NoteCoord ClampPosition(float position, float time) => new(MainSystem.Args.ClampNotePosition(position), time);
        public static NoteCoord Clamp(NoteCoord coord, float maxTime) => new(MainSystem.Args.ClampNotePosition(coord.Position), Mathf.Clamp(coord.Time, 0f, maxTime));

        public static NoteCoord operator -(NoteCoord left, NoteCoord right) => new(left.Position - right.Position, left.Time - right.Time);

        public static NoteCoord operator +(NoteCoord left, NoteCoord right) => new(left.Position + right.Position, left.Time + right.Time);
    }
}