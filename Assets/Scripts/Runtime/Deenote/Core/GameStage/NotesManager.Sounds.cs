#nullable enable

using Deenote.Entities.Models;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    partial class NotesManager
    {
        private int _nextHitBackgroundNoteIndex;
        private int _nextHitNoteIndex;

        // TODO:NExt在gpmanager调用，顺便需要在发生变动时更新上面两个变量

        private void UpdateNoteSoundsRelatively(bool forward, bool playSound)
        {
            _game.AssertChartLoaded();

            int prevHitNoteIndex = _nextHitNoteIndex;
            int prevHitBackgroundNoteIndex = _nextHitBackgroundNoteIndex;

            var chart = _game.CurrentChart;
            var musicPlayer = _game.MusicPlayer;

            if (forward) {
                while (_nextHitNoteIndex < chart.NoteNodes.Length && chart.NoteNodes[_nextHitNoteIndex].Time <= musicPlayer.Time)
                    _nextHitNoteIndex++;
                while (_nextHitBackgroundNoteIndex < chart.BackgroundNotes.Length && chart.BackgroundNotes[_nextHitBackgroundNoteIndex].Time <= musicPlayer.Time)
                    _nextHitBackgroundNoteIndex++;
            }
            else {
                while (_nextHitNoteIndex > 0 && chart.NoteNodes[_nextHitNoteIndex - 1].Time > musicPlayer.Time)
                    _nextHitNoteIndex--;
                while (_nextHitBackgroundNoteIndex > 0 && chart.BackgroundNotes[_nextHitBackgroundNoteIndex - 1].Time > musicPlayer.Time)
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

            for (int i = prevHitNoteIndex; i < _nextHitNoteIndex; i++) {
                if (chart.NoteNodes[i] is NoteModel note) {
                    _game.HitSoundPlayer.PlaySound(note.Kind);
                    _game.PianoSoundPlayer.PlaySounds(note.Sounds);
                }
            }
            for (int i = prevHitBackgroundNoteIndex; i < _nextHitBackgroundNoteIndex; i++) {
                _game.PianoSoundPlayer.PlaySounds(chart.BackgroundNotes[i].Sounds);
            }
        }

        private void UpdateNoteSoundsIndices()
        {
            _game.AssertChartLoaded();
            _nextHitNoteIndex = 0;
            _nextHitBackgroundNoteIndex = 0;
            var chart = _game.CurrentChart;
            var musicPlayer = _game.MusicPlayer;

            while (_nextHitNoteIndex < chart.NoteNodes.Length && chart.NoteNodes[_nextHitNoteIndex].Time <= musicPlayer.Time)
                _nextHitNoteIndex++;
            while (_nextHitBackgroundNoteIndex < chart.BackgroundNotes.Length && chart.BackgroundNotes[_nextHitBackgroundNoteIndex].Time <= musicPlayer.Time)
                _nextHitBackgroundNoteIndex++;
        }
    }
}