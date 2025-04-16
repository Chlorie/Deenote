#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Core.Editing
{
    public sealed partial class StageNotePlacer : FlagNotifiable<StageNotePlacer, StageNotePlacer.NotificationFlag>
    {
        private const float PlacementAreaMaxPosition = 6f;

        /// <summary>
        /// Press and drag mouse horizontal, the note will change to a swipe
        /// when drag horizontal delta value greater than this
        /// </summary>
        private const float SwipeHorizontalDragDeltaThreshold = 30f;
        /// <summary>
        /// If music time changed, after enter note-placing state, if music time delta is greater
        /// than the value, we never place a swipe note, and always place hold
        /// </summary>
        private const float SwipeVerticalDragDeltaTimeThreshold = 60f / 160f * 4;
        /// <summary>
        /// Press and drag mouse vertical, the note will change to a hold
        /// when drag vertical delta value is greater than this
        /// </summary>
        private const float HoldVerticalDragDeltaThreshold = 30f;
        /// <summary>
        /// If music time changed after enter note-placing state, if music time delta is greater
        /// than the value, we still placing a hold
        /// </summary>
        private const float HoldVerticalDragDeltaTimeThreshold = 60f / 160f / 4;
        /// <summary>
        /// If the cotangent of angle of drag direction and horizontal line is less than this
        /// The note is a swipe, otherwise, a hold
        /// </summary>
        private const float SwipeDragAngleCotangent = 1.7320508076f;

        internal StageChartEditor _editor;

        private NoteModel _metaPrototype;
        private PooledObjectListView<PlacementNoteIndicatorController> _indicators;
        private PooledObjectListView<NoteModel> _prototypes;

        public StageNotePlacer(StageChartEditor editor)
        {
            _editor = editor;
            _metaPrototype = new NoteModel();
            _prototypes = new PooledObjectListView<NoteModel>(
                new ObjectPool<NoteModel>(() => new NoteModel(),
                    note => _metaPrototype.CloneDataTo(note, true), defaultCapacity: 1));
            _prototypes.Add(out _);
            _indicators = null!;

            _editor._game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                _ => CancelPlaceNote());
            _editor._game.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.HighlightedNoteSpeed,
                _ => SetPlacingNoteSpeed(null, forceUpdateAndNotify: true));
            _editor._game.StageLoaded += args =>
            {
                _indicatorPanelTransform = args.Stage.NoteIndicatorPanelTransform;
                _indicators?.Clear();
                var indicators = new PooledObjectListView<PlacementNoteIndicatorController>(
                    UnityUtils.CreateObjectPool(args.Stage.Args.PlacementNoteIndicatorPrefab,
                        _indicatorPanelTransform,
                        item => item.OnInstantiate(this)));

                foreach (var note in _prototypes) {
                    indicators.Add(out var indicator);
                    indicator.Initialize(note);
                }
                _indicators = indicators;
            };
            _editor._game.MusicPlayer.TimeChanged += args =>
            {
                var delta = args.NewTime - args.OldTime;
                _updateNoteCoord.Time += delta;
                UpdatePlaceNote(_updateNoteCoord, _updateMousePosition);
            };
        }

        public NoteModel ClonePlaceNotePrototype() => _metaPrototype.Clone(cloneSounds: true);

        #region MoveIndicator

        private partial void UpdateMoveIndicator(NoteCoord coord, Vector2 mousePosition)
        {
            var moveCoord = _editor._game.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
            MoveIndicatorsTo(moveCoord);
        }

        #endregion

        #region PlaceSingleNote

        private Vector2 _freezeMousePosition;
        private NoteCoord _beginNoteCoord;

        private partial StateFlag BeginPlaceSingleNote(NoteCoord coord, Vector2 mousePosition)
        {
            _freezeMousePosition = mousePosition;
            _beginNoteCoord = _editor._game.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid);
            return StateFlag.PlacingSingleNote;
        }

        private partial void UpdatePlaceSingleNote(NoteCoord coord, Vector2 mousePosition)
        {
            if (!IsInPlacementTime(coord))
                return;

            var note = _prototypes[0];
            var indicator = _indicators[0];
            Debug.Assert(indicator.NotePrototype == note);

            var delta = MathUtils.Abs(mousePosition - _freezeMousePosition);
            // Draw swipe note when the angle between mouse movement direction
            // and horizontal line is within 30 degree
            delta.y *= SwipeDragAngleCotangent;

            // Drag horizontal
            if (delta.x >= SwipeHorizontalDragDeltaThreshold && delta.x > delta.y) {
                // When music time changed to much, place hold
                if (Mathf.Abs(_beginNoteCoord.Time - coord.Time) >= SwipeVerticalDragDeltaTimeThreshold)
                    goto PlaceHold;
                else
                    goto PlaceSwipe;
            }
            // Drag vertical
            else if (delta.y >= HoldVerticalDragDeltaThreshold) {
                goto PlaceHold;
            }
            // No drag, but music changed to much, place hold
            else if (Mathf.Abs(_beginNoteCoord.Time - coord.Time) >= HoldVerticalDragDeltaTimeThreshold) {
                goto PlaceHold;
            }
            // No drag
            else {
                goto PlaceClick;
            }

        PlaceSwipe:
            note.Kind = NoteModel.NoteKind.Swipe;
            note.Duration = 0f;
            indicator.Refresh();
            // Indicator may be moved if create hold by dragging down
            indicator.MoveTo(_beginNoteCoord);
            return;

        PlaceHold:
            note.Kind = _metaPrototype.Kind;
            var dragEndCoordTime = _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid).Time;
            if (dragEndCoordTime >= _beginNoteCoord.Time) {
                note.Duration = dragEndCoordTime - _beginNoteCoord.Time;
                indicator.Refresh();
                indicator.MoveTo(_beginNoteCoord);
            }
            else {
                note.Duration = _beginNoteCoord.Time - dragEndCoordTime;
                indicator.Refresh();
                indicator.MoveTo(_beginNoteCoord with { Time = dragEndCoordTime });
            }
            return;

        PlaceClick:
            note.Kind = _metaPrototype.Kind;
            note.Duration = 0f;
            indicator.Refresh();
            indicator.MoveTo(_beginNoteCoord);
            return;
        }

        private partial StateFlag EndPlaceSingleNote(NoteCoord coord, Vector2 mousePosition)
        {
            var note = _prototypes[0];
            Debug.Assert(_indicators[0].NotePrototype == note);
            NoteCoord placeCoord;
            if (note.IsHold && coord.Time < _beginNoteCoord.Time) {
                // If placing hold by dragging down, the actual time of hold is the end coord's time
                placeCoord = _beginNoteCoord;
                placeCoord.Time -= note.Duration;
            }
            else {
                placeCoord = _beginNoteCoord;
            }
            _editor.AddNote(note, placeCoord);

            ResetNotePrototypesToIdle();
            if (PlaceSlideModifier)
                return StateFlag.IdlePlacingSlides;
            else
                return StateFlag.Idle;
        }

        #endregion

        #region Drag Place Slides

        private NoteCoord _dragPrevCoord;
        private bool _dragSlideMoveUp;

        private partial StateFlag BeginDragPlaceSlides(NoteCoord coord, Vector2 mousePosition)
        {
            _dragPrevCoord = coord;
            _beginNoteCoord = _editor._game.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid);

            return StateFlag.PlacingSlides;
        }

        private partial void UpdateDragPlaceSlides(NoteCoord coord, Vector2 mousePosition)
        {
            if (!IsInPlacementTime(coord))
                return;

            var startCoord = _beginNoteCoord;
            var prevCoord = _dragPrevCoord;
            // When mouse drag over a time grid, generate an indicator at the position
            if (coord.Time >= startCoord.Time) {
                _dragSlideMoveUp = true;
                if (prevCoord.Time < startCoord.Time) {
                    // If mouse is below the base note at prev frame, remove all extra notes
                    _prototypes.RemoveRange(1..);
                    _indicators.RemoveRange(1..);
                    prevCoord = startCoord;
                }
                if (coord.Time > prevCoord.Time)
                    AtUpMoveUp();
                else if (coord.Time < prevCoord.Time)
                    AtUpMoveDown();
            }
            else {
                _dragSlideMoveUp = false;
                if (prevCoord.Time > startCoord.Time) {
                    _prototypes.RemoveRange(1..);
                    _indicators.RemoveRange(1..);
                    prevCoord.Time = startCoord.Time;
                }
                if (coord.Time < prevCoord.Time)
                    AtDownMoveDown();
                else if (coord.Time > prevCoord.Time)
                    AtDownMoveUp();
            }

            _dragPrevCoord = coord;

            void AtUpMoveUp()
            {
                var ceil = _editor._game.Grids.CeilToNearestNextTimeGridTime(coord.Time);
                float compareTime = prevCoord.Time;
                while (true) {
                    var nGrid = _editor._game.Grids.CeilToNearestNextTimeGridTime(compareTime);
                    if (nGrid is not { } gridTime)
                        return;
                    Debug.Assert(ceil is not null);
                    if (gridTime >= ceil!.Value) {
                        Debug.Assert(gridTime == ceil.Value);
                        return;
                    }

                    _prototypes.Add(out var note);
                    note.InsertAsLinkAfter(_prototypes[^2]);
                    _indicators.Add(out var indicator);
                    InitIndicator(indicator, note, gridTime);
                    _indicators[^2].Refresh();

                    compareTime = gridTime;
                }
            }
            void AtUpMoveDown()
            {
                var ceil = _editor._game.Grids.CeilToNearestNextTimeGridTime(prevCoord.Time);
                var compareTime = coord.Time;
                while (true) {
                    var nGrid = _editor._game.Grids.CeilToNearestNextTimeGridTime(compareTime);
                    if (nGrid is not { } gridTime)
                        return;
                    Debug.Assert(ceil is not null);
                    if (gridTime >= ceil!.Value) {
                        Debug.Assert(gridTime == ceil.Value);
                        return;
                    }

                    _prototypes[^1].UnlinkWithoutCutChain();
                    _prototypes.RemoveAt(^1);
                    _indicators.RemoveAt(^1);
                    if (_indicators.Count >= 1)
                        _indicators[^1].Refresh(); // Update link line

                    compareTime = gridTime;
                }
            }
            void AtDownMoveUp()
            {
                var floor = _editor._game.Grids.FloorToNearestNextTimeGridTime(prevCoord.Time);
                // When at down, the order in _prototypes is reversed
                var compareTime = coord.Time;
                while (true) {
                    var nGrid = _editor._game.Grids.FloorToNearestNextTimeGridTime(compareTime);
                    if (nGrid is not { } gridTime)
                        return;
                    Debug.Assert(floor is not null);
                    if (gridTime <= floor!.Value) {
                        Debug.Assert(gridTime == floor.Value);
                        return;
                    }

                    _prototypes[^1].UnlinkWithoutCutChain();
                    _prototypes.RemoveAt(^1);
                    _indicators.RemoveAt(^1);

                    compareTime = gridTime;
                }
            }
            void AtDownMoveDown()
            {
                var floor = _editor._game.Grids.FloorToNearestNextTimeGridTime(coord.Time);
                float compareTime = prevCoord.Time;
                while (true) {
                    var nGrid = _editor._game.Grids.FloorToNearestNextTimeGridTime(compareTime);
                    if (nGrid is not { } gridTime)
                        return;
                    Debug.Assert(floor is not null);
                    if (gridTime <= floor!.Value) {
                        Debug.Assert(gridTime == floor.Value);
                        return;
                    }

                    _prototypes.Add(out var note);
                    note.InsertAsLinkBefore(_prototypes[^2]);
                    _indicators.Add(out var indicator);
                    InitIndicator(indicator, note, gridTime);

                    compareTime = gridTime;
                }
            }

            void InitIndicator(PlacementNoteIndicatorController indicator, NoteModel prototype, float gridTime)
            {
                var cloneCoord = new NoteCoord(
                    MathUtils.MapTo(gridTime, prevCoord.Time, coord.Time, prevCoord.Position, coord.Position),
                    gridTime);
                cloneCoord = _editor._game.Grids.Quantize(cloneCoord, SnapToPositionGrid, false); // Time already quantized
                prototype.PositionCoord = cloneCoord - _beginNoteCoord;

                indicator.Initialize(prototype);
                indicator.MoveTo(cloneCoord);
            }
        }

        private partial StateFlag EndDragPlaceSlides(NoteCoord coord, Vector2 mousePosition)
        {
            var placeCoord = _beginNoteCoord;

            _editor.AddMultipleNotes(_prototypes.AsSpan(), placeCoord);

            ResetNotePrototypesToIdle();
            if (PlaceSlideModifier)
                return StateFlag.IdlePlacingSlides;
            else
                return StateFlag.Idle;
        }

        #endregion

        #region Paste Notes

        private partial StateFlag BeginPasteNotes(NoteCoord coord, Vector2 mousePosition)
        {
            return StateFlag.PlacingPastedNotes;
        }

        private partial void UpdatePasteNotes(NoteCoord coord, Vector2 mousePosition)
        {
            Debug.Assert(_indicators.Count == _editor.ClipBoard.Notes.Length);

            if (PasteRememberPositionModifier) {
                coord.Position = _editor.ClipBoard.BaseCoord.Position;
                coord = _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid);
            }
            else {
                coord = _editor._game.Grids.Quantize(NoteCoord.ClampPosition(coord), SnapToPositionGrid, SnapToTimeGrid);
            }

            MoveIndicatorsTo(coord);
        }

        private partial StateFlag EndPasteNotes(NoteCoord coord, Vector2 mousePosition)
        {
            Debug.Assert(!_editor.ClipBoard.Notes.IsEmpty);

            NoteCoord placeCoord;
            if (PasteRememberPositionModifier) {
                placeCoord = coord with { Position = _editor.ClipBoard.BaseCoord.Position };
                placeCoord = _editor._game.Grids.Quantize(placeCoord, false, SnapToPositionGrid);
            }
            else {
                placeCoord = _editor._game.Grids.Quantize(coord, SnapToPositionGrid, SnapToTimeGrid);
            }
            _editor.AddMultipleNotes(_editor.ClipBoard.Notes, placeCoord);

            //ResetNotePrototypesToIdle(); Do this in StateMachine, as in current state indicator property edit are not allowed
            if (PlaceSlideModifier)
                return StateFlag.IdlePlacingSlides;
            else
                return StateFlag.Idle;
        }

        #endregion

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
            if (_indicators is null)
                return;

            using (var resetter = _indicators.Resetting(_prototypes.Count)) {
                foreach (var note in _prototypes) {
                    resetter.Add(out var indicator);
                    indicator.Initialize(note);
                }
            }
        }

        private bool IsInPlacementArea(NoteCoord coord)
        {
            var game = _editor._game;
            game.AssertStageLoaded();

            if (coord.Position is > PlacementAreaMaxPosition or < -PlacementAreaMaxPosition)
                return false;
            if (!IsInPlacementTime(coord))
                return false;

            return true;
        }

        private bool IsInPlacementTime(NoteCoord coord)
        {
            var game = _editor._game;
            game.AssertStageLoaded();
            return coord.Time <= game.MusicPlayer.Time + game.StageNoteAppearAheadTime;
        }

        public enum NotificationFlag
        {
            PlaceSoundNoteByDefault,
            IsIndicatorOn,
            SnapToPositionGrid,
            SnapToTimeGrid,

            PlacingNoteSpeed,
        }
    }
}