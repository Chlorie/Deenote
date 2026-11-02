#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Mathematics;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    /// <summary>
    /// Lightweight stage view for a speed change warning. It only displays a selectable
    /// note body and a time-grid-like line; it has no hold or hit effects.
    /// </summary>
    internal abstract class GameStageSpeedChangeWarningNoteController : MonoBehaviour
    {
        /// <summary>
        /// Position of the warning body, in note-coordinate space.
        /// </summary>
        internal const float SpritePosition = -3f;

        [SerializeField] private SpeedChangeWarningLineRenderer _lineRenderer = default!;
        [SerializeField] private Collider _selectionCollider = default!;

        protected GamePlayManager _game = default!;

        private float _stageDeltaTime;
        private bool _isActive;

        public SpeedChangeWarningModel Model { get; private set; } = default!;

        /// <summary>
        /// Time before the warning is hit at which the warning starts to appear.
        /// Warnings always move in the lane of <see cref="GamePlayManager.HighlightedNoteSpeed"/>.
        /// </summary>
        private float AppearAheadTime
        {
            get {
                _game.AssertStageLoaded();
                return _game.GetStageNoteAppearAheadTime(_game.HighlightedNoteSpeed);
            }
        }

        internal void OnInstantiate(GamePlayManager game)
        {
            _game = game;
            _game.AssertStageLoaded();

            _lineRenderer ??= GetComponentInChildren<SpeedChangeWarningLineRenderer>(true);
            _selectionCollider ??= GetComponentInChildren<Collider>(true);
            _lineRenderer?.OnInstantiate(game);
        }

        internal void Initialize(SpeedChangeWarningModel model)
        {
            Model = model;
            RefreshVisual();
            RefreshStageDeltaTime();
        }

        private void OnDisable()
        {
            _isActive = false;
            _lineRenderer?.SetVisible(false);
        }

        internal void RefreshStageDeltaTime()
        {
            if (Model is null)
                return;

            _stageDeltaTime = Model.Time - _game.MusicPlayer.Time;
            bool active = _stageDeltaTime >= 0f;
            if (active != _isActive) {
                _isActive = active;
                OnStateChanged(active);
            }

            if (active) {
                transform.WithLocalPositionZ(
                    _game.ConvertNoteCoordTimeToWorldZ(_stageDeltaTime, _game.HighlightedNoteSpeed));

                bool timeGridVisible = _game.WarningNoteVisibility.HasFlag(GamePlayManager.SpeedWarningNoteVisibility.ShowLine);
                if (_selectionCollider is not null)
                    _selectionCollider.enabled = timeGridVisible;
                _lineRenderer?.SetTime(_stageDeltaTime, _game.HighlightedNoteSpeed, timeGridVisible ? 1f : 0f);
                SetNoteSpriteAlpha(_game.WarningNoteVisibility.HasFlag(GamePlayManager.SpeedWarningNoteVisibility.ShowNote));
                RefreshColoring();
            }
        }

        internal void RefreshVisual()
        {
            if (Model is null)
                return;

            _game.AssertStageLoaded();
            transform.WithLocalPositionX(_game.ConvertNoteCoordPositionToWorldX(SpritePosition));
            SetNoteSprite();
            SetNoteSize();
            RefreshColoring();
            RefreshVisibility();
        }

        internal void RefreshVisibility()
        {
            _lineRenderer.SetVisible(_game.WarningNoteVisibility.HasFlag(GamePlayManager.SpeedWarningNoteVisibility.ShowNoteLine));
            SetNoteSpriteAlpha(_game.WarningNoteVisibility.HasFlag(GamePlayManager.SpeedWarningNoteVisibility.ShowNote));
        }

        internal void RefreshColoring()
        {
            if (!_isActive)
                return;

            Color color = Model.IsSelected ? _game.Stage.Args.NoteSelectedColor : Color.white;
            SetNoteSpriteColorRGB(color);
        }

        private void OnStateChanged(bool active)
        {
            if (_selectionCollider is not null) _selectionCollider.enabled = active;
            _lineRenderer?.SetVisible(active);
            SetBodyActive(active);
            if (active)
                RefreshColoring();
        }

        private void SetNoteSpriteAlpha(bool visible)
        {
            _game.AssertStageLoaded();
            // Debug.Assert(_isActive);

            if (!visible) {
                SetNoteSpriteRendererAlpha(0f);
                return;
            }

            var appearAheadTime = AppearAheadTime;
            var noteFadeInEndTime = appearAheadTime * (1 - _game.Stage.Args.NoteFadeInRangePercent);

            var alpha = MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0f, 1f);
            SetNoteSpriteRendererAlpha(alpha);
        }

        protected abstract void SetNoteSprite();

        protected abstract void SetNoteSize();

        protected abstract void SetNoteSpriteColorRGB(Color color);

        protected abstract void SetNoteSpriteRendererAlpha(float alpha);

        protected abstract void SetBodyActive(bool active);
    }
}
