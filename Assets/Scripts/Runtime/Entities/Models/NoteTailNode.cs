#nullable enable

using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class NoteTailNode : IStageNoteNode
    {
        public NoteModel HeadModel { get; }

        public float Time
        {
            get {
                Debug.Assert(HeadModel.IsHold);
                return HeadModel.EndTime;
            }
        }

        public float Position => HeadModel.Position;

        public float Speed => HeadModel.Speed;

        bool IStageNoteNode.IsComboNode => true;

        public NoteTailNode(NoteModel headModel) => HeadModel = headModel;
    }

}