#nullable enable

using Deenote.Core.GamePlay.Audio;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    partial class NotesManager
    {
        private const double AudioLookaheadSeconds = 0.25d;
        private const double MinAudioScheduleLeadSeconds = 0.01d;

        private long _scheduledAudioEpoch;
        private int _nextScheduledNoteIndex;
        private int _nextScheduledBackgroundNoteIndex;
        private bool _scheduledAudioInitialized;

        /// <summary>
        /// Schedules chart audio ahead of the DSP clock. This is deliberately
        /// separate from the render-time update path.
        /// </summary>
        internal void ScheduleAudio()
        {
            if (!_game.IsChartLoaded())
                return;

            var musicPlayer = _game.MusicPlayer;
            var transport = musicPlayer.Transport;

            if (!transport.CanSchedule)
                return;

            double dspNow = AudioSettings.dspTime;
            if (!_scheduledAudioInitialized) {
                ResetScheduledAudio(
                    transport.ChartTimeAtDsp(dspNow),
                    transport.Epoch);
            }

            var chart = _game.CurrentChart;
            double horizonChartTime = transport.ChartTimeAtDsp(dspNow + AudioLookaheadSeconds);

            while (_nextScheduledNoteIndex < chart.NoteNodes.Count) {
                var node = chart.NoteNodes[_nextScheduledNoteIndex];
                if (node.Time > horizonChartTime)
                    break;

                // Holds intentionally trigger only at their head. NoteTailNode
                // participates in display/combo logic but never schedules audio.
                if (node is NoteModel note) {
                    double targetDspTime = transport.DspTimeAtChart(note.Time);
                    if (targetDspTime >= dspNow + MinAudioScheduleLeadSeconds) {
                        _game.HitSoundPlayer.ScheduleSound(note.Kind, targetDspTime);
                        foreach (var sound in note.Sounds) {
                            double soundDspTime = transport.DspTimeAtChart(note.Time + sound.Delay);
                            _game.PianoSoundPlayer.ScheduleSound(sound, soundDspTime);
                        }
                    }
                }

                _nextScheduledNoteIndex++;
            }

            while (_nextScheduledBackgroundNoteIndex < chart.BackgroundSoundNotes.Count) {
                var note = chart.BackgroundSoundNotes[_nextScheduledBackgroundNoteIndex];
                if (note.Time > horizonChartTime)
                    break;

                double targetDspTime = transport.DspTimeAtChart(note.Time);
                if (targetDspTime >= dspNow + MinAudioScheduleLeadSeconds) {
                    foreach (var sound in note.Sounds) {
                        double soundDspTime = transport.DspTimeAtChart(note.Time + sound.Delay);
                        _game.PianoSoundPlayer.ScheduleSound(sound, soundDspTime);
                    }
                }

                _nextScheduledBackgroundNoteIndex++;
            }
        }

        private void ResetScheduledAudio(double chartTime, long epoch)
        {
            _scheduledAudioEpoch = epoch;
            _scheduledAudioInitialized = true;

            if (!_game.IsChartLoaded()) {
                _nextScheduledNoteIndex = 0;
                _nextScheduledBackgroundNoteIndex = 0;
                return;
            }

            var chart = _game.CurrentChart;
            _nextScheduledNoteIndex = chart.NoteNodes.AsSpan()
                .FindLowerBoundIndex(new NodeTimeComparable((float)chartTime));
            _nextScheduledBackgroundNoteIndex = chart.BackgroundSoundNotes.AsSpan()
                .FindLowerBoundIndex(new NodeTimeComparable((float)chartTime));
        }

        internal void CancelScheduledAudio()
        {
            _scheduledAudioInitialized = false;
            _game.HitSoundPlayer.CancelScheduledSounds();
            _game.PianoSoundPlayer.CancelScheduledSounds();
        }

        internal void CancelPendingAudio()
        {
            _scheduledAudioInitialized = false;
            _game.HitSoundPlayer.CancelPendingSounds();
            _game.PianoSoundPlayer.CancelPendingSounds();
        }

        internal void RebaseScheduledAudio()
        {
            _scheduledAudioInitialized = false;
        }
    }
}
