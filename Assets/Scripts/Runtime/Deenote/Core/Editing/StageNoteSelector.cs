#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.Collections;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class StageNoteSelector : FlagNotifiable<StageNoteSelector, StageNoteSelector.NotificationFlag>
    {
        private const float DragSelectionAreaMaxPosition = 4f;

        private RectTransform _dragRangeIndicatorRectTransform = default!;
        private GamePlayManager _game = default!;

        private readonly List<NoteModel> _selectedNotes = new();
        private NoteCoord _dragStartCoord;
        private NoteCoord _dragEndCoord;
        private readonly List<NoteModel> _inDragRangeNotes = new();

        public bool IsDragSelecting { get; private set; }

        public ReadOnlySpan<NoteModel> SelectedNotes => _selectedNotes.AsSpan();

        internal StageNoteSelector(GamePlayManager game)
        {
            _game = game;
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager => Clear());
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.GameStageLoaded,
                manager =>
                {
                    manager.AssertStageLoaded();
                    _dragRangeIndicatorRectTransform = manager.Stage.NoteDragSelectionPanelTransform;
                });
            _game.MusicPlayer.TimeChanged += args =>
            {
                if (!IsDragSelecting)
                    return;

                var delta = args.NewTime - args.OldTime;
                _dragEndCoord.Time += delta;
                UpdateDragSelection(_dragStartCoord, _dragEndCoord);
            };
        }

        public void AddSelect(NoteModel note)
        {
            _game.AssertChartLoaded();
            Debug.Assert(_game.CurrentChart.Search(note) >= 0);

            if (note.IsSelected)
                return;

            NotifyFlag(NotificationFlag.SelectedNotesChanging);
            AddSelectNonNotify(note);
            NotifyFlag(NotificationFlag.SelectedNotesChanged);
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

            NotifyFlag(NotificationFlag.SelectedNotesChanging);
            AddSelectMultipleNonNotify(notes);
            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        private void AddSelectMultipleNonNotify(IEnumerable<NoteModel> notes)
        {
            _game.AssertChartLoaded();
            Debug.Assert(notes.All(note => _game.CurrentChart.Search(note) >= 0));

            var prevCount = _selectedNotes.Count;
            _selectedNotes.AddRange(notes);
            if (prevCount > 0) {
                foreach (var note in _selectedNotes.AsSpan()[prevCount..]) {
                    note.IsSelected = true;
                }
            }
        }

        public void Reselect(IEnumerable<NoteModel> notes)
        {
            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            ClearNonNotify();
            AddSelectMultipleNonNotify(notes);

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        public void SelectAll()
        {
            _game.AssertChartLoaded();

            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            ClearNonNotify();
            foreach (var note in _game.CurrentChart.EnumerateNoteModels()) {
                AddSelectNonNotify(note);
            }

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        public void Deselect(NoteModel note)
        {
            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            if (_selectedNotes.Remove(note))
                note.IsSelected = false;

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        /// <summary>
        /// Deselect notes in selection, if note is not selected, do nothing
        /// </summary>
        public void DeselectMultiple(IEnumerable<NoteModel> notes)
        {
            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            foreach (var note in notes) {
                if (note.IsSelected) {
                    var remove = _selectedNotes.Remove(note);
                    Debug.Assert(remove is true);
                    note.IsSelected = false;
                }
            }

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        public void DeselectAt(Index index)
        {
            var i = index.GetCheckedOffset(_selectedNotes.Count);

            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            var note = _selectedNotes[i];
            _selectedNotes.RemoveAt(i);
            note.IsSelected = false;

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        public void Clear()
        {
            NotifyFlag(NotificationFlag.SelectedNotesChanging);
            ClearNonNotify();
            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        private void ClearNonNotify()
        {
            foreach (var note in _selectedNotes) {
                note.IsSelected = false;
            }
            _selectedNotes.Clear();
        }

        #region Drag select

        public void BeginDragSelect(NoteCoord startCoord, bool toggleMode)
        {
            if (!IsInDragSelectArea(startCoord))
                return;

            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            _dragRangeIndicatorRectTransform.gameObject.SetActive(true);

            _dragStartCoord = startCoord;
            _dragEndCoord = startCoord;
            IsDragSelecting = true;

            if (!toggleMode)
                ClearNonNotify();
            UpdateDragSelection(startCoord, startCoord);

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        public void UpdateDragSelect(NoteCoord endCoord)
        {
            if (!IsInDragSelectArea(endCoord))
                return;

            if (!IsDragSelecting)
                return;

            NotifyFlag(NotificationFlag.SelectedNotesChanging);

            Debug.Assert(_dragRangeIndicatorRectTransform.gameObject.activeSelf);
            _dragEndCoord = endCoord;
            UpdateDragSelection(_dragStartCoord, _dragEndCoord);

            NotifyFlag(NotificationFlag.SelectedNotesChanged);
        }

        public void EndDragSelect()
        {
            if (!IsDragSelecting)
                return;

            _dragRangeIndicatorRectTransform.gameObject.SetActive(false);
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

            Utils.SortAsc(ref startCoord.Position, ref endCoord.Position);
            Utils.SortAsc(ref startCoord.Time, ref endCoord.Time);

            (float xMin, float zMin) = _game.Stage.ConvertNoteCoordToWorldPosition(startCoord - new NoteCoord(position: 0, _game.MusicPlayer.Time));
            (float xMax, float zMax) = _game.Stage.ConvertNoteCoordToWorldPosition(endCoord - new NoteCoord(position: 0, _game.MusicPlayer.Time));

            _dragRangeIndicatorRectTransform.offsetMin = new(xMin, zMin);
            _dragRangeIndicatorRectTransform.offsetMax = new(xMax, zMax);

            // Optimize
            // TODO: should consider note sprite size
            // 我在考虑从raycast映射后的coord直接就是考虑sprite size后的coord，
            // 这个size问题能不能试着在raycast2coord的方法里解决
            _selectedNotes.Clear();

            foreach (var note in _game.CurrentChart.EnumerateNoteModels()) {
                float notePos = note.Position;
                float halfNoteSize = note.Size / 2f;
                float noteTime = note.Time;
                bool inRange = noteTime >= startCoord.Time
                    && noteTime <= endCoord.Time
                    && notePos + halfNoteSize >= startCoord.Position
                    && notePos - halfNoteSize <= endCoord.Position;

                note.SetIsInSelectionRange(inRange);
                if (inRange)
                    _inDragRangeNotes.Add(note); // TODO: 这个不会重复加？

                if (note.IsSelected) {
                    _selectedNotes.Add(note);
                }
            }

            NoteTimeComparer.AssertInOrder(_selectedNotes);
        }

        #endregion

        public enum NotificationFlag
        {
            SelectedNotesChanging,
            SelectedNotesChanged,
        }
    }
}