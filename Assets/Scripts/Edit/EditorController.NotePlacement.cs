#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Edit.Elements;
using Deenote.GameStage;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Edit
{
    partial class EditorController
    {
        private const float NoteIndicatorIsSwipeChangeThreshold = 30f;
        private const float NoteIndicatorIsHoldChangeThreshold = 30f;
        private const float NoteIndicatorIsSwipeCotangent = 1.732f;

        [Header("Note Placement")]
        [SerializeField] private NoteData _placeNoteTemplate = null!;
        [SerializeField] private Transform _noteIndicatorParentTransform = null!; // Also note panel
        [SerializeField] private NoteIndicatorController _noteIndicatorPrefab = null!;
        private PooledObjectListView<NoteIndicatorController> _noteIndicatorList;

        [Header("Clip Board")]
        [SerializeField] private float _clipBoardBasePosition;
        [SerializeField] private PooledObjectListView<NoteData> _clipBoardNotes = new(NoteData.Pool);

        [SerializeField] private PlacementState _placeState;
        [SerializeField] private PlacementOptions __placeOptions;
        private (Vector2 Screen, NoteCoord Coord) _freezeMousePosition; // Available on _placementState is Placing
        private NoteCoord _prevMouseCoord; // Available when _placeState is PlacingLinks

        [SerializeField] private bool __isNoteIndicatorOn;
        [SerializeField] private bool __snapToPositionGrid;
        [SerializeField] private bool __snapToTimeGrid;

        public bool IsPlacing => _placeState is PlacementState.Placing or PlacementState.PlacingLinks;

        public PlacementOptions PlaceOptions
        {
            get => __placeOptions;
            set {
                if (__placeOptions == value)
                    return;
                var diff = __placeOptions ^ value;
                __placeOptions = value;

                if (diff.HasFlag(PlacementOptions.PlaceSlide)) {
                    switch (_placeState) {
                        case PlacementState.Idle or PlacementState.Placing:
                            Debug.Assert(_noteIndicatorList.Count == 1);
                            _placeNoteTemplate.IsSlide = __placeOptions.HasFlag(PlacementOptions.PlaceSlide);
                            _noteIndicatorList[0].Initialize(_placeNoteTemplate);
                            break;
                        case PlacementState.PlacingLinks:
                            break;
                        case PlacementState.Pasting:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

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

                _propertyChangeNotifier.Invoke(this, NotifyProperty.IsIndicatorOn);
            }
        }

        public bool SnapToPositionGrid
        {
            get => __snapToPositionGrid;
            set {
                if (__snapToPositionGrid == value)
                    return;
                __snapToPositionGrid = value;
                _propertyChangeNotifier.Invoke(this, NotifyProperty.SnapToPositionGrid);
            }
        }

        public bool SnapToTimeGrid
        {
            get => __snapToTimeGrid;
            set {
                if (__snapToTimeGrid == value)
                    return;
                __snapToTimeGrid = value;
                _propertyChangeNotifier.Invoke(this, NotifyProperty.SnapToTimeGrid);
            }
        }

        #region Unity

        private void AwakeNotePlacement()
        {
            _placeNoteTemplate = new NoteData { Size = 1f };
            _noteIndicatorList = new PooledObjectListView<NoteIndicatorController>(
                UnityUtils.CreateObjectPool(_noteIndicatorPrefab, _noteIndicatorParentTransform, defaultCapacity: 1));
        }

        private void Start_NotePlacement()
        {
            Stage.MusicController.OnTimeChanged += (oldVal, newVal, manully) =>
            {
                Vector2 mousePosition = Input.mousePosition;
                if (Stage.PerspectiveView.TryConvertScreenPointToNoteCoord(mousePosition, out var coord)) {
                    MoveNoteIndicator(mousePosition, coord);
                }
                else {
                    HideNoteIndicator();
                }
            };
        }

        #endregion

        public void BeginPlaceNote(Vector2 mousePositionScreen, NoteCoord coord)
        {
            if (!IsInNoteSelectionArea(coord))
                return;

            // When pasting, we needn't change the note type when mouse moving
            if (_placeState is PlacementState.Pasting)
                return;

            _freezeMousePosition = (mousePositionScreen, Stage.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid));

            if (__placeOptions.HasFlag(PlacementOptions.PlaceSlide)) {
                _placeState = PlacementState.PlacingLinks;
                _prevMouseCoord = _freezeMousePosition.Coord;
            }
            else {
                _placeState = PlacementState.Placing;
            }
        }

        public void EndPlaceNote(NoteCoord coord)
        {
            if (Stage.Chart is null)
                return;
            switch (_placeState) {
                case PlacementState.Pasting: PasteNote(); break;
                case PlacementState.Placing: PlaceNote(); break;
                case PlacementState.PlacingLinks: PlaceLink(); break;
                // Cancelled
                case PlacementState.Idle or _: return;
            }

            CancelPlaceNote();

            void PasteNote()
            {
                if (_clipBoardNotes.Count == 0)
                    return;

                if (__placeOptions.HasFlag(PlacementOptions.PastingRememberPosition))
                    coord = Stage.Grids.Quantize(new(_clipBoardBasePosition, coord.Time), false, SnapToTimeGrid);
                else
                    coord = Stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);

                _operationHistory.Do(Stage.Chart.Notes.AddMultipleNotes(coord, _clipBoardNotes.AsSpan())
                    .WithRedoneAction(notes =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                        _noteSelectionController.ClearSelection();
                        _noteSelectionController.SelectNotes(notes.OfType<NoteModel>());
                        OnNotesChanged(true, true);
                    })
                    .WithUndoneAction(notes =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                        _noteSelectionController.DeselectNotes(notes.OfType<NoteModel>());
                        OnNotesChanged(true, true);
                    }));
            }

            void PlaceNote()
            {
                NoteCoord placeCoord;
                if (_placeNoteTemplate.IsHold && coord.Time < _freezeMousePosition.Coord.Time)
                    // If placing hold by dragging down, the actual time of hold is the end coord's time
                    placeCoord = new NoteCoord(coord.Time, _freezeMousePosition.Coord.Position);
                else
                    placeCoord = _freezeMousePosition.Coord;
                placeCoord = Stage.Grids.Quantize(placeCoord, SnapToPositionGrid, SnapToTimeGrid);
                _operationHistory.Do(Stage.Chart.Notes.AddNote(placeCoord, _placeNoteTemplate)
                    .WithRedoneAction(() =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                        _noteSelectionController.ClearSelection();
                        OnNotesChanged(true, true);
                        NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    })
                    .WithUndoneAction(() => OnNotesChanged(true, false)));
            }

            void PlaceLink()
            {
                coord = Stage.Grids.Quantize(_freezeMousePosition.Coord, SnapToPositionGrid, SnapToTimeGrid);
                using var owner = SpanOwner<NoteData>.Allocate(_noteIndicatorList.Count);
                var span = owner.Span;
                for (int i = 0; i < span.Length; i++) {
                    span[i] = _noteIndicatorList[i].NotePrototype;
                }
                _operationHistory.Do(Stage.Chart.Notes.AddMultipleNotes(coord, span)
                    .WithRedoneAction(notes =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                        _noteSelectionController.ClearSelection();
                        _noteSelectionController.SelectNotes(notes.OfType<NoteModel>());
                        OnNotesChanged(true, true);
                    })
                    .WithUndoneAction(notes =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                        _noteSelectionController.DeselectNotes(notes.OfType<NoteModel>());
                        OnNotesChanged(true, true);
                    }));

                // Requires manually set if Options.PlaceSlide changed when placing link
                _placeNoteTemplate.IsSlide = PlaceOptions.HasFlag(PlacementOptions.PlaceSlide);
            }
        }

        public void MoveNoteIndicator(Vector2 mouseScreenPosition, NoteCoord mouseCoordPosition)
        {
            if (!IsNoteIndicatorOn)
                return;

            _noteIndicatorParentTransform.gameObject.SetActive(true);
            switch (_placeState) {
                case PlacementState.Pasting: {
                    if (IsInNoteSelectionArea(mouseCoordPosition))
                        MoveCopied();
                    else
                        HideNoteIndicator();
                    break;
                }
                case PlacementState.Placing:
                    AdjustPlacingNoteKind(); break;
                case PlacementState.PlacingLinks:
                    DragLink(); break;
                case PlacementState.Idle or _: {
                    if (IsInNoteSelectionArea(mouseCoordPosition))
                        MoveIndicator();
                    else
                        HideNoteIndicator();
                    break;
                }
            }

            void MoveCopied()
            {
                var coord = mouseCoordPosition;
                if (__placeOptions.HasFlag(PlacementOptions.PastingRememberPosition)) {
                    coord.Position = _clipBoardBasePosition;
                    coord = Stage.Grids.Quantize(coord, false, SnapToTimeGrid);
                }
                else {
                    coord = Stage.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                }

                Debug.Assert(_noteIndicatorList.Count == _clipBoardNotes.Count);
                for (int i = 0; i < _noteIndicatorList.Count; i++) {
                    var indicator = _noteIndicatorList[i];
                    var note = _clipBoardNotes[i];
                    indicator.MoveTo(NoteCoord.ClampPosition(coord + note.PositionCoord));
                }
            }

            void AdjustPlacingNoteKind()
            {
                Debug.Assert(_noteIndicatorList.Count == 1);
                var delta = MathUtils.Abs(mouseScreenPosition - _freezeMousePosition.Screen);
                // 鼠标移动方向与水平线的夹角在30°以内时绘制swipe
                delta.y *= NoteIndicatorIsSwipeCotangent;
                if (delta.x >= NoteIndicatorIsSwipeChangeThreshold && delta.x > delta.y) {
                    _placeNoteTemplate.IsSwipe = true;
                    _placeNoteTemplate.Duration = 0f;
                    var indicator = _noteIndicatorList[0];
                    indicator.Initialize(_placeNoteTemplate);
                    // indicator may be moved if create hold by dragging down
                    indicator.MoveTo(_freezeMousePosition.Coord);
                }
                else if (delta.y >= NoteIndicatorIsHoldChangeThreshold && delta.x <= delta.y) {
                    _placeNoteTemplate.IsSwipe = false;
                    var mouseCoordTime = Stage.Grids.Quantize(mouseCoordPosition, false, SnapToTimeGrid).Time;

                    if (mouseCoordTime >= _freezeMousePosition.Coord.Time) {
                        _placeNoteTemplate.Duration = mouseCoordTime - _freezeMousePosition.Coord.Time;
                        var indicator = _noteIndicatorList[0];
                        indicator.Initialize(_placeNoteTemplate);
                    }
                    else {
                        _placeNoteTemplate.Duration = _freezeMousePosition.Coord.Time - mouseCoordTime;
                        var indicator = _noteIndicatorList[0];
                        indicator.Initialize(_placeNoteTemplate);
                        indicator.MoveTo(new NoteCoord(mouseCoordTime, _freezeMousePosition.Coord.Position));
                    }
                }
                else {
                    _placeNoteTemplate.IsSwipe = false;
                    _placeNoteTemplate.Duration = 0f;
                    var indicator = _noteIndicatorList[0];
                    indicator.Initialize(_placeNoteTemplate);
                    // indicator may be moved if create hold by dragging down
                    indicator.MoveTo(_freezeMousePosition.Coord);
                }
            }

            void MoveIndicator()
            {
                Debug.Assert(_noteIndicatorList.Count == 1);
                var qPos = Stage.Grids.Quantize(NoteCoord.ClampPosition(mouseCoordPosition),
                    SnapToPositionGrid, SnapToTimeGrid);
                _noteIndicatorList[0].MoveTo(qPos);
            }

            void DragLink()
            {
                var startCoordTime = _freezeMousePosition.Coord.Time;
                var currentCoordTime = mouseCoordPosition.Time;
                var prevCoordTime = _prevMouseCoord.Time;
                // When the mouse hovers over a time grid, generate an indicator at the mouse position.
                // Move upward
                if (currentCoordTime > startCoordTime) {
                    if (prevCoordTime < startCoordTime) {
                        // If previous frame, mouse is below the base note, remove all generated notes
                        _noteIndicatorList.RemoveRange(..^1);
                        prevCoordTime = startCoordTime;
                    }
                    if (currentCoordTime > prevCoordTime) {
                        float compareTime = prevCoordTime;
                        // Optimizable: 
                        while (true) {
                            //                         TODO: It could be more clear if we have FloorToNearest(currentCoordTime)
                            var nGridTime = Stage.Grids.CeilToNextNearestTimeGridTime(compareTime);
                            if (nGridTime is not { } gridTime)
                                return;
                            if (currentCoordTime >= gridTime) {
                                _noteIndicatorList.Add(out var newIndicator);
                                InitIndicator(newIndicator, gridTime);
                                LinkNotes(_noteIndicatorList[^2].NotePrototype, _noteIndicatorList[^1].NotePrototype);
                            }
                            else break;
                            compareTime = gridTime; // If possible, we should get a series of grid times between (prevCoordTime, currentCoordTime]
                                                    // rather than manually loop, too silly
                        }
                    }
                    else if (currentCoordTime < prevCoordTime) {
                        float compareTime = currentCoordTime;
                        while (true) {
                            var nGridTime = Stage.Grids.CeilToNextNearestTimeGridTime(compareTime);
                            if (nGridTime is not { } gridTime)
                                return;
                            if (prevCoordTime > gridTime) {
                                _noteIndicatorList.RemoveAt(^1);
                                _noteIndicatorList[^1].NotePrototype.NextLink = null;
                            }
                            else break;
                            compareTime = gridTime;
                        }
                    }
                }
                // Move downward
                else if (currentCoordTime < startCoordTime) {
                    if (prevCoordTime > startCoordTime) {
                        _noteIndicatorList.RemoveRange(1..);
                        prevCoordTime = startCoordTime;
                    }
                    if (currentCoordTime < prevCoordTime) {
                        float compareTime = prevCoordTime;
                        while (true) {
                            var nGridTime = Stage.Grids.FloorToNextNearestTimeGridTime(compareTime);
                            if (nGridTime is not { } gridTime)
                                return;
                            if (currentCoordTime <= gridTime) {
                                _noteIndicatorList.Insert(0, out var newIndicator);
                                InitIndicator(newIndicator, gridTime);
                                LinkNotes(_noteIndicatorList[0].NotePrototype, _noteIndicatorList[1].NotePrototype);
                            }
                            else break;
                            compareTime = gridTime;
                        }
                    }
                    else if (currentCoordTime > prevCoordTime) {
                        float compareTime = currentCoordTime;
                        while (true) {
                            var nGridTime = Stage.Grids.FloorToNextNearestTimeGridTime(compareTime);
                            if (nGridTime is not { } gridTime)
                                return;
                            if (prevCoordTime < gridTime) {
                                _noteIndicatorList.RemoveAt(0);
                                _noteIndicatorList[0].NotePrototype.PrevLink = null;
                            }
                            else break;
                            compareTime = gridTime;
                        }
                    }
                }

                _prevMouseCoord = mouseCoordPosition;

                void InitIndicator(NoteIndicatorController indicator, float gridTime)
                {
                    // TODO: Here will create a lot of NoteData instance, consider using object pool
                    NoteData cloneNote = _placeNoteTemplate.Clone();
                    NoteCoord coord = new(gridTime,
                        MathUtils.MapTo(prevCoordTime, currentCoordTime, gridTime, _prevMouseCoord.Position, mouseCoordPosition.Position));
                    coord = Stage.Grids.Quantize(coord, SnapToPositionGrid, false);
                    indicator.MoveTo(coord);
                    // Indicator.NotePrototype's coord is relative to _indicators[0]
                    cloneNote.PositionCoord = coord - _freezeMousePosition.Coord;
                    indicator.Initialize(cloneNote);
                }

                void LinkNotes(NoteData prev, NoteData next)
                {
                    prev.NextLink = next;
                    next.PrevLink = prev;
                }
            }
        }

        public void CancelPlaceNote()
        {
            _placeState = PlacementState.Idle;
            _placeNoteTemplate.IsSwipe = false;
            _placeNoteTemplate.Duration = 0f;
            _placeNoteTemplate.IsSlide = __placeOptions.HasFlag(PlacementOptions.PlaceSlide);
            _placeNoteTemplate.PrevLink = null;
            _placeNoteTemplate.NextLink = null;
            RefreshNoteIndicator();
        }

        public void RemoveSelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            // __selectedNotes.Sort(NoteTimeComparer.Instance);
            _operationHistory.Do(Stage.Chart.Notes.RemoveNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.ClearSelection();
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction((removedNotes) =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.SelectNotes(removedNotes.OfType<NoteModel>());
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                }));

            NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
        }

        public void AddNotesSnappingToCurve(int count, ReadOnlySpan<GridController.CurveApplyProperty> applyProperties = default)
        {
            if (Stage.Chart is null)
                return;
            var curveTime = Stage.Grids.CurveTime;
            if (curveTime is null)
                return;
            var (startTime, endTime) = curveTime.Value;

            var list = ListPool<NoteData>.Get();
            list.Capacity = Mathf.Max(count, list.Capacity);
            for (int i = 0; i < count; i++) {
                var time = startTime + (endTime - startTime) / (count + 1) * (i + 1);
                var coord = Stage.Grids.Quantize(new(time, 0f), true, false);
                list.Add(new NoteData { PositionCoord = coord, });
            }
            _operationHistory.Do(Stage.Chart.Notes.AddMultipleNotes(new NoteCoord(startTime, 0f), list.AsSpan())
                .WithRedoneAction(notes =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.ClearSelection();
                    _noteSelectionController.SelectNotes(notes.OfType<NoteModel>());
                    OnNotesChanged(true, true);
                })
                .WithUndoneAction(notes =>
                {
                    _noteSelectionController.DeselectNotes(notes.OfType<NoteModel>());
                    OnNotesChanged(true, true);
                }));

            ListPool<NoteData>.Release(list);
            // TODO: Apply applyproperties
        }

        /// <summary>
        /// Get which notes to display, does not update position here
        /// </summary>
        private void RefreshNoteIndicator()
        {
            if (!IsNoteIndicatorOn)
                return;

            if (_placeState is PlacementState.Pasting) {
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

        public void HideNoteIndicator()
        {
            _noteIndicatorParentTransform.gameObject.SetActive(false);
        }

        #region Copy Paste

        public void CopySelectedNotes()
        {
            if (_placeState is PlacementState.Placing or PlacementState.PlacingLinks)
                return;
            if (_placeState is PlacementState.Pasting)
                _placeState = PlacementState.Idle;

            _clipBoardNotes.Clear();
            if (SelectedNotes.IsEmpty)
                return;

            NoteData baseNote = SelectedNotes[0].Data;
            _clipBoardBasePosition = baseNote.Position;

            using var __sn_dict = DictionaryPool<NoteData, NoteData>.Get(out var slideNotes);
            foreach (var note in SelectedNotes) {
                _clipBoardNotes.Add(out var data);
                note.Data.CloneTo(data);
                data.PositionCoord -= baseNote.PositionCoord;

                data.PrevLink = data.NextLink = null;
                if (data.IsSlide) {
                    slideNotes.Add(note.Data, data);

                    NoteData? prevLinkNote = note.Data.PrevLink;
                    NoteData copiedPrev = default!;
                    while (prevLinkNote != null && !slideNotes.TryGetValue(prevLinkNote, out copiedPrev))
                        prevLinkNote = prevLinkNote.PrevLink;

                    // prevLink is null || copiedPrev is not null
                    if (prevLinkNote != null) {
                        Debug.Assert(copiedPrev is not null);
                        data.PrevLink = copiedPrev;
                        copiedPrev!.NextLink = data;
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

            // Block pasting when placing
            if (_placeState is PlacementState.Placing or PlacementState.PlacingLinks)
                return;
            _placeState = PlacementState.Pasting;
            RefreshNoteIndicator();
        }

        #endregion

        #region Notify

        public void NotifyCurveGeneratedWithSelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.Length < 2)
                return;

            NoteModel first = SelectedNotes[0];
            NoteModel last = SelectedNotes[^1];

            _operationHistory.Do(Stage.Chart.Notes.RemoveNotes(SelectedNotes[1..^1])
                .WithRedoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.ClearSelection();
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(removedNotes =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.SelectNote(first);
                    _noteSelectionController.SelectNotes(removedNotes.OfType<NoteModel>());
                    _noteSelectionController.SelectNote(last);
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                }));
            NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
        }

        #endregion

        private enum PlacementState
        {
            /// <summary>
            /// Normal state, the indicator is following the mouse
            /// </summary>
            Idle,
            /// <summary>
            /// Placing, freeze the indicator's position, and set duration or flick properties
            /// </summary>
            Placing,
            /// <summary>
            /// Drag mouse and placing links on grid.
            /// </summary>
            PlacingLinks,
            /// <summary>
            /// Pasting
            /// </summary>
            Pasting,
        }

        [Flags]
        public enum PlacementOptions
        {
            None = 0,
            PastingRememberPosition = 1 << 0,
            PlaceSlide = 1 << 1,
            PlaceSoundNoteByDefault = 1 << 2,
        }
    }
}