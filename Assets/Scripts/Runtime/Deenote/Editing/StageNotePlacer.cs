#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GamePlay;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using System;
using UnityEngine;

namespace Deenote.Editing
{
    public sealed class StageNotePlacer : FlagNotifiable<StageNotePlacer, StageNotePlacer.NotificationFlag>
    {
        private const float PlacementAreaMaxPosition = 4f;

        /// <summary>
        /// Press and drag mouse horizontal, the note will change to a swipe
        /// when drag horizontal delta value greater than this
        /// </summary>
        private const float SwipeHorizontalDragDeltaThreshold = 30f;
        /// <summary>
        /// Press and drag mouse vertical, the note will change to a hold
        /// when drag vertical delta value is greater than this
        /// </summary>
        private const float HoldVerticalDragDeltaThreshold = 30f;
        /// <summary>
        /// If the cotangent of angle of drag direction and horizontal line is less than this
        /// The note is a swipe, otherwise, a hold
        /// </summary>
        private const float SwipeDragAngleCotangent = 1.7320508076f;

        private Transform _indicatorPanelTransform = default!;
        private PooledObjectListView<PlacementNoteIndicatorController> _indicators;

        // Placement
        private NoteModel _placeNotePrototype = default!;
        private Vector2 _freezeMouseScreenPosition;
        private Vector2 _placingMouseScreenPosition;
        private NoteCoord _freezeNoteCoord;
        private NoteCoord _placingNoteCoord;
        private NoteCoord _linkDragPrevCoord;

        internal StageChartEditor _editor = default!;

        private PlacementState _state;

        private PlacementOptions _options_bf;
        private bool _isIndicatorOn_bf;
        private bool _snapToPositionGrid_bf;
        private bool _snapToTimeGrid;

        public bool IsPlacing => _state is PlacementState.Placing or PlacementState.PlacingLinks;

