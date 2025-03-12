#nullable enable

using CommunityToolkit.HighPerformance;
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

            // HACK: 播放过程中拖动进度条时，会有MusicPlayer.Update里的播放时SetTime反而使Time变小的情况。
            // 所以原本是Debug.Assert改成了if判断。
            // 后续再解决
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

        private void UpdateNoteSoundsIndices()
        {
            _game.AssertChartLoaded();
            _nextHitSoundNoteIndex = 0;
            _nextHitBackgroundNoteIndex = 0;
            var chart = _game.CurrentChart;
            var musicPlayer = _game.MusicPlayer;

            while (_nextHitSoundNoteIndex < chart.NoteNodes.Count && chart.NoteNodes[_nextHitSoundNoteIndex].Time <= musicPlayer.Time)
                _nextHitSoundNoteIndex++;
            while (_nextHitBackgroundNoteIndex < chart.BackgroundSoundNotes.Count && chart.BackgroundSoundNotes [_nextHitBackgroundNoteIndex].Time <= musicPlayer.Time)
                _nextHitBackgroundNoteIndex++;
        }
    }
}