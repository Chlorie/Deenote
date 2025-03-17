#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class PlacementNoteIndicatorController : MonoBehaviour
    {
        private const float NoteAlpha = 0.5f;

        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRender = default!;
        private StageNotePlacer _placer = default!;

        private Vector2 _localPosition;
        private Vector2? _linkLineEndOffset;
        private NoteModel _note = default!;

        public NoteModel NotePrototype => _note;

        internal void OnInstantiate(StageNotePlacer placer)
        {
            _placer = placer;

            _note = new();

            var game = _placer._editor._game;
            game.AssertStageLoaded();
            game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal void Refresh()
        {
            _placer._editor._game.AssertStageLoaded();

            var game = _placer._editor._game;
            var stage = _placer._editor._game.Stage;
            var args = stage.Args;
            var prefab = _note switch {
                { Kind: NoteModel.NoteKind.Swipe } => args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => args.SlideNoteSpritePrefab,
                { HasSounds: true } => args.BlackNoteSpritePrefab,
                _ => _placer._editor._game.IsPianoNotesDistinguished
                    ? args.NoSoundNoteSpritePrefab
                    : args.BlackNoteSpritePrefab,
            };

            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(_note.Size, 1f, 1f) * prefab.Scale;

            if (_note.IsSlide && _note.NextLink is not null) {
                var (tox, toz) = game.ConvertNoteCoordToWorldPosition(_note.NextLink.PositionCoord - _note.PositionCoord);
                _linkLineEndOffset = new Vector2(tox, toz);
            }
            else {
                _linkLineEndOffset = null;
            }

            if (_note.IsHold) {
                ref readonly var holdprefab = ref stage.Args.HoldSpritePrefab;
                _holdBodySpriteRender.gameObject.SetActive(true);
                _holdBodySpriteRender.transform.WithLocalScaleXY(
                    _note.Size * holdprefab.ScaleX,
                    game.ConvertNoteCoordTimeToHoldScaleY(_note.Duration, _note.Speed));
            }
            else {
                _holdBodySpriteRender.gameObject.SetActive(false);
            }
        }

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
            var (x, z) = _placer._editor._game.ConvertNoteCoordToWorldPosition(localCoord, _note.Speed);
            _localPosition = new Vector2(x, z);
            transform.WithLocalPositionXZ(x, z);
        }
    }
}