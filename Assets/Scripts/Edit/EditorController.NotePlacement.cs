using Deenote.Edit.Elements;
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

        [SerializeField] private bool __isNoteIndicatorOn;
        [SerializeField] private bool __snapToPositionGrid;
        [SerializeField] private bool __snapToTimeGrid;

        [Header("Clip Board")]
        [SerializeField] private float _clipBoardBasePosition;
        [SerializeField] private List<NoteData> _clipBoardNotes;

        [SerializeField] private bool _isPasting;

        public bool IsNoteIndicatorOn
        {
            get => __isNoteIndicatorOn;
            set {
                if (__isNoteIndicatorOn == value)
                    return;

                __isNoteIndicatorOn = value;
                if (__isNoteIndicatorOn) {
                    UpdateNoteIndicator();
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
                PasteNoteAt(coord, rememberPosition);
            }
            else {
                PlaceNoteAt(coord);
            }

            NoteTimeComparer.AssertInOrder(_stage.Chart.Data.Notes);

            void PasteNoteAt(NoteCoord coord, bool rememberPosition)
            {
                if (rememberPosition) {
                    coord = _stage.Grids.Quantize(new(_clipBoardBasePosition, coord.Time), false, SnapToTimeGrid);
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
                    // Note: 旧Dnt在撤销时，会重新选中与撤销前谱面被选中id相同的note
                    // 撤销后被选中的note基本为无意义note，还会影响已选note
                    // 因此这里选择不对选择进行修改
                    // TODO: 被删了的note也可能被选中，这种情况下会怎么样？
                    .WithUndoneAction(() => OnNotesChanged(true, false)));
                _isPasting = false;
                UpdateNoteIndicator();
            }

            void PlaceNoteAt(NoteCoord coord)
            {
                coord = _stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                _operationHistory.Do(_stage.Chart.Notes.AddNote(coord, _placeNoteTemplate)
                    // TODO: dnt下键时会取消选择，但是undo时不会恢复
                    // 由于完全没看懂怎么实现的所以先这样。
                    // 效果理论上一致
                    .WithRedoneAction(() =>
                    {
                        OnNoteSelectionChanging();
                        _noteSelectionController.ClearSelection();
                        OnNotesChanged(true, true);
                    })
                    .WithUndoneAction(() => OnNotesChanged(true, false)));
            }
        }

        // TODO: 编辑之后，需要手动检测一下Note的collision情况，除了这里，还有NotePlacement
        public void RemoveSelectedNotes()
        {
            // __selectedNotes.Sort(NoteTimeComparer.Instance);
            _operationHistory.Do(_stage.Chart.Notes.RemoveNotes(SelectedNotes)
                // TODO: 目前删除时会取消选择，undo时不会恢复
                // 考虑在RemoveNoteOperataion添加恢复时将被删note添加回_selectedNotes的逻辑
                // PS: 如果_selectedNotes不为空，undo时保留已有notes。
                .WithRedoneAction(() =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.ClearSelection();
                    OnNotesChanged(true, true);
                })
                .WithUndoneAction((removedNotes) =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.SelectNotes(removedNotes);
                    OnNotesChanged(true, true);
                }));

            NoteTimeComparer.AssertInOrder(_stage.Chart.Data.Notes);
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
                var coord = _stage.Grids.Quantize(new(0f, time), true, false);
                list.Add(new NoteData { PositionCoord = coord, });
            }
            _operationHistory.Do(_stage.Chart.Notes.AddMultipleNotes(new NoteCoord(0f, startTime), list)
                .WithRedoneAction(notes =>
                {
                    OnNoteSelectionChanging();
                    _noteSelectionController.SelectNotes(notes);
                    OnNotesChanged(true, true);
                })
                .WithUndoneAction(() => OnNotesChanged(true, false)));

            ListPool<NoteData>.Release(list);
        }

        #region NoteIndicator

        /// <summary>
        /// Get which notes to display, does not update position here
        /// </summary>
        private void UpdateNoteIndicator()
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
                data.Position -= baseNote.Position;
                data.Time -= baseNote.Time;
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
            UpdateNoteIndicator();
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