#nullable enable

using System;
using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class SoundNoteModel : IStageTimeNode
    {
        internal readonly NoteModel _noteModel;

        public float Time
        {
            get => _noteModel._time;
            set => _noteModel._time = value;
        }

        public ReadOnlySpan<PianoSoundValueModel> Sounds => _noteModel.Sounds;

        // In some charts there're no-sound notes at background, which has no effect
        // other than increase json size.
        // Maybe related to official chart maker, we just remain it here
        //private List<PianoSoundValueModel> _sounds;

        internal SoundNoteModel(NoteModel note)
        {
            Debug.Assert(!note.IsVisibleOnStage());
            _noteModel = note;
        }

        public SoundNoteModel Clone()
            => new(_noteModel.Clone(cloneSounds: true));
    }
}
