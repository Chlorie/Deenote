#nullable enable

using CommunityToolkit.HighPerformance;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    partial class NotesManager
    {
        private int _nextHitBackgroundNoteIndex;
        private int _nextHitSoundNoteIndex;

        private void UpdateNoteSoundsRelatively(bool forward, bool playSound)
        {
            _game.AssertChartLoaded();

            int prevHitNoteIndex = _nextHitSoundNoteIndex;
            int prevHitBackgroundNoteIndex = _nextHitBackgroundNoteIndex;

            var chart = _game.CurrentChart;
            var musicPlayer = _game.MusicPlayer;

            if (forward) {
                while (_nextHitSoundNoteIndex < chart.NoteNodes.Count && chart.NoteNodes[_nextHitSoundNoteIndex].Time <= musicPlayer.Time)
                    _nextHitSoundNoteIndex++;
                while (_nextHitBackgroundNoteIndex < chart.BackgroundSoundNotes.Count && chart.BackgroundSoundNotes[_nextHitBackgroundNoteIndex].Time <= musicPlayer.Time)
                    _nextHitBackgroundNoteIndex++;
            }
            else {
                while (_nextHitSoundNoteIndex > 0 && chart.NoteNodes[_nextHitSoundNoteIndex - 1].Time > musicPlayer.Time)
                    _nextHitSoundNoteIndex--;
                while (_nextHitBackgroundNoteIndex > 0 && chart.BackgroundSoundNotes[_nextHitBackgroundNoteIndex - 1].Time > musicPlayer.Time)
                    _nextHitBackgroundNoteIndex--;
            }

            if (!playSound)
                return;
            if (!musicPlayer.IsPlaying)
                return;

            // HACK: If drag time slider while music player, in MusicPlay.Update the SetTime may cause Time smaller
            // So I changed the Debug.Assert to if statement. Maybe handle it later
            if (!forward)
                return;

            for (int i = prevHitNoteIndex; i < _nextHitSoundNoteIndex; i++) {
                if (chart.NoteNodes[i] is NoteModel note) {
                    _game.HitSoundPlayer.PlaySound(note.Kind);
                    _game.PianoSoundPlayer.PlaySounds(note.Sounds.AsSpan());
                }
            }
            for (int i = prevHitBackgroundNoteIndex; i < _nextHitBackgroundNoteIndex; i++) {
                _game.PianoSoundPlayer.PlaySounds(chart.BackgroundSoundNotes[i].Sounds);
            }
        }

        private void RefreshNoteSoundsIndices()
        {
            _game.AssertChartLoaded();
            var chart = _game.CurrentChart;
            var musicPlayer = _game.MusicPlayer;

            _nextHitSoundNoteIndex = chart.NoteNodes.AsSpan().FindUpperBoundIndex(new NodeTimeComparable(musicPlayer.Time));
            _nextHitBackgroundNoteIndex = chart.BackgroundSoundNotes.AsSpan().FindUpperBoundIndex(new NodeTimeComparable(musicPlayer.Time));
        }
    }
}