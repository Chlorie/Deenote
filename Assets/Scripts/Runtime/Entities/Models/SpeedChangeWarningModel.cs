#nullable enable

using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class SpeedChangeWarningModel : IStageTimeNode
    {
        internal readonly NoteModel _noteModel;

        public float Time => _noteModel.Time;

        internal SpeedChangeWarningModel(NoteModel note)
        {
            Debug.Assert(!note.IsVisibleOnStage());
            Debug.Assert(!note.HasSounds);
            Debug.Assert(note.WarningType is WarningType.SpeedChange);
            _noteModel = note;
        }

        public SpeedChangeWarningModel(float time) :
            this(new NoteModel() { Time = time, Position = 4f, WarningType = WarningType.SpeedChange })
        { }

        public SpeedChangeWarningModel Clone()
            => new(_noteModel.Clone(cloneSounds: false));
    }
}