        public PlacementOptions Options
        {
            get => _options_bf;
            set {
                if (Utils.SetField(ref _options_bf, value, out var old)) {
                    var diff = old ^ value;

                    if (diff.HasFlag(PlacementOptions.PlaceSlide)) {
                        switch (_state) {
                            case PlacementState.Idle or PlacementState.Placing:
                                Debug.Assert(_indicators.Count == 1);
                                _placeNotePrototype.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                                _indicators[0].Initialize(_placeNotePrototype);
                                break;
                            case PlacementState.PlacingLinks:
                                break;
                            case PlacementState.PlacingMultiple:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private bool IsPlacingSlide => Options.HasFlag(PlacementOptions.PlaceSlide);

        public bool IsIndicatorOn
        {
            get => _isIndicatorOn_bf;
            set {
                if (Utils.SetField(ref _isIndicatorOn_bf, value)) {
                    if (value)
                        RefreshIndicators();
                    else
                        _indicators.Clear();
                    NotifyFlag(NotificationFlag.IsIndicatorOn);
                }
            }
        }

        public bool SnapToPositionGrid
        {
            get => _snapToPositionGrid_bf;
            set {
                if (Utils.SetField(ref _snapToPositionGrid_bf, value)) {
                    NotifyFlag(NotificationFlag.SnapToPositionGrid);
                }
            }
        }

        public bool SnapToTimeGrid
        {
            get => _snapToTimeGrid;
            set {
                if (Utils.SetField(ref _snapToTimeGrid, value)) {
                    NotifyFlag(NotificationFlag.SnapToTimeGrid);
                }
            }
        }

        internal StageNotePlacer(StageChartEditor editor)
        {
            _placeNotePrototype = new NoteModel();
            _editor = editor;

            _editor._game.RegisterNotification(
                GamePlayManager.NotificationFlag.IsShowLinkLines,
                manager =>
                {
                    bool show = manager.IsShowLinkLines;
                    foreach (var note in _indicators)
                        note.UpdateLinkLineVisibility(show);
                });
            _editor._game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager =>
                {
                    CancelPlaceNote();
                });
            _editor._game.RegisterNotification(
                GamePlayManager.NotificationFlag.GameStageLoaded,
                manager =>
                {
                    manager.AssertStageLoaded();

                    _indicatorPanelTransform = manager.Stage.NoteIndicatorPanelTransform;
                    _indicators = new(UnityUtils.CreateObjectPool(manager.Stage.Args.PlacementNoteIndicatorPrefab,
                        _indicatorPanelTransform,
                        item =>
                        {
                            item.OnInstantiate(this);
                            item.UpdateLinkLineVisibility(manager.IsShowLinkLines);
                        }));
                    RefreshIndicators();
                });
            _editor._game.MusicPlayer.TimeChanged += args =>
            {
                var delta = args.NewTime - args.OldTime;
                _placingNoteCoord.Time += delta;
                UpdateMovePlace(_placingNoteCoord, _placingMouseScreenPosition);
            };
        }

        internal void Unity_Start()
        {
            // TODO: Temp
            SnapToPositionGrid = SnapToTimeGrid = true;
            IsIndicatorOn = true;
        }

        public NoteModel ClonePlaceNotePrototype() => _placeNotePrototype.Clone();

        #region Mouse Place

        public void BeginPlaceNote(NoteCoord coord, Vector2 mouseScreenPosition)
        {
            if (!IsInPlacementArea(coord))
                return;

            switch (_state) {
                case PlacementState.Placing:
                case PlacementState.PlacingLinks:
                    CancelPlaceNote();
                    break;
                case PlacementState.PlacingMultiple:
                    return;
                case PlacementState.Idle or _:
                    break;
            }

            // When pasting, we needn't change the note type when mouse dragging
            if (_state is PlacementState.PlacingMultiple)
                return;

            _freezeMouseScreenPosition = mouseScreenPosition;
            _placingMouseScreenPosition = mouseScreenPosition;
            _freezeNoteCoord = _editor._game.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid);
            _placingNoteCoord = _freezeNoteCoord;

            if (Options.HasFlag(PlacementOptions.PlaceSlide)) {
                _state = PlacementState.PlacingLinks;
                _linkDragPrevCoord = _freezeNoteCoord;
            }
            else {
                _state = PlacementState.Placing;
            }
        }

        public void UpdateMovePlace(NoteCoord coord, Vector2 mouseScreenPosition)
        {
            _placingMouseScreenPosition = mouseScreenPosition;
            _placingNoteCoord = coord;

            if (!IsIndicatorOn)
                return;

            _indicatorPanelTransform.gameObject.SetActive(true);
            switch (_state) {
                case PlacementState.Placing:
                    ChangePlacingNoteKind();
                    break;
                case PlacementState.PlacingLinks:
                    DragLink();
                    break;
                case PlacementState.PlacingMultiple when IsInPlacementArea(coord):
                    MoveCopied();
                    break;
                case PlacementState.Idle when IsInPlacementArea(coord):
                    MoveIndicator();
                    break;
                default:
                    HideIndicators();
                    break;
            }

            void ChangePlacingNoteKind()
            {
                Debug.Assert(_indicators.Count == 1);
                var delta = MathUtils.Abs(_placingMouseScreenPosition - _freezeMouseScreenPosition);
                // Draw swipe note when the angle between mouse movement direction
                // and horizontal line is within 30 degree
                delta.y *= SwipeDragAngleCotangent;

                // Swipe
                if (delta.x >= SwipeHorizontalDragDeltaThreshold && delta.x > delta.y) {
                    _placeNotePrototype.Kind = NoteModel.NoteKind.Swipe;
                    _placeNotePrototype.Duration = 0f;
                    var indicator = _indicators[0];
                    indicator.Initialize(_placeNotePrototype);
                    // Indicator may be moved if create hold by dragging down
                    indicator.MoveTo(_freezeNoteCoord);
                }
                // Hold
                else if (delta.y >= HoldVerticalDragDeltaThreshold) {
                    _placeNotePrototype.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                    var dragEndCoordTime = _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid).Time;
                    if (dragEndCoordTime >= _freezeNoteCoord.Time) {
                        _placeNotePrototype.Duration = dragEndCoordTime - _freezeNoteCoord.Time;
                        var indicator = _indicators[0];
                        indicator.Initialize(_placeNotePrototype);
                        indicator.MoveTo(_freezeNoteCoord);
                    }
                    else {
                        _placeNotePrototype.Duration = _freezeNoteCoord.Time - dragEndCoordTime;
                        var indicator = _indicators[0];
                        indicator.Initialize(_placeNotePrototype);
                        indicator.MoveTo(_freezeNoteCoord with { Time = dragEndCoordTime });
                    }
                }
                // Click
                else {
                    _placeNotePrototype.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                    _placeNotePrototype.Duration = 0f;
                    var indicator = _indicators[0];
                    indicator.Initialize(_placeNotePrototype);
                    indicator.MoveTo(_freezeNoteCoord);
                }
            }

            void DragLink()
            {
                var startCoordTime = _freezeNoteCoord.Time;
                var currentCoordTime = coord.Time;
                var prevCoordTime = _linkDragPrevCoord.Time;
                // When mouse drag over a time grid, generate an indicator at the position
                if (currentCoordTime > startCoordTime) {
                    if (prevCoordTime < startCoordTime) {
                        // If mouse is below the base note at prev frame, remove all extra notes
                        _indicators.RemoveRange(..^1);
                        prevCoordTime = startCoordTime;
                    }
                    if (currentCoordTime > prevCoordTime)
                        AtUpMoveUp();
                    else if (currentCoordTime < prevCoordTime)
                        AtUpMoveDown();
                }
                else if (currentCoordTime < startCoordTime) {
                    if (prevCoordTime > startCoordTime) {
                        _indicators.RemoveRange(1..);
                        prevCoordTime = startCoordTime;
                    }
                    if (currentCoordTime < prevCoordTime)
                        AtDownMoveDown();
                    else if (currentCoordTime > prevCoordTime)
                        AtDownMoveUp();
                }


                void AtUpMoveUp()
                {
                    float compareTime = prevCoordTime;
                    // Optimize
                    while (true) {
                        //                          TODO: It could be more clear if we have FloorToNearest(currentCoordTime)
                        var nGridTime = _editor._game.Grids.CeilToNextNearestTimeGridTime(compareTime);
                        if (nGridTime is not { } gridTime)
                            return;
                        if (currentCoordTime >= gridTime) {
                            _indicators.Add(out var newIndicator);
                            InitIndicator(newIndicator, gridTime);
                            LinkNotes(_indicators[^1].NotePrototype, _indicators[^1].NotePrototype);
                        }
                        else
                            return;

                        compareTime = gridTime;
                        // If possible, we should get a sequence of grid times between (prevCoordTime, currentCoordTime]
                        // rather than manually loop, too silly
                    }
                }

                void AtUpMoveDown()
                {
                    var compareTime = currentCoordTime;
                    while (true) {
                        var nGridTime = _editor._game.Grids.CeilToNextNearestTimeGridTime(compareTime);
                        if (nGridTime is not { } gridTime)
                            return;
                        if (prevCoordTime >= gridTime) {
                            _indicators[^1].NotePrototype.UnlinkWithoutCutChain();
                            _indicators.RemoveAt(^1);
                        }
                        else
                            return;
                        compareTime = gridTime;
                    }
                }

                void AtDownMoveDown()
                {
                    float compareTime = prevCoordTime;
                    while (true) {
                        var nGridTime = _editor._game.Grids.FloorToNextNearestTimeGridTime(compareTime);
                        if (nGridTime is not { } gridTime)
                            return;
                        if (currentCoordTime <= gridTime) {
                            _indicators.Insert(0, out var newIndicator);
                            InitIndicator(newIndicator, gridTime);
                            LinkNotes(_indicators[0].NotePrototype, _indicators[1].NotePrototype);
                        }
                        else return;
                        compareTime = gridTime;
                    }
                }

                void AtDownMoveUp()
                {
                    float compareTime = currentCoordTime;
                    while (true) {
                        var nGridTime = _editor._game.Grids.FloorToNextNearestTimeGridTime(compareTime);
                        if (nGridTime is not { } gridTime)
                            return;
                        if (prevCoordTime < gridTime) {
                            _indicators[0].NotePrototype.UnlinkWithoutCutChain();
                            _indicators.RemoveAt(0);
                        }
                        else break;
                        compareTime = gridTime;
                    }
                }

                void InitIndicator(PlacementNoteIndicatorController indicator, float gridTime)
                {
                    // Optimize, Here creates a lot ot NoteModel instance, considering use a pool
                    var cloneNotePrototype = _placeNotePrototype.Clone();
                    var cloneCoord = new NoteCoord(
                        MathUtils.MapTo(gridTime, prevCoordTime, currentCoordTime, _linkDragPrevCoord.Position, coord.Position),
                        gridTime);
                    cloneCoord = _editor._game.Grids.Quantize(cloneCoord, SnapToPositionGrid, false); // Time already snapped
                    indicator.MoveTo(cloneCoord);
                    // Indicator.NotePrototype's coord is relative to _indicators[0]
                    cloneNotePrototype.PositionCoord = cloneCoord;
                    indicator.Initialize(cloneNotePrototype);
                }

                void LinkNotes(NoteModel prev, NoteModel next)
                {
                    prev.InsertAsLinkBefore(next);
                }
            }

            void MoveCopied()
            {
                if (Options.HasFlag(PlacementOptions.PastingRememberPosition)) {
                    coord.Position = _editor.ClipBoardBaseCoord.Position;
                    coord = _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid);
                }
                else {
                    coord = _editor._game.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                }

                Debug.Assert(_indicators.Count == _editor.ClipBoardNotes.Length);
                var baseCoord = coord - _editor.ClipBoardBaseCoord;
                for (int i = 0; i < _indicators.Count; i++) {
                    var indicator = _indicators[i];
                    var note = _editor.ClipBoardNotes[i];
                    indicator.MoveTo(NoteCoord.ClampPosition(baseCoord + note.PositionCoord));
                }
            }

            void MoveIndicator()
            {
                Debug.Assert(_indicators.Count == 1);
                var moveCoord = _editor._game.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                _indicators[0].MoveTo(moveCoord);
            }
        }

        public void EndPlaceNote(NoteCoord coord)
        {
            if (_editor._game.CurrentChart is null)
                return;

            switch (_state) {
                case PlacementState.Placing: PlaceNote(); break;
                case PlacementState.PlacingLinks: PlaceLink(); break;
                case PlacementState.PlacingMultiple: PasteNotes(); break;
                // Already cancelled
                case PlacementState.Idle or _: return;
            }

            CancelPlaceNote(); // Reset states

            return;

            void PlaceNote()
            {
                Debug.Assert(_indicators.Count == 1);
                Debug.Assert(_indicators[0].NotePrototype == _placeNotePrototype);

                NoteCoord placeCoord;
                if (_placeNotePrototype.IsHold && coord.Time < _freezeNoteCoord.Time)
                    // If placing hold by dragging down, the actual time of hold is the end coord's time
                    placeCoord = _freezeNoteCoord with { Time = coord.Time };
                else
                    placeCoord = _freezeNoteCoord;
                // TODO: indicator应该是一直在修改note 的位置，只要isindicatorOn，或许这句话多余
                placeCoord = _editor._game.Grids.Quantize(placeCoord, SnapToPositionGrid, SnapToTimeGrid);
                _editor.AddNote(placeCoord, _placeNotePrototype);
            }

            void PlaceLink()
            {
                var placeCoord = _editor._game.Grids.Quantize(_freezeNoteCoord, SnapToPositionGrid, SnapToTimeGrid);
                // TODO:如果indicators关掉似乎就不对了
                // 不过后续打算改成下键的时候indicator还是会开启看情况选择吧
                using var so_notes = SpanOwner<NoteModel>.Allocate(_indicators.Count);
                var notes = so_notes.Span;
                for (int i = 0; i < notes.Length; i++) {
                    notes[i] = _indicators[i].NotePrototype;
                }
                _editor.AddMultipleNotes(placeCoord, notes);
            }

            void PasteNotes()
            {
                Debug.Assert(!_editor.ClipBoardNotes.IsEmpty);

                NoteCoord placeCoord;
                if (Options.HasFlag(PlacementOptions.PastingRememberPosition)) {
                    placeCoord = coord with { Position = _editor.ClipBoardBaseCoord.Position };
                    placeCoord = _editor._game.Grids.Quantize(placeCoord, false, true);
                }
                else {
                    placeCoord = _editor._game.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid);
                }

                _editor.AddMultipleNotes(placeCoord, _editor.ClipBoardNotes);
            }
        }

