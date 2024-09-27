using UnityEngine;

namespace Deenote.Project.Models
{
    public interface IStageNoteModel
    {
        public float Time { get; }
        public float Position { get; }
    }

    public sealed class NoteTailModel : IStageNoteModel
    {
        public NoteModel HeadModel { get; }

        public float Time
        {
            get {
                Debug.Assert(HeadModel.Data.IsHold);
                return HeadModel.Data.EndTime;
            }
        }

        public float Position => HeadModel.Data.Position;

        public NoteTailModel(NoteModel headModel) => HeadModel = headModel;
    }
}