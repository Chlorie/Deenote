using Deenote.Project.Models.Datas;
using System;

namespace Deenote.Utilities
{
    public static class ProjectUtils
    {
        private static readonly string[] _pianoNoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public static string ToPitchDisplayString(int pitch)
        {
            int octave = Math.DivRem(pitch, 12, out var rem) - 2;
            return $"{_pianoNoteNames[rem]}{octave}";
        }

        public static string ToPitchDisplayString(this PianoSoundData sound)
            => ToPitchDisplayString(sound.Pitch);

        public static void UnlinkWithoutCutLinkChain(this NoteData note)
        {
            var prevLink = note.PrevLink;
            var nextLink = note.NextLink;

            note.IsSlide = false;
            if (prevLink != null)
                prevLink.NextLink = nextLink;
            if (nextLink != null)
                nextLink.PrevLink = prevLink;
        }

        public static void InsertAsLinkBefore(this NoteData note, NoteData nextLink)
        {
            note.UnlinkWithoutCutLinkChain();

            note.NextLink = nextLink;
            note.PrevLink = nextLink.PrevLink;
            if (note.PrevLink != null)
                note.PrevLink.NextLink = note;
            nextLink.PrevLink = note;
        }

        public static void InsertAsLinkAfter(this NoteData note, NoteData prevLink)
        {
            note.UnlinkWithoutCutLinkChain();

            note.PrevLink = prevLink;
            note.NextLink = prevLink.NextLink;
            if (note.NextLink != null)
                note.NextLink.PrevLink = note;
            prevLink.NextLink = note;
        }
    }
}