        public void CancelPlaceNote()
        {
            _state = PlacementState.Idle;
            _placeNotePrototype.UnlinkWithoutCutChain();
            _placeNotePrototype.Duration = 0f;
            _placeNotePrototype.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
            RefreshIndicators();
        }

        #endregion

        public void HideIndicators()
        {
            _indicatorPanelTransform.gameObject.SetActive(false);
        }

        /// <summary>
        /// Get which notes to display, does not update position here
        /// </summary>
        private void RefreshIndicators()
        {
            if (!IsIndicatorOn)
                return;

            if (_editor._game.Stage is null)
                return;

            if (_state is PlacementState.PlacingMultiple) {
                using var indicators = _indicators.Resetting();
                foreach (var note in _editor.ClipBoardNotes) {
                    indicators.Add(out var indicator);
                    indicator.Initialize(note);
                }
            }
            else {
                _indicators.SetCount(1);
                _indicators[0].Initialize(_placeNotePrototype);
            }
        }

        internal void PreparePasteClipBoard()
        {
            switch (_state) {
                // Block pasting when placing
                case PlacementState.Placing:
                case PlacementState.PlacingLinks:
                    return;
                case PlacementState.PlacingMultiple:
                    break;
                case PlacementState.Idle or _:
                    break;
            }

            _state = PlacementState.PlacingMultiple;
            RefreshIndicators();
        }

        private bool IsInPlacementArea(NoteCoord coord)
        {
            var game = _editor._game;
            game.AssertStageLoaded();

            if (coord.Position is > PlacementAreaMaxPosition or < -PlacementAreaMaxPosition)
                return false;
            if (coord.Time > game.MusicPlayer.Time + game.Stage.NoteAppearAheadTime)
                return false;

            return true;
        }

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
            /// Pasting, or maybe some other situations
            /// </summary>
            PlacingMultiple,
        }

        public enum PlacementOptions
        {
            None = 0,
            PastingRememberPosition = 1 << 0,
            PlaceSlide = 1 << 1,
            PlaceSoundNoteByDefault = 1 << 2,
        }

        public enum NotificationFlag
        {
            IsIndicatorOn,
            SnapToPositionGrid,
            SnapToTimeGrid,
        }
    }
}