#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public abstract class PlacementNoteIndicatorController : MonoBehaviour
    {
        private const float NoteAlpha = 0.5f;

        protected StageNotePlacer _placer = default!;

        private Vector2 _localPosition;
        protected Vector2? _linkLineEndOffset;
        protected NoteModel _note = default!;

        public NoteModel NotePrototype => _note;

        internal void OnInstantiate(StageNotePlacer placer)
        {
            _placer = placer;

            _note = new();

            var game = _placer._editor._game;
            game.AssertStageLoaded();
            game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal protected abstract void Refresh();

        internal void Initialize(NoteModel note)
        {
            _note = note;
            Refresh();
        }

        private void OnDestroy()
        {
            var game = _placer._editor._game;
            if (game.IsStageLoaded())
                game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _linkLineEndOffset = null;
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            var showLinkLine = _placer._editor._game.IsShowLinkLines;
            if (showLinkLine && _linkLineEndOffset is { } offset) {
                _placer._editor._game.AssertStageLoaded();

                var args = _placer._editor._game.Stage.GridLineArgs;
                collector.AddLine(_localPosition, _localPosition + offset,
                    args.LinkLineColor with { a = NoteAlpha },
                    args.LinkLineWidth);
            }
        }

        public void MoveTo(NoteCoord coord)
        {
            _placer._editor._game.AssertStageLoaded();

            var localCoord = coord - new NoteCoord(position: 0, time: _placer._editor._game.MusicPlayer.Time);
            var (x, z) = _placer._editor._game.ConvertNoteCoordToWorldPosition(localCoord, NotePrototype.Speed);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);
        }
    }
}