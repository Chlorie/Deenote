using Deenote.Project.Models.Datas;
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

        public static NoteCoord ClampPosition(NoteCoord coord) => new(Mathf.Clamp(coord.Position, -MainSystem.Args.StageMaxPosition, MainSystem.Args.StageMaxPosition), coord.Time);
    }
}