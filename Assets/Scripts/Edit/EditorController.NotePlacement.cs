using Deenote.Edit.Elements;
using Deenote.Edit.Operations;
using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Edit
{
    partial class EditorController
    {
        [Header("Note Placement")]
        [SerializeField] NoteData _placeNoteTemplate;
        [SerializeField] Transform _noteIndicatorParentTransform; // Also note panel
        [SerializeField] NoteIndicatorController _noteIndicatorPrefab;
        private PooledObjectListView<NoteIndicatorController> _noteIndicatorList;

        [Header("Clip Board")]
        [SerializeField] private float _clipBoardBasePosition;
        [SerializeField] private List<NoteData> _clipBoardNotes;
        [SerializeField] private bool _isPasting;

        [SerializeField] private bool __isNoteIndicatorOn;
        [SerializeField] private bool __snapToPositionGrid;
        [SerializeField] private bool __snapToTimeGrid;

        public bool IsNoteIndicatorOn
        {
            get => __isNoteIndicatorOn;
            set {
                if (__isNoteIndicatorOn == value)
                    return;

                __isNoteIndicatorOn = value;
                if (__isNoteIndicatorOn) {
                    RefreshNoteIndicator();
                }
                else {
                    _noteIndicatorList.Clear();
                }

                _editorPropertiesWindow.NotifyShowIndicatorChanged(__isNoteIndicatorOn);
            }
        }

        public bool SnapToPositionGrid
        {
            get => __snapToPositionGrid;
            set {
                if (__snapToPositionGrid == value)
                    return;
                __snapToPositionGrid = value;
                _editorPropertiesWindow.NotifyVerticalGridSnapChanged(value);
            }
        }

        public bool SnapToTimeGrid
        {
            get => __snapToTimeGrid;
            set {
                if (__snapToTimeGrid == value)
                    return;
                __snapToTimeGrid = value;
                _editorPropertiesWindow.NotifyTimeGridSnapChanged(value);
            }
        }

        private void AwakeNotePlacement()
        {
            _placeNoteTemplate = new NoteData { Size = 1f };
            _noteIndicatorList = new PooledObjectListView<NoteIndicatorController>(
                UnityUtils.CreateObjectPool(_noteIndicatorPrefab, _noteIndicatorParentTransform, 1));
            _clipBoardNotes = new();
        }

        public void PlaceNoteAt(NoteCoord coord, bool rememberPosition)
        {
            if (_isPasting) {
                PasteNote();
            }
            else {
                PlaceNote();
            }

            NoteTimeComparer.AssertInOrder(_stage.Chart.Notes);

            void PasteNote()
            {
                if (rememberPosition) {
                    coord = _stage.Grids.Quantize(new(coord.Time, _clipBoardBasePosition), false, SnapToTimeGrid);
                }
                else {
                    coord = _stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                }
                _operationHistory.Do(_stage.Chart.Notes.AddMultipleNotes(coord, _clipBoardNotes)
                    .WithRedoneAction(notes =>
                    {
                        OnNoteSelectionChanging();
                        _noteSelectionController.SelectNotes(notes);
                        OnNotesChanged(true, true);
                    })
                    .WithUndoneAction(notes =>
                    {
                        OnNoteSelectionChanging();
                        _noteSelectionController.DeselectNotes(notes);
                        OnNotesChanged(true, true);
                    }));
                _isPasting = false;
                RefreshNoteIndicator();
            }

            void PlaceNote()
            {
                coord = _stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                _operationHistory.Do(_stage.Chart.Notes.AddNote(coord, _placeNoteTemplate)
                    .WithRedoneAction(() =>
                    {
                        OnNoteSelectionChanging();
                        _noteSelectionController.ClearSelection();
                        OnNotesChanged(true, true);
                    })
                    .WithUndoneAction(() => OnNotesChanged(true, false)));
            }
        }

        public void RemoveSelectedNotes()
        {
            if (SelectedNotes.Count == 0) {
                _operationHistory.Do(UndoableOperation.DoNothing);
                return;
            }

            // __selectedNotes.Sort(NoteTimeComparer.Instance);
            _operationHistory.Do(_stage.Chart.Notes.RemoveNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.ClearSelection();
                    _propertiesWindow.NotifyNoteIsLinkChanged(false);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction((removedNotes) =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.AddNote(removedNotes);
                    _propertiesWindow.NotifyNoteIsLinkChanged(SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                }));

            NoteTimeComparer.AssertInOrder(_stage.Chart.Notes);
        }

        public void AddNotesSnappingToCurve(int count)
        {
            var curveTime = _stage.Grids.CurveTime;
            if (curveTime is null)
                return;
            var (startTime, endTime) = curveTime.Value;

            var list = ListPool<NoteData>.Get();
            list.Capacity = Mathf.Max(count, list.Capacity);
            for (int i = 0; i < count; i++) {
                var time = startTime + (endTime - startTime) / (count + 1) * (i + 1);
                var coord = _stage.Grids.Quantize(new(time, 0f), true, false);
                list.Add(new NoteData { PositionCoord = coord, });
            }
            _operationHistory.Do(_stage.Chart.Notes.AddMultipleNotes(new NoteCoord(startTime, 0f), list)
                .WithRedoneAction(notes =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.SelectNotes(notes);
                    OnNotesChanged(true, true);
                })
                .WithUndoneAction(notes =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.DeselectNotes(notes);
                    OnNotesChanged(true, true);
                }));

            ListPool<NoteData>.Release(list);
        }

        #region NoteIndicator

        /// <summary>
        /// Get which notes to display, does not update position here
        /// </summary>
        private void RefreshNoteIndicator()
        {
            if (!IsNoteIndicatorOn)
                return;

            if (_isPasting) {
                using var indicators = _noteIndicatorList.Resetting();
                foreach (var note in _clipBoardNotes) {
                    indicators.Add(out var indicator);
                    indicator.Initialize(note);
                }
            }
            else {
                _noteIndicatorList.SetCount(1);
                _noteIndicatorList[0].Initialize(_placeNoteTemplate);
            }
        }

        public void MoveNoteIndicator(NoteCoord mousePosition, bool rememberPosition)
        {
            if (!IsNoteIndicatorOn)
                return;

            _noteIndicatorParentTransform.gameObject.SetActive(true);
            if (_isPasting) {
                var coord = mousePosition;
                if (rememberPosition) {
                    coord.Position = _clipBoardBasePosition;
                    coord = _stage.Grids.Quantize(coord, false, SnapToTimeGrid);
                }
                else {
                    coord = _stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                }

                Debug.Assert(_noteIndicatorList.Count == _clipBoardNotes.Count);
                for (int i = 0; i < _noteIndicatorList.Count; i++) {
                    var indicator = _noteIndicatorList[i];
                    var note = _clipBoardNotes[i];
                    indicator.MoveTo(NoteCoord.ClampPosition(coord + note.PositionCoord));
                }
            }
            else {
                var qPos = _stage.Grids.Quantize(NoteCoord.ClampPosition(mousePosition), SnapToPositionGrid, SnapToTimeGrid);
                _noteIndicatorList[0].MoveTo(qPos);
            }
        }

        public void HideNoteIndicator()
        {
            _noteIndicatorParentTransform.gameObject.SetActive(false);
        }

        #endregion

        #region Copy Paste

        public void CopySelectedNotes()
        {
            _isPasting = false;

            _clipBoardNotes.Clear();
            if (SelectedNotes.Count == 0)
                return;

            // __selectedNotes.Sort(NoteTimeComparer.Instance);
            NoteData baseNote = SelectedNotes[0].Data;
            _clipBoardBasePosition = baseNote.Position;

            using var __sn_dict = DictionaryPool<NoteData, NoteData>.Get(out var slideNotes);
            foreach (var note in SelectedNotes) {
                var data = note.Data.Clone();
                data.PositionCoord -= baseNote.PositionCoord;
                _clipBoardNotes.Add(data);

                data.PrevLink = data.NextLink = null;
                if (data.IsSlide) {
                    slideNotes.Add(note.Data, data);

                    NoteData prevLinkNote = note.Data.PrevLink;
                    NoteData copiedPrev = null;
                    while (prevLinkNote != null && !slideNotes.TryGetValue(prevLinkNote, out copiedPrev))
                        prevLinkNote = prevLinkNote.PrevLink;

                    // prevLink is null || copiedPrev is not null
                    if (prevLinkNote != null) {
                        data.PrevLink = copiedPrev;
                        copiedPrev.NextLink = data;
                    }
                }
            }
        }

        public void CutSelectedNotes()
        {
            CopySelectedNotes();
            RemoveSelectedNotes();
        }

        public void PasteNotes()
        {
            if (_clipBoardNotes.Count == 0)
                return;

            _isPasting = true;
            RefreshNoteIndicator();
        }

        #endregion

        #region Notify

        public void NotifyIsShowLinkLinesChanged(bool value)
        {
            foreach (var note in _noteIndicatorList) {
                note.UpdateLinkLineVisibility(value);
            }
        }

        public void NotifyCurveGeneratedWithSelectedNotes()
        {
            _noteSelectionController.DeselectNoteAt(^1);
            _noteSelectionController.DeselectNoteAt(0);
            RemoveSelectedNotes();
        }

        #endregion
    }
}