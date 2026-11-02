#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class StageNoteSelector
    {
        private const float DragSelectionAreaMaxPosition = 6f;

        private GamePlayManager _game = default!;

        private readonly List<NoteModel> _selectedNotes = new();
        private readonly List<SpeedChangeWarningModel> _selectedWarnings = new();

        private NoteCoord _dragStartCoord;
        private NoteCoord _dragEndCoord;
        private readonly List<NoteModel> _inDragRangeNotes = new();
        private readonly List<SpeedChangeWarningModel> _inDragRangeWarnings = new();
        private DraggingSelectionState _state;

        public bool IsDragSelecting => _state != DraggingSelectionState.Idle;

        public ReadOnlySpan<NoteModel> SelectedNotes => _selectedNotes.AsSpan();
        public ReadOnlySpan<SpeedChangeWarningModel> SelectedWarnings => _selectedWarnings.AsSpan();

        public event Action<StageNoteSelector>? SelectedNotesChanging;
        public event Action<StageNoteSelector>? SelectedNotesChanged;
        public event Action<StageNoteSelector>? SelectedWarningsChanging;
        public event Action<StageNoteSelector>? SelectedWarningsChanged;

        internal StageNoteSelector(GamePlayManager game)
        {
            _game = game;
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager => Clear());

            _game.StageLoaded += _ =>
            {
                _game.Grids.RegisterNotification(
                    GridsManager.NotificationFlag.TimeGridVisible,
                    OnTimeGridVisibleChanged);
            };

            _game.MusicPlayer.TimeChanged += args =>
            {
                if (!IsDragSelecting)
                    return;

                var delta = args.NewTime - args.OldTime;
                _dragEndCoord.Time += delta;
                UpdateDragSelection(_dragStartCoord, _dragEndCoord);
            };
        }

        #region Select Collection Modification

        public void AddSelect(NoteModel note)
        {
            _game.AssertChartLoaded();
            Debug.Assert(_game.CurrentChart.NoteNodes.Contains(note));

            if (note.IsSelected)
                return;

            OnSelectedNotesChanging();
            AddSelectNonNotify(note);
            OnSelectedNotesChanged();
        }

        private void AddSelectNonNotify(NoteModel note)
        {
            _selectedNotes.Add(note);
            note.IsSelected = true;
        }

        public void AddSelectWarning(SpeedChangeWarningModel warning)
        {
            _game.AssertChartLoaded();

            if (warning.IsSelected)
                return;

            OnSelectedWarningsChanging();
            AddSelectWarningNonNotify(warning);
            OnSelectedWarningsChanged();
        }

        private void AddSelectWarningNonNotify(SpeedChangeWarningModel warning)
        {
            _selectedWarnings.Add(warning);
            warning.IsSelected = true;
        }

        public void AddSelectMultiple(IEnumerable<NoteModel> notes)
        {
            _game.AssertChartLoaded();
            Debug.Assert(notes.All(note => _game.CurrentChart.NoteNodes.Contains(note)));

            OnSelectedNotesChanging();
            AddSelectMultipleNonNotify(notes);
            OnSelectedNotesChanged();
        }

        private void AddSelectMultipleNonNotify(IEnumerable<NoteModel> notes)
        {
            _game.AssertChartLoaded();
            Debug.Assert(notes.All(note => _game.CurrentChart.NoteNodes.Contains(note)));

            var prevCount = _selectedNotes.Count;
            _selectedNotes.AddRange(notes);
            foreach (var note in _selectedNotes.AsSpan()[prevCount..]) {
                note.IsSelected = true;
            }
        }

        private void AddSelectMultipleNonNotify(ReadOnlySpan<NoteModel> notes)
        {
            _game.AssertChartLoaded();
            Debug.Assert(notes.ToArray().All(note => _game.CurrentChart.NoteNodes.Contains(note)));

            var prevCount = _selectedNotes.Count;
            _selectedNotes.AddRange(notes);
            foreach (var note in _selectedNotes.AsSpan()[prevCount..]) {
                note.IsSelected = true;
            }
        }

        public void Reselect(IEnumerable<NoteModel> notes)
        {
            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();

            ClearNonNotify();
            ClearWarningsNonNotify();
            AddSelectMultipleNonNotify(notes);

            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        public void Reselect(ReadOnlySpan<NoteModel> notes)
        {
            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();

            ClearNonNotify();
            ClearWarningsNonNotify();
            AddSelectMultipleNonNotify(notes);

            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        public void ReselectWarning(SpeedChangeWarningModel warning)
        {
            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();

            ClearNonNotify();
            ClearWarningsNonNotify();
            AddSelectWarningNonNotify(warning);

            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        public void SelectAll()
        {
            _game.AssertChartLoaded();

            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();

            ClearNonNotify();
            ClearWarningsNonNotify();
            foreach (var note in _game.CurrentChart.EnumerateNoteModels()) {
                AddSelectNonNotify(note);
            }

            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        public void Deselect(NoteModel note)
        {
            OnSelectedNotesChanging();

            if (_selectedNotes.Remove(note))
                note.IsSelected = false;

            OnSelectedNotesChanged();
        }

        /// <summary>
        /// Deselect notes in selection, if note is not selected, do nothing
        /// </summary>
        public void DeselectMultiple(IEnumerable<NoteModel> notes)
        {
            OnSelectedNotesChanging();

            foreach (var note in notes) {
                if (note.IsSelected) {
                    var remove = _selectedNotes.Remove(note);
                    Debug.Assert(remove is true);
                    note.IsSelected = false;
                }
            }

            OnSelectedNotesChanged();
        }

        public void DeselectAt(Index index)
        {
            var i = index.GetCheckedOffset(_selectedNotes.Count);

            OnSelectedNotesChanging();

            var note = _selectedNotes[i];
            _selectedNotes.RemoveAt(i);
            note.IsSelected = false;

            OnSelectedNotesChanged();
        }

        public void DeselectWarning(SpeedChangeWarningModel warning)
        {
            OnSelectedWarningsChanging();

            if (_selectedWarnings.Remove(warning))
                warning.IsSelected = false;

            OnSelectedWarningsChanged();
        }

        public void ClearWarnings()
        {
            OnSelectedWarningsChanging();
            ClearWarningsNonNotify();
            OnSelectedWarningsChanged();
        }

        private void OnTimeGridVisibleChanged(GridsManager grids)
        {
            if (grids.TimeGridVisible)
                return;

            // Warnings are drawn as a part of the time grid, so they cannot be selected when it is hidden
            if (_selectedWarnings.Count == 0 && _inDragRangeWarnings.Count == 0)
                return;

            foreach (var warning in _inDragRangeWarnings) {
                warning.SetIsInSelectionRange(false);
            }
            _inDragRangeWarnings.Clear();

            ClearWarnings();
        }

        public void Clear()
        {
            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();
            ClearNonNotify();
            ClearWarningsNonNotify();
            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        private void ClearNonNotify()
        {
            foreach (var note in _selectedNotes) {
                note.IsSelected = false;
            }
            _selectedNotes.Clear();
        }

        private void ClearWarningsNonNotify()
        {
            foreach (var warning in _selectedWarnings) {
                warning.IsSelected = false;
            }
            _selectedWarnings.Clear();
        }

        #endregion

        #region Drag select

        public void BeginDragSelect(NoteCoord startCoord, bool toggleMode)
        {
            if (!IsInDragSelectArea(startCoord))
                return;

            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();

            _dragStartCoord = startCoord;
            _dragEndCoord = startCoord;
            _state = DraggingSelectionState.StartedSelection;

            if (!toggleMode) {
                ClearNonNotify();
                ClearWarningsNonNotify();
            }

            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        public void UpdateDragSelect(NoteCoord endCoord)
        {
            if (_state is DraggingSelectionState.Idle)
                return;
            _state = DraggingSelectionState.Dragging;

            if (!IsInDragSelectArea(endCoord))
                return;

            OnSelectedNotesChanging();
            OnSelectedWarningsChanging();

            _dragEndCoord = endCoord;
            UpdateDragSelection(_dragStartCoord, _dragEndCoord);

            OnSelectedWarningsChanged();
            OnSelectedNotesChanged();
        }

        public void EndDragSelect(Vector2 viewportPoint)
        {
            if (_state is DraggingSelectionState.Idle)
                return;

            _game.AssertStageLoaded();
            _game.Stage.SetSelectionPanelRectInvisible();

            _state = DraggingSelectionState.Idle;
            bool isClick = (_dragStartCoord == _dragEndCoord) || (_inDragRangeNotes.Count == 0 && _inDragRangeWarnings.Count == 0);
            if (isClick) {
                if (_game.Grids.TimeGridVisible
                    && _game.TryRaycastPerspectiveViewportPointToWarning(viewportPoint, out var warningController)) {
                    ReselectWarning(warningController.Model);
                }
                else if (_game.TryRaycastPerspectiveViewportPointToNote(viewportPoint, out var noteController)) {
                    var note = noteController.NoteModel;
                    Reselect(MemoryMarshal.CreateReadOnlySpan(ref note, 1));
                }
            }
            else {
                foreach (var note in _inDragRangeNotes) {
                    note.ApplySelection();
                }
                _inDragRangeNotes.Clear();

                foreach (var warning in _inDragRangeWarnings) {
                    warning.ApplySelection();
                }
                _inDragRangeWarnings.Clear();
            }
        }

        private bool IsInDragSelectArea(NoteCoord coord)
        {
            if (coord.Position is > DragSelectionAreaMaxPosition or < -DragSelectionAreaMaxPosition)
                return false;
            return true;
        }

        private void UpdateDragSelection(NoteCoord startCoord, NoteCoord endCoord)
        {
            _game.AssertChartLoaded();
            _game.AssertStageLoaded();
            NumberUtils.SortAsc(ref startCoord.Position, ref endCoord.Position);
            NumberUtils.SortAsc(ref startCoord.Time, ref endCoord.Time);

            _game.Stage.SetSelectionPanelRect(startCoord, endCoord);

            // Optimize:现在是全遍历
            // TODO: should consider note sprite size
            // 我在考虑从raycast映射后的coord直接就是考虑sprite size后的coord，
            // 这个size问题能不能试着在raycast2coord的方法里解决
            _selectedNotes.Clear();
            _selectedWarnings.Clear();

            foreach (var note in _game.CurrentChart.EnumerateNoteModels()) {
                bool inRange = _game.IsNoteHighlighted(note) && IsInSelectionRange(note);

                note.SetIsInSelectionRange(inRange);
                if (inRange)
                    _inDragRangeNotes.Add(note);

                if (note.IsSelected) {
                    _selectedNotes.Add(note);
                }
            }

            bool warningsSelectable = _game.Grids.TimeGridVisible;
            foreach (var warning in _game.CurrentChart.SpeedChangeWarnings) {
                bool inRange = warningsSelectable && IsWarningInSelectionRange(warning);

                warning.SetIsInSelectionRange(inRange);
                if (inRange)
                    _inDragRangeWarnings.Add(warning);

                if (warning.IsSelected) {
                    _selectedWarnings.Add(warning);
                }
            }

            NodeTimeComparer.AssertInOrder(_selectedNotes);

            bool IsInSelectionRange(NoteModel note)
            {
                float pos = note.Position;
                float halfSize = note.Size / 2f;
                float time = note.Time;
                float speed = note.Speed;
                float currentTime = _game.MusicPlayer.Time;

                if (time < currentTime) {
                    return time >= startCoord.Time
                        && time <= endCoord.Time
                        && pos + halfSize >= startCoord.Position
                        && pos - halfSize <= endCoord.Position;
                }

                // TODO: Currently, select by drag down, and play stage backward, may make some note in EarlyDisplay mode have strange selection state
                // Theoretically I should judge if _game.EarlyDisplaySlowNotes, but its weird selecting in TimeOrder mode,
                // still considering how to implement t

                // The note is on stage
                if (time < currentTime + _game.GetStageNoteAppearAheadTime(speed)) {
                    var pseudoTime = ToPseudoTime(time);
                    return pseudoTime >= startCoord.Time
                        && pseudoTime <= endCoord.Time
                        && pos + halfSize >= startCoord.Position
                        && pos - halfSize <= endCoord.Position;
                }
                // The note is not on stage
                else {
                    var pseudoTime = ToAboveStagePseudoTime(time, speed);
                    return pseudoTime >= startCoord.Time
                        && pseudoTime <= endCoord.Time
                        && pos + halfSize >= startCoord.Position
                        && pos - halfSize <= endCoord.Position;
                }

                float ToPseudoTime(float time)
                    => currentTime + (time - currentTime) * _game.GetDisplayNoteSpeed(speed);

                float ToAboveStagePseudoTime(float time, float speed)
                    => time + (_game.StageNoteAppearAheadTime - _game.GetStageNoteAppearAheadTime(speed));
            }

            bool IsWarningInSelectionRange(SpeedChangeWarningModel warning)
            {
                float pos = GameStageSpeedChangeWarningNoteController.SpritePosition;
                const float halfSize = 0.5f;
                float time = warning.Time;
                float currentTime = _game.MusicPlayer.Time;
                float displaySpeed = _game.HighlightedNoteSpeed;

                if (time < currentTime) {
                    return time >= startCoord.Time
                        && time <= endCoord.Time
                        && pos + halfSize >= startCoord.Position
                        && pos - halfSize <= endCoord.Position;
                }

                if (time < currentTime + _game.GetStageNoteAppearAheadTime(displaySpeed)) {
                    var pseudoTime = currentTime + (time - currentTime) * _game.GetDisplayNoteSpeed(displaySpeed);
                    return pseudoTime >= startCoord.Time
                        && pseudoTime <= endCoord.Time
                        && pos + halfSize >= startCoord.Position
                        && pos - halfSize <= endCoord.Position;
                }

                var aboveStageTime = time + (_game.StageNoteAppearAheadTime - _game.GetStageNoteAppearAheadTime(displaySpeed));
                return aboveStageTime >= startCoord.Time
                    && aboveStageTime <= endCoord.Time
                    && pos + halfSize >= startCoord.Position
                    && pos - halfSize <= endCoord.Position;
            }
        }

        #endregion

        private void OnSelectedNotesChanging() => SelectedNotesChanging?.Invoke(this);
        private void OnSelectedNotesChanged()
        {
            foreach (var note in _game.NotesManager.OnStageNotes)
                note.RefreshColoring();
            SelectedNotesChanged?.Invoke(this);
        }

        private void OnSelectedWarningsChanging() => SelectedWarningsChanging?.Invoke(this);
        private void OnSelectedWarningsChanged()
        {
            foreach (var warning in _game.SpeedChangeWarningsManager.OnStageWarnings)
                warning.RefreshColoring();
            SelectedWarningsChanged?.Invoke(this);
        }

        private enum DraggingSelectionState
        {
            Idle,
            StartedSelection,
            Dragging,
        }
    }
}