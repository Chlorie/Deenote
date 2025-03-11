#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Library.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class StageNoteSelector
    {
        private const float DragSelectionAreaMaxPosition = 6f;

        private GamePlayManager _game = default!;

        private readonly List<NoteModel> _selectedNotes = new();

        private NoteCoord _dragStartCoord;
        private NoteCoord _dragEndCoord;
        private readonly List<NoteModel> _inDragRangeNotes = new();

        public bool IsDragSelecting { get; private set; }

        public ReadOnlySpan<NoteModel> SelectedNotes => _selectedNotes.AsSpan();

        public event Action<StageNoteSelector>? SelectedNotesChanging;
        public event Action<StageNoteSelector>? SelectedNotesChanged;

        internal StageNoteSelector(GamePlayManager game)
        {
            _game = game;
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager => Clear());
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
            Debug.Assert(_game.CurrentChart.Search(note) >= 0);

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

        public void AddSelectMultiple(IEnumerable<NoteModel> notes)
        {
            _game.AssertChartLoaded();
            Debug.Assert(notes.All(note => _game.CurrentChart.Search(note) >= 0));

            OnSelectedNotesChanging();
            AddSelectMultipleNonNotify(notes);
            OnSelectedNotesChanged();
        }

        private void AddSelectMultipleNonNotify(IEnumerable<NoteModel> notes)
        {
            _game.AssertChartLoaded();
            Debug.Assert(notes.All(note => _game.CurrentChart.Search(note) >= 0));

            var prevCount = _selectedNotes.Count;
            _selectedNotes.AddRange(notes);
            foreach (var note in _selectedNotes.AsSpan()[prevCount..]) {
                note.IsSelected = true;
            }
        }

        public void Reselect(IEnumerable<NoteModel> notes)
        {
            OnSelectedNotesChanging();

            ClearNonNotify();
            AddSelectMultipleNonNotify(notes);

            OnSelectedNotesChanged();
        }

        public void SelectAll()
        {
            _game.AssertChartLoaded();

            OnSelectedNotesChanging();

            ClearNonNotify();
            foreach (var note in _game.CurrentChart.EnumerateNoteModels()) {
                AddSelectNonNotify(note);
            }

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

        public void Clear()
        {
            OnSelectedNotesChanging();
            ClearNonNotify();
            OnSelectedNotesChanged();
        }

        private void ClearNonNotify()
        {
            foreach (var note in _selectedNotes) {
                note.IsSelected = false;
            }
            _selectedNotes.Clear();
        }

        #endregion

        #region Drag select

        public void BeginDragSelect(NoteCoord startCoord, bool toggleMode)
        {
            if (!IsInDragSelectArea(startCoord))
                return;

            OnSelectedNotesChanging();

            _dragStartCoord = startCoord;
            _dragEndCoord = startCoord;
            IsDragSelecting = true;

            if (!toggleMode)
                ClearNonNotify();
            UpdateDragSelection(startCoord, startCoord);

            OnSelectedNotesChanged();
        }

        public void UpdateDragSelect(NoteCoord endCoord)
        {
            if (!IsDragSelecting)
                return;

            if (!IsInDragSelectArea(endCoord))
                return;

            OnSelectedNotesChanging();

            _dragEndCoord = endCoord;
            UpdateDragSelection(_dragStartCoord, _dragEndCoord);

            OnSelectedNotesChanged();
        }

        public void EndDragSelect()
        {
            if (!IsDragSelecting)
                return;

            _game.AssertStageLoaded();
            _game.Stage.SetSelectionPanelRectInvisible();

            IsDragSelecting = false;

            foreach (var note in _inDragRangeNotes) {
                note.ApplySelection();
            }
            _inDragRangeNotes.Clear();
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

            foreach (var note in _game.CurrentChart.EnumerateNoteModels()) {
                bool inRange = _game.IsNoteHighlighted(note) && IsInSelectionRange(note);

                note.SetIsInSelectionRange(inRange);
                if (inRange)
                    _inDragRangeNotes.Add(note);

                if (note.IsSelected) {
                    _selectedNotes.Add(note);
                }
            }

            NoteTimeComparer.AssertInOrder(_selectedNotes);

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

                // TODO: 目前如果note按时间顺序出现的话，在往下框选并倒退时间，可能导致一些在提前显示模式下能出现的note的选择情况怪怪的，
                // 理论上应该做个_game.EarlyDisplaySlowNotes的判断，但是在按时间顺序的模式下的选择判断太怪了，没想好怎么写

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
        }

        #endregion

        private void OnSelectedNotesChanging() => SelectedNotesChanging?.Invoke(this);
        private void OnSelectedNotesChanged()
        {
            foreach (var note in _game.NotesManager.OnStageNotes)
                note.RefreshColoring();
            SelectedNotesChanged?.Invoke(this);
        }
    }
}