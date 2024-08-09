using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using UnityEngine.Pool;

namespace Deenote.Edit
{
    partial class EditorController
    {
        #region Simple Properties

        public void EditSelectedNotesPositionCoord(Func<NoteCoord, NoteCoord> valueSelector)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, valueSelector, n => n.PositionCoord, (n, v) => n.PositionCoord = v)
                .WithDoneAction(() =>
                {
                    // TODO: Ensure Notes Order
                    _propertiesWindow.NotifyNoteTimeChanged(SelectedNotes.IsSameForAll(n => n.Data.Time, out var time) ? time : null);
                    _propertiesWindow.NotifyNotePositionChanged(SelectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: false);
                }));
        }

        public void EditSelectedNotesTime(Func<float, float> valueSelector)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, valueSelector, n => n.Time, (n, v) => n.Time = MainSystem.Args.ClampNoteTime(v))
                .WithDoneAction(() =>
                {
                    // TODO: Ensure Notes Order
                    _propertiesWindow.NotifyNoteTimeChanged(SelectedNotes.IsSameForAll(n => n.Data.Time, out var time) ? time : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: false);
                }));
        }

        public void EditSelectedNotesTime(float newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Time, (n, v) => n.Time = MainSystem.Args.ClampNoteTime(v))
                .WithDoneAction(() =>
                {
                    // TODO: Ensure Notes Order
                    _propertiesWindow.NotifyNoteTimeChanged(SelectedNotes.IsSameForAll(n => n.Data.Time, out var time) ? time : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesPosition(Func<float, float> valueSelector)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, valueSelector, n => n.Position, (n, v) => n.Position = MainSystem.Args.ClampNotePosition(v))
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNotePositionChanged(SelectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesPosition(float newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Position, (n, v) => n.Position = MainSystem.Args.ClampNotePosition(v))
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNotePositionChanged(SelectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSize(Func<float, float> valueSelector)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, valueSelector, n => n.Size, (n, v) => n.Size = MainSystem.Args.ClampNoteSize(v))
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteSizeChanged(SelectedNotes.IsSameForAll(n => n.Data.Size, out var size) ? size : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSize(float newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Size, (n, v) => n.Size = MainSystem.Args.ClampNoteSize(v))
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteSizeChanged(SelectedNotes.IsSameForAll(n => n.Data.Size, out var size) ? size : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesShift(float newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Shift, (n, v) => n.Shift = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteShiftChanged(SelectedNotes.IsSameForAll(n => n.Data.Shift, out var shift) ? shift : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSpeed(float newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Speed, (n, v) => n.Speed = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteSpeedChanged(SelectedNotes.IsSameForAll(n => n.Data.Speed, out var speed) ? speed : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesDuration(float newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Duration, (n, v) => n.Duration = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteDurationChanged(SelectedNotes.IsSameForAll(n => n.Data.Duration, out var duration) ? duration : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesVibrate(bool newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.Vibrate, (n, v) => n.Vibrate = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteVibrateChanged(SelectedNotes.IsSameForAll(n => n.Data.Vibrate, out var vibrate) ? vibrate : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesIsSwipe(bool newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.IsSwipe, (n, v) => n.IsSwipe = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteIsSwipeChanged(SelectedNotes.IsSameForAll(n => n.Data.IsSwipe, out var swipe) ? swipe : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesWarningType(WarningType newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.WarningType, (n, v) => n.WarningType = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteWarningTypeChanged(SelectedNotes.IsSameForAll(n => n.Data.WarningType, out var wType) ? wType : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesEventId(string newValue)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotes(SelectedNotes, newValue, n => n.EventId, (n, v) => n.EventId = v)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteEventIdChanged(SelectedNotes.IsSameForAll(n => n.Data.EventId, out var evId) ? evId : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        #endregion

        public void EditSelectedNoteSounds(PianoSoundValueData[] values)
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotesSounds(SelectedNotes, values)
                .WithDoneAction(() =>
                {
                    _propertiesWindow.NotifyNotePianoSoundsChanged(SelectedNotes.IsSameForAll(n => n.Data.Sounds, out var sounds, PianoSoundListDataEqualityComparer.Instance) ? sounds : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void LinkSelectedNotes()
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.LinkNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteIsLinkChanged(true);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteIsLinkChanged(SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void UnlinkSelectedNotes()
        {
            if (SelectedNotes.Count == 0)
                return;

            _operationHistory.Do(Stage.Chart.Notes.UnlinkNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteIsLinkChanged(false);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(() =>
                {
                    _propertiesWindow.NotifyNoteIsLinkChanged(SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void ToggleSoundOfSelectedNotes()
        {
            // TODO: Un-undoable
            using var _ = ListPool<NoteData>.Get(out var soundNotes);

            foreach (var note in SelectedNotes) {
                var data = note.Data;
                if (!data.HasSound) {
                    data.Sounds.Add(new PianoSoundData(0, 0f, 72, 0));
                    continue;
                }

                foreach (var sound in data.Sounds) {
                    if (sound.Velocity > 0) {
                        var soundNote = data.Clone();
                        soundNote.Sounds.Add(sound);
                        soundNotes.Add(soundNote);
                    }
                }
            }
            if (soundNotes.Count > 0) {
                Stage.Chart.Data.Notes.AddRange(soundNotes);
                // TODO: Should sort stably
                Stage.Chart.Data.Notes.Sort(NoteTimeComparer.Instance);
            }
        }
    }
}