#nullable enable

using Deenote.Core.GameStage.Args;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class DeemoGameStageSpeedChangeWarningNoteController : GameStageSpeedChangeWarningNoteController
    {
        [SerializeField] private SpriteRenderer _noteSpriteRenderer = default!;

        protected override void SetNoteSprite()
        {
            _noteSpriteRenderer.sprite = _game.Stage!.Args.SpeedChangeWarningSpritePrefab.Sprite; 
        }

        protected override void SetNoteSize()
        {
            var prefab = _game.Stage!.Args.SpeedChangeWarningSpritePrefab;
            _noteSpriteRenderer.transform.localScale = Vector3.one * prefab.Scale;
        }

        protected override void SetNoteSpriteColorRGB(Color color)
        {
            _noteSpriteRenderer.WithColorRGB(color);
        }

        protected override void SetNoteSpriteRendererAlpha(float alpha)
        {
            _noteSpriteRenderer.WithColorAlpha(alpha);
        }

        protected override void SetBodyActive(bool active)
        {
            _noteSpriteRenderer.gameObject.SetActive(active);
        }

        private GameStageArgs.NoteSpritePrefab GetSpritePrefab()
        {
            var args = _game.Stage!.Args;
            return args.SpeedChangeWarningSpritePrefab;
        }
    }
}
