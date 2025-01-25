#nullable enable

using Deenote.Entities.Models;

namespace Deenote.GamePlay
{
    partial class GamePlayManager
    {
        private int _nextHitBackgroundNoteIndex;
        private int _nextHitNoteIndex;

        // TODO:NExt在gpmanager调用，顺便需要在发生变动时更新上面两个变量
        // 考虑把这个文件和Notes.cs都拆到新的类里。

        private void Update_NoteSounds()
        {
            if (_manualPlaySpeedMultiplier != 0f)
                return;
            if (!MusicPlayer.IsPlaying)
                return;

            AssertChartLoaded();

            while (true) {
                var node = CurrentChart.NoteNodes[_nextHitNoteIndex];
                if (node.Time <= MusicPlayer.Time) {
                    if (node is NoteModel note) {
                        if (note.IsSlide)
                            HitSoundPlayer.PlaySlideSound();
                        else
                            HitSoundPlayer.PlayClickSound();

                        if (note.HasSounds)
                            PianoSoundPlayer.PlaySounds(note.Sounds);
                    }
                    _nextHitNoteIndex++;
                }
                else
                    break;
            }

            while (true) {
                var note = CurrentChart.BackgroundNotes[_nextHitBackgroundNoteIndex];
                if (note.Time <= MusicPlayer.Time) {
                    PianoSoundPlayer.PlaySounds(note.Sounds);
                    _nextHitBackgroundNoteIndex++;
                }
                else
                    break;
            }
        }

        private void UpdateNoteSounds(bool forward, bool manually)
        {
            //AssertChartLoaded();

            //if (forward) {
            //    while (_nextHitNoteIndex < CurrentChart.NoteNodes.Count && CurrentChart.NoteNodes[_nextHitNoteIndex].Time <= MusicPlayer.Time)
            //        _nextHitNoteIndex++;
            //    while (_nextHitBackgroundNoteIndex < CurrentChart.BackgroundNotes.Count && CurrentChart.BackgroundNotes[_nextHitBackgroundNoteIndex].Time <= MusicPlayer.Time)
            //        _nextHitBackgroundNoteIndex++;
            //}
            //else {
            //    while (_nextHitNoteIndex > 0 && CurrentChart.NoteNodes[_nextHitNoteIndex - 1].Time > MusicPlayer.Time)
            //        _nextHitNoteIndex--;
            //    while (_nextHitBackgroundNoteIndex > 0 && CurrentChart.BackgroundNotes[_nextHitBackgroundNoteIndex - 1].Time > MusicPlayer.Time)
            //        _nextHitBackgroundNoteIndex--;
            //}
        }
    }
}