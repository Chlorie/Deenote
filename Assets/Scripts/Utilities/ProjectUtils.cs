using Deenote.Project.Models.Datas;
using System;

namespace Deenote.Utilities
{
    public static class ProjectUtils
    {
        private static readonly string[] _pianoNoteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string ToPitchDisplayString(int pitch)
        {
            int octave = Math.DivRem(pitch, 12, out var rem) - 2;
            return $"{_pianoNoteNames[rem]}{octave}";
        }

        public static string ToPitchDisplayString(this PianoSoundData sound) 
            =>ToPitchDisplayString(sound.Pitch);
    }
}