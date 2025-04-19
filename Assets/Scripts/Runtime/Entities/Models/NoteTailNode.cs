#nullable enable

using Deenote.Entities.Models;
using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class NoteTailNode : IStageNoteNode
    {
        private readonly uint _uid;

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

        uint IStageNoteNode.Uid => _uid;

        public NoteTailNode(NoteModel headModel)
        {
            HeadModel = headModel;
            IStageNoteNode.InitUid(ref _uid);
        }
    }
}
