#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Models;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        /// <summary>
        /// The time from a note(speed==1) is activated to falls on the judgeline as Sudden+ is 0
        /// </summary>
        /// <remarks>
        /// We start to track note when note appears as if sudden+ is 0, and sets its
        /// visibility according to <see cref="StageNoteAppearAheadTime"/>
        /// </remarks>
        public float StageNoteActiveAheadTime
        {
            get {
                var args = Stage!.Args;
                var fallSpeed = ActualNoteFallSpeed;
                return args.NotePanelBaseLength / args.NoteTimeToZBaseMultiplierFunction.GetY(fallSpeed) / fallSpeed;
            }
        }

        public float GetStageNoteActiveAheadTime(float noteSpeed) => StageNoteActiveAheadTime / GetDisplayNoteSpeed(noteSpeed);
        public float GetStageNoteActiveTime(IStageNoteNode node) => node.Time - GetStageNoteActiveAheadTime(node.Speed);

        /// <summary>
        /// The time from a note(speed==1) appears to falls on the judgeline
        /// </summary>
        public float StageNoteAppearAheadTime => StageNoteActiveAheadTime * VisibleRangePercentage;
        public float GetStageNoteAppearAheadTime(float noteSpeed) => StageNoteAppearAheadTime / GetDisplayNoteSpeed(noteSpeed);
        public float GetStageNoteAppearTime(IStageNoteNode node) => node.Time - GetStageNoteAppearAheadTime(node.Speed);

        internal float GetDisplayNoteSpeed(float speed) => IsApplySpeedDifference ? speed : 1f;

        /// <summary>
        /// Get the time as if the note has speed == 1 and it falls on the current position in world
        /// <br/>
        /// The return value may be useless if note is not active on stage
        /// </summary>
        internal float GetNotePseudoTime(float time, float noteSpeed)
        {
            var currentTime = MusicPlayer.Time;
            return currentTime + (time - currentTime) * GetDisplayNoteSpeed(noteSpeed);
        }

        #region Converters

        public float ConvertWorldXToNoteCoordPosition(float x)
            => x / (Stage!.Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        public float ConvertWorldZToNoteCoordTime(float z, float noteSpeed = 1f)
            => ConvertWorldZToNoteCoordTimeBase(z) / ActualNoteFallSpeed / GetDisplayNoteSpeed(noteSpeed);

        public float ConvertNoteCoordPositionToWorldX(float position)
            => position * (Stage!.Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        public float ConvertNoteCoordTimeToWorldZ(float time, float noteSpeed = 1f)
            => ActualNoteFallSpeed * GetDisplayNoteSpeed(noteSpeed) * ConvertNoteCoordTimeToWorldZBase(time);

        public float ConvertNoteCoordTimeToHoldScaleY(float time, float noteSpeed = 1f)
            => ActualNoteFallSpeed * GetDisplayNoteSpeed(noteSpeed) * ConvertNoteCoordTimeToWorldZBase(time) / Stage!.Args.HoldSpritePrefab.Sprite.bounds.size.y;

        public (float X, float Z) ConvertNoteCoordToWorldPosition(NoteCoord coord, float noteSpeed = 1f)
            => (ConvertNoteCoordPositionToWorldX(coord.Position), ConvertNoteCoordTimeToWorldZ(coord.Time, noteSpeed));

        public bool TryConvertPerspectiveViewportPointToNoteCoord(Vector2 perspectiveViewPanelViewportPoint, float noteSpeed, out NoteCoord coord)
        {
            AssertStageLoaded();

            if (!IsInViewArea(perspectiveViewPanelViewportPoint)) {
                coord = default;
                return false;
            }

            var stage = Stage;
            var raycastViewportPoint = stage.ConvertPerspectiveViewportPointToRaycastingViewportPoint(perspectiveViewPanelViewportPoint);
            if (stage.TryConvertRaycastingViewportPointToNotePanelPosition(raycastViewportPoint, out var notePanelPosition)) {
                coord = new NoteCoord(
                    ConvertWorldXToNoteCoordPosition(notePanelPosition.X),
                    ConvertWorldZToNoteCoordTime(notePanelPosition.Z, noteSpeed) + MusicPlayer.Time);
                return true;
            }

            coord = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        internal bool TryRaycastPerspectiveViewportPointToNote(Vector2 perspectiveViewPanelViewportPoint, [MaybeNullWhen(false)] out GameStageNoteController note)
        {
            AssertStageLoaded();

            if (!IsInViewArea(perspectiveViewPanelViewportPoint)) {
                note = default;
                return false;
            }
            var stage = Stage;

            var raycastViewportPoint = stage.ConvertPerspectiveViewportPointToRaycastingViewportPoint(perspectiveViewPanelViewportPoint);
            if (stage.TryRaycastRaycastingViewportPointToNote(raycastViewportPoint, out note)) {
                return true;
            }
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        private float ConvertNoteCoordTimeToWorldZBase(float time)
            => time * Stage!.Args.NoteTimeToZBaseMultiplierFunction.GetY(ActualNoteFallSpeed);

        private float ConvertWorldZToNoteCoordTimeBase(float z)
            => z / Stage!.Args.NoteTimeToZBaseMultiplierFunction.GetY(ActualNoteFallSpeed);

        #endregion
    }
}