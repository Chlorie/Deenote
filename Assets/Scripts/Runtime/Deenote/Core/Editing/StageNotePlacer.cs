#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Library.Numerics;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Core.Editing
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
        private PooledObjectListView<NoteModel> _notePrototypes;

        // Placement
        //private NoteModel _placeNotePrototype = default!;
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
        private bool _snapToTimeGrid_bf;
        /// <summary>
        /// Access use <see cref="PlacingNoteSpeed"/>, use <see cref="GamePlayManager.HighlightedNoteSpeed"/> when null
        /// </summary>
        private float? _placingNoteSpeed;

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
                                Debug.Assert(_notePrototypes.Count == 1);
                                _notePrototypes[0].Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                                RefreshIndicators();
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
        }

        private bool IsPlacingSlide => Options.HasFlag(PlacementOptions.PlaceSlide);

        public bool IsIndicatorOn
        {
            get => _isIndicatorOn_bf;
            set {
                if (Utils.SetField(ref _isIndicatorOn_bf, value)) {
                    SetIndicatorsVisibility(value);
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
            get => _snapToTimeGrid_bf;
            set {
                if (Utils.SetField(ref _snapToTimeGrid_bf, value)) {
                    NotifyFlag(NotificationFlag.SnapToTimeGrid);
                }
            }
        }

        public float PlacingNoteSpeed
        {
            get => _placingNoteSpeed ?? _editor._game.HighlightedNoteSpeed;
        }

        private void SetPlacingNoteSpeed(float? value, bool forceUpdate = false)
        {
            if (Utils.SetField(ref _placingNoteSpeed, value)) {
                SetIndicatorsNoteSpeed();
                NotifyFlag(NotificationFlag.PlacingNoteSpeed);
            }
            else if (forceUpdate) {
                SetIndicatorsNoteSpeed();
                NotifyFlag(NotificationFlag.PlacingNoteSpeed);
            }

            void SetIndicatorsNoteSpeed()
            {
                if (_state is not PlacementState.Pasting) {
                    var speed = PlacingNoteSpeed;
                    if (!_indicators.IsNull) {
                        foreach (var indicator in _indicators) {
                            indicator.NotePrototype.Speed = speed;
                            indicator.Refresh();
                        }
                    }
                }
            }
        }

        internal StageNotePlacer(StageChartEditor editor)
        {
            _editor = editor;
            _notePrototypes = new PooledObjectListView<NoteModel>(
                new ObjectPool<NoteModel>(() => new NoteModel()));
            _notePrototypes.Add(out _);

            _editor._game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager => CancelPlaceNote());
            _editor._game.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.HighlightedNoteSpeed,
                manager => SetPlacingNoteSpeed(null, forceUpdate: true));
            _editor._game.StageLoaded += args =>
            {
                _indicatorPanelTransform = args.Stage.NoteIndicatorPanelTransform;
                _indicators.Clear();

                var indicators = new PooledObjectListView<PlacementNoteIndicatorController>(
                    UnityUtils.CreateObjectPool(args.Stage.Args.PlacementNoteIndicatorPrefab,
                        _indicatorPanelTransform,
                        item => item.OnInstantiate(this)));

                foreach (var note in _notePrototypes) {
                    indicators.Add(out var indicator);
                    indicator.Initialize(note);
                }

                _indicators = indicators;
            };
            _editor._game.MusicPlayer.TimeChanged += args =>
            {
                var delta = args.NewTime - args.OldTime;
                _placingNoteCoord.Time += delta;
                UpdateMovePlace(_placingNoteCoord, _placingMouseScreenPosition);
            };
        }

        public NoteModel ClonePlaceNotePrototype() => _notePrototypes[0].Clone();

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
                case PlacementState.Pasting:
                    return;
                case PlacementState.Idle or _:
                    break;
            }

            // When pasting, we needn't change the note type when mouse dragging
            if (_state is PlacementState.Pasting)
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

            switch (_state) {
                case PlacementState.Placing:
                    SetIndicatorsVisibility(true);
                    ChangePlacingNoteKind();
                    break;
                case PlacementState.PlacingLinks:
                    SetIndicatorsVisibility(true);
                    DragLink();
                    break;
                case PlacementState.Pasting when IsInPlacementArea(coord):
                    SetIndicatorsVisibility(true);
                    MoveCopied();
                    break;
                case PlacementState.Idle when IsInPlacementArea(coord):
                    SetIndicatorsVisibility(true);
                    MoveIndicator();
                    break;
                default:
                    SetIndicatorsVisibility(false);
                    break;
            }

            void ChangePlacingNoteKind()
            {
                Debug.Assert(_notePrototypes.Count == 1);
                Debug.Assert(_indicators.Count == 1);

                var note = _notePrototypes[0];
                var indicator = _indicators[0];
                Debug.Assert(indicator.NotePrototype == note);

                var delta = MathUtils.Abs(_placingMouseScreenPosition - _freezeMouseScreenPosition);
                // Draw swipe note when the angle between mouse movement direction
                // and horizontal line is within 30 degree
                delta.y *= SwipeDragAngleCotangent;

                // Swipe
                if (delta.x >= SwipeHorizontalDragDeltaThreshold && delta.x > delta.y) {
                    note.Kind = NoteModel.NoteKind.Swipe;
                    note.Duration = 0f;
                    indicator.Refresh();
                    // Indicator may be moved if create hold by dragging down
                    indicator.MoveTo(_freezeNoteCoord);
                }
                // Hold
                else if (delta.y >= HoldVerticalDragDeltaThreshold) {
                    note.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                    var dragEndCoordTime = _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid).Time;
                    if (dragEndCoordTime >= _freezeNoteCoord.Time) {
                        note.Duration = dragEndCoordTime - _freezeNoteCoord.Time;
                        indicator.Refresh();
                        indicator.MoveTo(_freezeNoteCoord);
                    }
                    else {
                        note.Duration = _freezeNoteCoord.Time - dragEndCoordTime;
                        indicator.Refresh();
                        indicator.MoveTo(_freezeNoteCoord with { Time = dragEndCoordTime });
                    }
                }
                // Click
                else {
                    note.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                    note.Duration = 0f;
                    indicator.Refresh();
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
                        _notePrototypes.RemoveRange(..^1);
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
                        _notePrototypes.RemoveRange(1..);
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
                    while (true) {
                        // Optimize: It could be more clear if we have FloorToNearest(currentCoordTime)
                        var nGridTime = _editor._game.Grids.CeilToNextNearestTimeGridTime(compareTime);
                        if (nGridTime is not { } gridTime)
                            return;
                        if (currentCoordTime >= gridTime) {
                            // TODO: _notePrototype修改
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
                    var note = indicator.NotePrototype;
                    _indicators[0].NotePrototype.CloneDataTo(note);
                    var cloneCoord = new NoteCoord(
                        MathUtils.MapTo(gridTime, prevCoordTime, currentCoordTime, _linkDragPrevCoord.Position, coord.Position),
                        gridTime);
                    cloneCoord = _editor._game.Grids.Quantize(cloneCoord, SnapToPositionGrid, false); // Time already snapped
                    // Indicator.NotePrototype's coord is relative to _indicators[0]
                    note.PositionCoord = cloneCoord;

                    indicator.Refresh();
                    indicator.MoveTo(cloneCoord);
                }

                void LinkNotes(NoteModel prev, NoteModel next)
                {
                    prev.InsertAsLinkBefore(next);
                }
            }

            void MoveCopied()
            {
                if (Options.HasFlag(PlacementOptions.PastingRememberPosition)) {
                    coord.Position = _editor.ClipBoard.BaseCoord.Position;
                    coord = _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid);
                }
                else {
                    coord = _editor._game.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                }

                Debug.Assert(_indicators.Count == _editor.ClipBoard.Notes.Length);

                MoveIndicatorsTo(coord);

                //var baseCoord = coord - _editor.ClipBoard.BaseCoord;
                //for (int i = 0; i < _indicators.Count; i++) {
                //    var indicator = _indicators[i];
                //    var note = _editor.ClipBoard.Notes[i];
                //    indicator.MoveTo(NoteCoord.ClampPosition(coord + note.PositionCoord));
                //}
            }

            void MoveIndicator()
            {
                Debug.Assert(_indicators.Count == 1);
                var moveCoord = _editor._game.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
                MoveIndicatorsTo(moveCoord);
            }
        }

        public void EndPlaceNote(NoteCoord coord)
        {
            if (_editor._game.CurrentChart is null)
                return;

            switch (_state) {
                case PlacementState.Placing:
                    PlaceNote();
                    break;
                case PlacementState.PlacingLinks:
                    PlaceLink();
                    break;
                case PlacementState.Pasting:
                    PasteNotes();
                    break;
                // Already cancelled
                case PlacementState.Idle or _:
                    return;
            }

            CancelPlaceNote(); // Reset states

            return;

            void PlaceNote()
            {
                Debug.Assert(_notePrototypes.Count == 1);
                Debug.Assert(_indicators.Count == 1);
                Debug.Assert(_indicators[0].NotePrototype == _notePrototypes[0]);

                var note = _notePrototypes[0];
                NoteCoord placeCoord;
                if (note.IsHold && coord.Time < _freezeNoteCoord.Time)
                    // If placing hold by dragging down, the actual time of hold is the end coord's time
                    placeCoord = _freezeNoteCoord with { Time = coord.Time };
                else
                    placeCoord = _freezeNoteCoord;
                // Optimize: indicator应该是一直在修改note 的位置，只要isindicatorOn，或许这句话多余
                //placeCoord = _editor._game.Grids.Quantize(placeCoord, SnapToPositionGrid, SnapToTimeGrid);
                _editor.AddNote(note, placeCoord);
            }

            void PlaceLink()
            {
                var placeCoord = _editor._game.Grids.Quantize(_freezeNoteCoord, SnapToPositionGrid, SnapToTimeGrid);

                _editor.AddMultipleNotes(_notePrototypes.AsSpan(), placeCoord);
            }

            void PasteNotes()
            {
                Debug.Assert(!_editor.ClipBoard.Notes.IsEmpty);

                NoteCoord placeCoord;
                if (Options.HasFlag(PlacementOptions.PastingRememberPosition)) {
                    placeCoord = coord with { Position = _editor.ClipBoard.BaseCoord.Position };
                    placeCoord = _editor._game.Grids.Quantize(placeCoord, false, true);
                }
                else {
                    placeCoord = _editor._game.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid);
                }

                _editor.AddMultipleNotes(_editor.ClipBoard.Notes, placeCoord);
            }
        }

        public void CancelPlaceNote()
        {
            _state = PlacementState.Idle;
            var note = _notePrototypes[0];
            note.UnlinkWithoutCutChain();
            note.Duration = 0f;
            note.Kind = IsPlacingSlide ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
            note.Speed = PlacingNoteSpeed;
            note.Sounds.Clear();
            _notePrototypes.SetCount(1);
            RefreshIndicators();
            SetPlacingNoteSpeed(null);
        }

        #endregion

        private void SetIndicatorsVisibility(bool visible)
        {
            if (_indicatorPanelTransform != null) {
                _indicatorPanelTransform.gameObject.SetActive(visible);
            }
        }

        public void HideIndicators()
        {
            _indicatorPanelTransform.gameObject.SetActive(false);
        }

        internal void PreparePasteClipBoard()
        {
            switch (_state) {
                // Block pasting when placing
                case PlacementState.Placing:
                case PlacementState.PlacingLinks:
                    return;
                case PlacementState.Pasting:
                case PlacementState.Idle or _:
                    break;
            }

            if (_editor.ClipBoard.Notes.Length == 0)
                return;

            _state = PlacementState.Pasting;
            using (var resetter = _notePrototypes.Resetting(_editor.ClipBoard.Notes.Length)) {
                var baseCoord = _editor.ClipBoard.BaseCoord;
                foreach (var cnote in _editor.ClipBoard.Notes) {
                    resetter.Add(out var note);
                    cnote.CloneDataTo(note, cloneSounds: true);
                }
            }
            RefreshIndicators();
            SetPlacingNoteSpeed(_indicators[0].NotePrototype.Speed);
        }

        private bool IsInPlacementArea(NoteCoord coord)
        {
            var game = _editor._game;
            game.AssertStageLoaded();

            if (coord.Position is > PlacementAreaMaxPosition or < -PlacementAreaMaxPosition)
                return false;
            if (coord.Time > game.MusicPlayer.Time + game.StageNoteAppearAheadTime)
                return false;

            return true;
        }

        private void MoveIndicatorsTo(NoteCoord coord)
        {
            Debug.Assert(_indicators.Count >= 1);

            if (_indicators.Count == 1) {
                _indicators[0].MoveTo(coord);
                return;
            }

            var baseCoord = coord - _indicators[0].NotePrototype.PositionCoord;
            foreach (var indicator in _indicators) {
                var c = baseCoord + indicator.NotePrototype.PositionCoord;
                c = NoteCoord.Clamp(c, _editor._game.MusicPlayer.ClipLength);
                indicator.MoveTo(c);
            }
        }

        private void RefreshIndicators()
        {
            if (_indicators.IsNull)
                return;

            using (var resetter = _indicators.Resetting(_notePrototypes.Count)) {
                foreach (var note in _notePrototypes) {
                    resetter.Add(out var indicator);
                    indicator.Initialize(note);
                }
            }
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
            /// Pasting
            /// </summary>
            Pasting,
        }

        public enum PlacementOptions
        {
            None = 0,
            PastingRememberPosition = 1 << 0,
            PlaceSlide = 1 << 1,
        }

        public enum NotificationFlag
        {
            IsIndicatorOn,
            SnapToPositionGrid,
            SnapToTimeGrid,

            PlacingNoteSpeed,
        }
    }
}