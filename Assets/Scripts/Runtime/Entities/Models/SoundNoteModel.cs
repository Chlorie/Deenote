#nullable enable

using Deenote.Entities.Models;
using Deenote.Library.Collections;
using System;
using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed class SoundNoteModel : IStageTimeNode
    {
        internal readonly NoteModel _noteModel;

        public float Time
        {
            get => _noteModel.Time;
            set => _noteModel.Time = value;
        }

        public ReadOnlySpan<PianoSoundValueModel> Sounds => _noteModel.Sounds.AsSpan();

        // In some charts there're no-sound notes at background, which has no effect
        // other than increase json size.
        // Maybe related to official chart maker, we just remain it here
        //private List<PianoSoundValueModel> _sounds;

        internal SoundNoteModel(NoteModel note)
        {
            Debug.Assert(!note.IsVisibleOnStage());
            _noteModel = note;
        }

        public SoundNoteModel(float time, ReadOnlySpan<PianoSoundValueModel> sounds)
        {
            _noteModel = new NoteModel();
            _noteModel.Time = time;
            _noteModel.Position = 12f;
            _noteModel.Sounds.AddRange(sounds);
        }

        public SoundNoteModel Clone()
            => new(_noteModel.Clone(cloneSounds: true));
    }
}