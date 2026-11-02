#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.Editing
{
    /// <summary>
    /// Semi-transparent preview of a speed change warning. It intentionally has no
    /// note-kind/hold/link logic and no selection collider.
    /// </summary>
    public sealed class SpeedChangeWarningIndicatorController : MonoBehaviour
    {
        private const float PreviewAlpha = 0.5f;

        [SerializeField] private SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] private SpeedChangeWarningLineRenderer _lineRenderer = default!;
        
        private StageNotePlacer _placer = default!;
        private float _time;

        internal SpeedChangeWarningLineRenderer LineRenderer => _lineRenderer;

        internal void OnInstantiate(StageNotePlacer placer)
        {
            _placer = placer;

            var game = _placer._editor._game;
            game.AssertStageLoaded();

            _lineRenderer ??= GetComponentInChildren<SpeedChangeWarningLineRenderer>(true);
            _lineRenderer?.OnInstantiate(game);

            transform.WithLocalPositionX(
                game.ConvertNoteCoordPositionToWorldX(GameStageSpeedChangeWarningNoteController.SpritePosition));
            _noteSpriteRenderer?.WithColorAlpha(PreviewAlpha);
        }

        private void OnDisable()
        {
            _lineRenderer?.SetVisible(false);
        }

        public void MoveTo(float time)
        {
            _time = time;

            var game = _placer._editor._game;
            game.AssertStageLoaded();

            float localTime = time - game.MusicPlayer.Time;
            float z = game.ConvertNoteCoordTimeToWorldZ(localTime, _placer.PlacingNoteSpeed);
            transform.WithLocalPositionZ(z);

            _lineRenderer?.SetTime(localTime, _placer.PlacingNoteSpeed, PreviewAlpha);
        }

        public SpeedChangeWarningModel CreateModel() => new(_time);
    }
}
