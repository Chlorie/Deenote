using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System;

namespace Deenote.Utilities
{
    public static class ProjectUtils
    {
        private static readonly string[] _pianoNoteNames = {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };

        public static string ToPitchDisplayString(int pitch)
        {
            int octave = Math.DivRem(pitch, 12, out var rem) - 2;
            return $"{_pianoNoteNames[rem]}{octave}";
        }

        public static string ToPitchDisplayString(this PianoSoundData sound)
            => ToPitchDisplayString(sound.Pitch);

        public static string ToPitchDisplayString(this in PianoSoundValueData sound)
            => ToPitchDisplayString(sound.Pitch);

        /// <summary>
        /// Set <paramref name="note"/> to non-slide, and 
        /// link its original prev note to original next note
        /// </summary>
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

        /// <summary>
        /// Insert <paramref name="note"/> into another link before <paramref name="nextLink"/>,
        /// this method auto calls <see cref="UnlinkWithoutCutLinkChain(NoteData)"/> first
        /// </summary>
        public static void InsertAsLinkBefore(this NoteData note, NoteData nextLink)
        {
            if (note.IsSlide)
                note.UnlinkWithoutCutLinkChain();

            note.NextLink = nextLink;
            note.PrevLink = nextLink.PrevLink;
            if (note.PrevLink != null)
                note.PrevLink.NextLink = note;
            nextLink.PrevLink = note;
        }

        /// <summary>
        /// Insert <paramref name="note"/> into another link after <paramref name="prevLink"/>,
        /// this method auto calls <see cref="UnlinkWithoutCutLinkChain(NoteData)"/> first
        /// </summary>
        public static void InsertAsLinkAfter(this NoteData note, NoteData prevLink)
        {
            if (note.IsSlide)
                note.UnlinkWithoutCutLinkChain();

            note.PrevLink = prevLink;
            note.NextLink = prevLink.NextLink;
            if (note.NextLink != null)
                note.NextLink.PrevLink = note;
            prevLink.NextLink = note;
        }

        /// <summary>
        /// Check whether the <paramref name="note"/> reached judgeline, the combo should plus or not
        /// </summary>
        public static bool IsComboNote(this IStageNoteModel note)
            => note is NoteTailModel or NoteModel { Data.IsHold: false };
    }
}