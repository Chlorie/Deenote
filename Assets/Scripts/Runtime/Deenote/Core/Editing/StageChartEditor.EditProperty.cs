#nullable enable

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Core.Editing.Operations;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using Deenote.Library.Collections;
using System;
using System.Runtime.InteropServices;

namespace Deenote.Core.Editing
{
    partial class StageChartEditor
    {
        #region Simple Edit Note Properties

        internal static readonly PianoSoundValueModel[] _defaultNoteSounds
            = new[] { new PianoSoundValueModel(0f, 0f, 72, 0) };

        private void OnNotePropertyEdited(bool notesVerticalPositionChanged, bool notesVisualDataChanged, NotificationFlag flag)
        {
            _game.AssertChartLoaded();
            NodeTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
            NotifyFlag(flag);
            _game.UpdateNotes(notesVerticalPositionChanged, notesVisualDataChanged);
        }

        public void EditSelectedNotesPositionCoord(Func<NoteCoord, NoteCoord> valueSelector)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            float clipLength = _game.MusicPlayer.ClipLength;
            _operations.Do(_game.CurrentChart
                .EditNotesCoord(Selector.SelectedNotes, v => NoteCoord.Clamp(valueSelector(v), clipLength))
                .OnDone(notes => OnNotePropertyEdited(true, true, NotificationFlag.NotePositionCoord)));
        }

        public void EditSelectedNotesTime(Func<float, float> valueSelector)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            float clipLength = _game.MusicPlayer.ClipLength;
            _operations.Do(_game.CurrentChart
                .EditNotesTime(Selector.SelectedNotes, v => EntityArgs.ClampTime(valueSelector(v), clipLength))
                .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteTime)));
        }

        public void EditSelectedNotesTime(float newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            float clipLength = _game.MusicPlayer.ClipLength;
            _operations.Do(_game.CurrentChart
                .EditNotesTime(Selector.SelectedNotes, EntityArgs.ClampTime(newValue, clipLength))
                .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteTime)));
        }

        public void EditSelectedNotesPosition(Func<float, float> valueSelector)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotesPosition(Selector.SelectedNotes, v => EntityArgs.ClampPosition(valueSelector(v)))
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NotePosition)));
        }

        public void EditSelectedNotesPosition(float newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotesPosition(Selector.SelectedNotes, EntityArgs.ClampPosition(newValue))
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NotePosition)));
        }

        public void EditSelectedNotesSize(Func<float, float> valueSelector)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, v => EntityArgs.ClampSize(valueSelector(v)),
                    n => n.Size, (n, v) => n.Size = v)
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSize)));
        }

        public void EditSelectedNotesSize(float newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, EntityArgs.ClampSize(newValue),
                    n => n.Size, (n, v) => n.Size = v)
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSize)));
        }

        public void EditSelectedNotesShift(float newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, newValue,
                    n => n.Shift, (n, v) => n.Shift = v)
                .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteShift)));
        }

        public void EditSelectedNotesSpeed(Func<float, float> valueSelector)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, v => EntityArgs.ClampNoteSpeed(valueSelector(v)),
                    n => n.Speed, (n, v) => n.Speed = v)
                .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteSpeed)));
        }

        public void EditSelectedNotesSpeed(float newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, EntityArgs.ClampNoteSpeed(newValue),
                    n => n.Speed, (n, v) => n.Speed = v)
                .OnDone(notes => OnNotePropertyEdited(true, false, NotificationFlag.NoteSpeed)));
        }

        public void EditSelectedNotesDuration(float newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(GetEditNotesDurationOperation(Selector.SelectedNotes, newValue));
        }

        public IUndoableOperation GetEditNotesDurationOperation(ReadOnlySpan<NoteModel> notes, float newValue)
        {
            _game.AssertChartLoaded();

            return _game.CurrentChart
                .EditNotesDuration(notes, newValue)
                .OnDone(notes => OnNotePropertyEdited(true, true, NotificationFlag.NoteDuration));
        }

        public void EditSelectedNotesVibrate(bool newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, newValue,
                    n => n.Vibrate, (n, v) => n.Vibrate = v)
                .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteVibrate)));
        }

        public void EditSelectedNotesKind(NoteModel.NoteKind newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotesKind(Selector.SelectedNotes, newValue)
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteKind)));
        }

        public void EditSelectedNotesWarningType(WarningType newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, newValue,
                    n => n.WarningType, (n, v) => n.WarningType = v)
                .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteWarningType)));
        }

        public void EditSelectedNotesEventId(string newValue)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotes(Selector.SelectedNotes, newValue,
                    n => n.EventId, (n, v) => n.EventId = v)
                .OnDone(notes => OnNotePropertyEdited(false, false, NotificationFlag.NoteEventId)));
        }

        public void EditSelectedNoteSounds(ReadOnlySpan<PianoSoundValueModel> values)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart
                .EditNotesSounds(Selector.SelectedNotes, values)
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSounds)));
        }

        /// <summary>
        /// Quick add or remove sound, if <paramref name="hasSound"/> but note already has sounds,
        /// do nothing.
        /// </summary>
        public void EditSelectedNoteSounds(bool hasSound)
        {
            if (!_game.IsChartLoaded())
                return;
            if (Selector.SelectedNotes.IsEmpty)
                return;

            using var so_editNotes = SpanOwner<NoteModel>.Allocate(Selector.SelectedNotes.Length);
            var editNotes = so_editNotes.Span;
            int index = 0;
            foreach (var note in Selector.SelectedNotes) {
                if (note.HasSounds != hasSound)
                    editNotes[index++] = note;
            }

            _operations.Do(_game.CurrentChart
                .EditNotesSounds(editNotes[..index], hasSound ? _defaultNoteSounds.AsSpan() : default)
                .OnDone(notes => OnNotePropertyEdited(false, true, NotificationFlag.NoteSounds)));
        }

        #endregion

        public void ApplySelectedNotesWithCurveTranform(GridsManager.CurveApplyProperty property)
        {
            switch (property) {
                case GridsManager.CurveApplyProperty.Size:
                    EditSelectedNotesSize(v => _game.Grids.GetCurveTransformedValue(v, GridsManager.CurveApplyProperty.Size) ?? v);
                    break;
                case GridsManager.CurveApplyProperty.Speed:
                    EditSelectedNotesSpeed(v => _game.Grids.GetCurveTransformedValue(v, GridsManager.CurveApplyProperty.Speed) ?? v);
                    break;
                default:
                    ThrowHelper.ThrowInvalidOperationException("Unknown curve apply property");
                    break;
            }
        }

        public void CreateHoldBetween(NoteModel head, NoteModel tail)
        {
            if (tail.Time == head.Time)
                return;

            if (tail.Time < head.Time)
                (head, tail) = (tail, head);

            var duration = tail.Time - head.Time;
            var rmv = GetRemoveNotesOperation(MemoryMarshal.CreateReadOnlySpan(ref tail, 1));
            var duredit = GetEditNotesDurationOperation(MemoryMarshal.CreateReadOnlySpan(ref head, 1), duration);
            OperationMemento.Do(new CombinedPairOperation(rmv, duredit));
        }

        public void InsertTempo(TempoRange range)
        {
            if (!_project.IsProjectLoaded())
                return;

            _operations.Do(_project.CurrentProject.InsertTempo(range)
                .OnDone(() => NotifyFlag(NotificationFlag.ProjectTempo)));
        }
    }
}