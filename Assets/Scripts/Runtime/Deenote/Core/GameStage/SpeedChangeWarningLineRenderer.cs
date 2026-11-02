#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    /// <summary>
    /// Draws the time line of a speed change warning through
    /// <see cref="PerspectiveLinesRenderer"/> so that it is rendered in the same way as time grids.
    /// </summary>
    internal sealed class SpeedChangeWarningLineRenderer : MonoBehaviour
    {
        [SerializeField] private float _lineStartPosition = -3f;
        [SerializeField] private float _lineEndPosition = 2f;

        private GamePlayManager? _game;
        private float _localTime;
        private float _speed = 1f;
        private float _alpha = 1f;
        private bool _visible;

        internal void OnInstantiate(GamePlayManager game)
        {
            _game = game;
            game.AssertStageLoaded();
            game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        private void OnDestroy()
        {
            if (_game is not null && _game.IsStageLoaded()) {
                _game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
            }
        }

        internal void SetTime(float localTime, float speed, float alpha)
        {
            _localTime = localTime;
            _speed = speed;
            _alpha = alpha;
        }

        internal void SetVisible(bool visible) => _visible = visible;

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (!_visible || _game is null || !_game.IsStageLoaded())
                return;

            if (_alpha <= 0)
                return;

            var z = _game.ConvertNoteCoordTimeToWorldZ(_localTime, _speed);
            var xl = _game.ConvertNoteCoordPositionToWorldX(_lineStartPosition);
            var xr = _game.ConvertNoteCoordPositionToWorldX(_lineEndPosition);
            var args = _game.Stage.GridLineArgs;
            collector.AddLine(
                new Vector2(xl, z),
                new Vector2(xr, z),
                args.SpeedChangeWarningLineColor with { a = args.SpeedChangeWarningLineColor.a * _alpha },
                args.SpeedChangeWarningLineWidth);
        }
    }
}
