#nullable enable

using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class SpeedChangeWarningModel : IStageTimeNode
    {
        private readonly NoteModel _noteModel;

        public float Time => _noteModel.Time;

        internal SpeedChangeWarningModel(NoteModel note)
        {
            Debug.Assert(!note.IsVisibleOnStage());
            Debug.Assert(!note.HasSounds);
            Debug.Assert(note.WarningType is WarningType.SpeedChange);
            _noteModel = note;
        }

        public SpeedChangeWarningModel Clone()
            => new(_noteModel.Clone(cloneSounds: false));
    }
}