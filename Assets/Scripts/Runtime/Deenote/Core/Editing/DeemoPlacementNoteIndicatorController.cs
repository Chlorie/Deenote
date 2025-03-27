#nullable enable

using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed class DeemoPlacementNoteIndicatorController : PlacementNoteIndicatorController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRender = default!;

        protected internal override void Refresh()
        {
            _placer._editor._game.AssertStageLoaded();

            var game = _placer._editor._game;
            var stage = _placer._editor._game.Stage;
            var args = stage.Args;
            var prefab = NotePrototype switch {
                { Kind: NoteModel.NoteKind.Swipe } => args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => args.SlideNoteSpritePrefab,
                { HasSounds: true } => args.BlackNoteSpritePrefab,
                _ => _placer._editor._game.IsPianoNotesDistinguished
                    ? args.NoSoundNoteSpritePrefab
                    : args.BlackNoteSpritePrefab,
            };

            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(NotePrototype.Size, 1f, 1f) * prefab.Scale;

            if (NotePrototype.IsSlide && NotePrototype.NextLink is not null) {
                var (tox, toz) = game.ConvertNoteCoordToWorldPosition(NotePrototype.NextLink.PositionCoord - NotePrototype.PositionCoord);
                _linkLineEndOffset = new Vector2(tox, toz);
            }
            else {
                _linkLineEndOffset = null;
            }

            if (NotePrototype.IsHold) {
                ref readonly var holdprefab = ref stage.Args.HoldSpritePrefab;
                _holdBodySpriteRender.gameObject.SetActive(true);
                _holdBodySpriteRender.transform.WithLocalScaleXY(
                    NotePrototype.Size * holdprefab.ScaleX,
                    game.ConvertNoteCoordTimeToHoldScaleY(NotePrototype.Duration, NotePrototype.Speed));
            }
            else {
                _holdBodySpriteRender.gameObject.SetActive(false);
            }
        }
    }
}