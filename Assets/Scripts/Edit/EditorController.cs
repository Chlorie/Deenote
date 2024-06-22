using Deenote.Edit.Elements;
using Deenote.GameStage;
using Deenote.Project;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.Windows;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Edit
{
    public sealed class EditorController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage;

        [Header("Notify")]
        [SerializeField] PropertiesWindow _propertiesWindow;
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;

        [Header("Note Selection")]
        [SerializeField] RectTransform _noteSelectionIndicatorTransform;
        private NoteCoord _noteSelectionIndicatorStartCoord;
        /// <summary>
        /// May not in order
        /// </summary>
        private List<NoteModel> _selectedNotes;

        [Header("Note Edit")]
        [SerializeField] NoteData _placeNoteTemplate;
        private UndoableOperationHistory _operationHistory;
        [Header("Placement Assistant")]
        [SerializeField] Transform _noteIndicatorParentTransform; // Also note panel
        [SerializeField] NoteIndicatorController _noteIndicatorPrefab;
        private ObjectPool<NoteIndicatorController> _noteIndicatorPool;
        private List<NoteIndicatorController> _noteIndicators;
        private List<NoteData> _clipBoardNotes;
        [SerializeField] private bool __isNoteIndicatorOn;
        [SerializeField] private bool __snapToPositionGrid;
        [SerializeField] private bool __snapToTimeGrid;
        private bool _isPasting;

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
                    foreach (var noteIndicator in _noteIndicators) {
                        _noteIndicatorPool.Release(noteIndicator);
                    }
                    _noteIndicators.Clear();
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
                _editorPropertiesWindow.NotifyHorizontalGridSnapChanged(value);
            }
        }

        private void Awake()
        {
            _selectedNotes = new();
            _placeNoteTemplate = new NoteData() { Size = 1f, };
            _operationHistory = new(100);

            _noteIndicatorPool = UnityUtils.CreateObjectPool(_noteIndicatorPrefab, _noteIndicatorParentTransform, 0);
            _noteIndicators = new();
            _clipBoardNotes = new();
        }

        private void Start()
        {
            SnapToPositionGrid = SnapToTimeGrid = true;
            IsNoteIndicatorOn = true;
        }

        #region Selection

        public void SelectAllNotes()
        {
            _selectedNotes.Clear();
            _selectedNotes.Capacity = _stage.Chart.Notes.Count;
            foreach (var note in _stage.Chart.Notes) {
                note.IsSelected = true;
                _selectedNotes.Add(note);
            }
            _stage.ForceUpdateNotesDisplay();
            _propertiesWindow.NotifyNoteSelectionChanged(_selectedNotes);
        }

        public void StartNoteSelection(NoteCoord startCoord, bool deselectPrevious)
        {
            _noteSelectionIndicatorTransform.gameObject.SetActive(true);

            startCoord.Position = Mathf.Clamp(startCoord.Position, -2f, 2f);
            startCoord.Time = Mathf.Clamp(startCoord.Time, 0f, _stage.MusicLength);
            _noteSelectionIndicatorStartCoord = startCoord;

            UpdateNoteSelectionInternal(startCoord, startCoord, deselectPrevious);
        }

        public void UpdateNoteSelection(NoteCoord endCoord)
        {
            endCoord.Position = Mathf.Clamp(endCoord.Position, -2f, 2f);
            endCoord.Time = Mathf.Clamp(endCoord.Time, 0f, _stage.MusicLength);

            UpdateNoteSelectionInternal(_noteSelectionIndicatorStartCoord, endCoord, false);
        }

        public void EndNoteSelection()
        {
            _noteSelectionIndicatorTransform.gameObject.SetActive(false);
        }

        private void UpdateNoteSelectionInternal(NoteCoord startCoord, NoteCoord endCoord, bool deselectPrevious)
        {
            Debug.Assert(startCoord.Position is >= -2f and <= 2f);
            Debug.Assert(endCoord.Position is >= -2f and <= 2f);

            if (startCoord.Position > endCoord.Position) {
                (startCoord.Position, endCoord.Position) = (endCoord.Position, startCoord.Position);
            }
            if (startCoord.Time > endCoord.Time) {
                (startCoord.Time, endCoord.Time) = (endCoord.Time, startCoord.Time);
            }

            (float xMin, float zMin) = MainSystem.Args.NoteCoordToWorldPosition(startCoord, _stage.CurrentMusicTime);
            (float xMax, float zMax) = MainSystem.Args.NoteCoordToWorldPosition(endCoord, _stage.CurrentMusicTime);

            _noteSelectionIndicatorTransform.offsetMin = new(xMin, zMin);
            _noteSelectionIndicatorTransform.offsetMax = new(xMax, zMax);

            // TODO: Optimize
            foreach (var note in _stage.Chart.Notes) {
                if (!deselectPrevious && note.IsSelected)
                    continue;

                float notePos = note.Data.Position;
                float halfNoteSize = note.Data.Size / 2;
                float noteTime = note.Data.Time;
                bool isSelected = notePos + halfNoteSize >= startCoord.Position
                    && notePos - halfNoteSize <= endCoord.Position
                    && noteTime >= startCoord.Time
                    && noteTime <= endCoord.Time;

                switch (note.IsSelected, isSelected) {
                    case (true, false):
                        _selectedNotes.Remove(note);
                        note.IsSelected = false;
                        break;
                    case (false, true):
                        _selectedNotes.Add(note);
                        note.IsSelected = true;
                        break;
                }
            }

            _stage.ForceUpdateNotesDisplay();
            _propertiesWindow.NotifyNoteSelectionChanged(_selectedNotes);
        }

        private void ClearSelection(bool autoNotify = true)
        {
            foreach (var note in _selectedNotes) {
                note.IsSelected = false;
            }
            _selectedNotes.Clear();
            if (autoNotify) {
                _stage.UpdateStageNotes(false);
                _propertiesWindow.NotifyNoteSelectionChanged(_selectedNotes);
            }
        }

        public void EditSelectedNotes(Action<NoteData> edit)
        {
            // TODO: Unable to undo now
            foreach (var note in _selectedNotes) {
                edit(note.Data);
            }
        }

        #endregion

        #region Undo

        public void Undo() => _operationHistory.Undo();

        public void Redo() => _operationHistory.Redo();

        #endregion

        public void PlaceNoteAt(NoteCoord coord)
        {
            coord = _stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
            _operationHistory.Do(_stage.Chart.Notes.Add(coord, _placeNoteTemplate)
                // TODO: dnt下键时会取消选择，但是undo时不会恢复
                // 由于完全没看懂怎么实现的所以先这样。
                // 效果理论上一致
                .WithRedoneAction(() => ClearSelection())
                .WithUndoneAction(() => _stage.UpdateStageNotes(false)));

            NoteTimeComparer.AssertInOrder(_stage.Chart.Data.Notes);
        }

        public void RemoveSelectedNotes()
        {
            _selectedNotes.Sort(NoteTimeComparer.Instance);
            _operationHistory.Do(_stage.Chart.Notes.RemoveNotes(_selectedNotes)
                // TODO: 目前删除时会取消选择，undo时不会恢复
                // 考虑在RemoveNoteOperataion添加恢复时将被删note添加回_selectedNotes的逻辑
                // PS: 如果_selectedNotes不为空，undo时保留已有notes。
                .WithRedoneAction(() => ClearSelection())
                .WithUndoneAction((removedNotes) =>
                {
                    foreach (var note in removedNotes) {
                        note.IsSelected = true;
                    }
                    _selectedNotes.AddRange(removedNotes);
                    _stage.UpdateStageNotes(false);
                    _propertiesWindow.NotifyNoteSelectionChanged(_selectedNotes);
                }));

            NoteTimeComparer.AssertInOrder(_stage.Chart.Data.Notes);
        }

        #region NoteIndicator

        private void UpdateNoteIndicator()
        {
            if (_noteIndicators.Count > 0) {
                foreach (var noteIndicator in _noteIndicators) {
                    _noteIndicatorPool.Release(noteIndicator);
                }
            }

            if (_isPasting) {
                // TODO:
            }
            else {
                var noteIndi = _noteIndicatorPool.Get();
                noteIndi.Initialize(_placeNoteTemplate);
                _noteIndicators.Add(noteIndi);
            }
        }

        public void MoveNoteIndicator(NoteCoord? mousePosition)
        {
            if (!IsNoteIndicatorOn)
                return;

            if (!mousePosition.HasValue) {
                _noteIndicatorParentTransform.gameObject.SetActive(false);
                return;
            }

            _noteIndicatorParentTransform.gameObject.SetActive(true);
            var qPos = _stage.Grids.Quantize(NoteCoord.ClampPosition(mousePosition.Value), SnapToPositionGrid, SnapToTimeGrid);
            _noteIndicators[0].MoveTo(qPos);
        }

        #endregion
    }
}
