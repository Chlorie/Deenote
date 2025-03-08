#nullable enable

using Deenote.Entities;
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
        public float StageNoteActiveAheadTime => Stage!.Args.NotePanelBaseLengthTime / ActualNoteFallSpeed;
        public float GetStageNoteActiveAheadTime(float noteSpeed) => StageNoteActiveAheadTime / GetDisplayNoteSpeed(noteSpeed);
        public float GetStageNoteActiveTime(IStageNoteNode node) => node.Time - GetStageNoteActiveAheadTime(node.Speed);

        /// <summary>
        /// The time from a note(speed==1) appears to falls on the judgeline
        /// </summary>
        public float StageNoteAppearAheadTime => StageNoteActiveAheadTime * VisibleRangePercentage;
        public float GetStageNoteAppearAheadTime(float noteSpeed) => StageNoteAppearAheadTime / GetDisplayNoteSpeed(noteSpeed);
        public float GetStageNoteAppearTime(IStageNoteNode node) => node.Time - GetStageNoteAppearAheadTime(node.Speed);

        internal float GetDisplayNoteSpeed(float speed) => IsApplySpeedDifference ? speed : 1f;

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

            if (Stage.TryConvertPerspectiveViewPointToNotePanelPosition(perspectiveViewPanelViewportPoint, out var notePanelPosition)) {
                coord = new NoteCoord(
                    ConvertWorldXToNoteCoordPosition(notePanelPosition.X),
                    ConvertWorldZToNoteCoordTime(notePanelPosition.Z, noteSpeed) + MusicPlayer.Time);
                return true;
            }

            coord = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: >= 0f and <= 1f };
        }

        private float ConvertNoteCoordTimeToWorldZBase(float time)
            => time * Stage!.Args.NoteTimeToZBaseMultiplier;

        private float ConvertWorldZToNoteCoordTimeBase(float z)
            => z / Stage!.Args.NoteTimeToZBaseMultiplier;

        #endregion
    }
}