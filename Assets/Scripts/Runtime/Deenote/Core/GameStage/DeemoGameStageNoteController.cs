#nullable enable

using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class DeemoGameStageNoteController : GameStageNoteController
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRenderer = default!;

        [SerializeField] SpriteRenderer _explosionEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _circleEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _waveEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _glowEffectSpriteRenderer = default!;

        private Color _waveColor;

        protected override void SetHoldingHitEffect()
        {
            _game.AssertStageLoaded();

            var stage = _game.Stage;

            ref readonly var prefabs = ref stage.Args.HoldSpritePrefab;
            // TODO: hold's hit effect on judgeline
            ref readonly var effectPrefab = ref stage.Args.HitEffectSpritePrefabs;
            _explosionEffectSpriteRenderer.sprite = effectPrefab.Explosions[8];
        }

        protected override void SetHitEffect(float time)
        {
            _game.AssertStageLoaded();
            var stage = _game.Stage;

            ref readonly var prefabs = ref stage.Args.HitEffectSpritePrefabs;

            // Explosion
            {
                int frame = Mathf.FloorToInt(time / prefabs.ExplosionTime * (prefabs.Explosions.Length + 1));
                _explosionEffectSpriteRenderer.sprite =
                    frame < prefabs.Explosions.Length ? prefabs.Explosions[frame] : null;
            }
            // Circle
            {
                float ratio = time / prefabs.CircleTime;
                // Note: magic number?
                float size = Mathf.Pow(ratio, 0.6f) * prefabs.CircleScale;
                float alpha = Mathf.Pow(1 - ratio, 0.33f);
                _circleEffectSpriteRenderer.transform.localScale = new Vector3(size, size, size);
                _circleEffectSpriteRenderer.WithColorAlpha(alpha);
            }
            // Wave
            {
                float ratio = time <= prefabs.WaveGrowTime
                    ? time / prefabs.WaveGrowTime
                    : 1 - (time - prefabs.WaveGrowTime) / prefabs.WaveFadeTime;
                float alpha = Mathf.Pow(ratio, 0.5f);
                _waveEffectSpriteRenderer.transform.localScale
                    = NoteModel.Size * new Vector3(prefabs.WaveScale.x, ratio * prefabs.WaveScale.y, 1f);
                _waveEffectSpriteRenderer.color
                    = _waveColor with { a = Mathf.Lerp(0, prefabs.WaveMaxAlpha, alpha) };
            }
            // Glow
            {
                const float GlowHeight = 1f;

                float ratio = time <= prefabs.GlowGrowTime
                    ? time / prefabs.GlowGrowTime
                    : 1 - (time - prefabs.GlowGrowTime) / prefabs.GlowFadeTime;
                float height = ratio * GlowHeight;
                _glowEffectSpriteRenderer.transform.localScale =
                    new Vector3(prefabs.GlowScale.x, height * prefabs.GlowScale.y, 1f);
                _glowEffectSpriteRenderer.color = prefabs.GlowColor with { a = ratio };
            }
        }

        protected override void SetNoteSprite()
        {
            _game.AssertStageLoaded();

            var prefab = NoteModel switch {
                { Kind: NoteModel.NoteKind.Swipe } => _game.Stage.Args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => _game.Stage.Args.SlideNoteSpritePrefab,
                { HasSounds: true } => _game.Stage.Args.BlackNoteSpritePrefab,
                _ when _game.IsPianoNotesDistinguished => _game.Stage.Args.NoSoundNoteSpritePrefab,
                _ => _game.Stage.Args.BlackNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _waveColor = prefab.WaveColor;

            if (NoteModel.IsHold) {
                _holdBodySpriteRenderer.gameObject.SetActive(true);
            }
            else {
                _holdBodySpriteRenderer.gameObject.SetActive(false);
            }
        }

        protected override void SetNoteSize()
        {
            _game.AssertStageLoaded();

            var prefab = NoteModel switch {
                { Kind: NoteModel.NoteKind.Swipe } => _game.Stage.Args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => _game.Stage.Args.SlideNoteSpritePrefab,
                { HasSounds: true } => _game.Stage.Args.BlackNoteSpritePrefab,
                _ when _game.IsPianoNotesDistinguished => _game.Stage.Args.NoSoundNoteSpritePrefab,
                _ => _game.Stage.Args.BlackNoteSpritePrefab,
            };
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(NoteModel.Size, 1f, 1f) * prefab.Scale;

            ref readonly var hiteffectPrefab = ref _game.Stage.Args.HitEffectSpritePrefabs;
            _explosionEffectSpriteRenderer.transform.localScale = NoteModel.Size * hiteffectPrefab.ExplosionScale * Vector3.one;

            if (NoteModel.IsHold) {
                ref readonly var holdPrefab = ref _game.Stage.Args.HoldSpritePrefab;
                _holdBodySpriteRenderer.transform.WithLocalScaleX(NoteModel.Size * holdPrefab.ScaleX);
                // Scale.y is set when time changed
            }
        }

        protected override void SetNoteSpriteRendererAlpha(float alpha)
        {
            _noteSpriteRenderer.WithColorAlpha(alpha);
        }

        protected override void SetHoldScaleY(float scaleY, bool isHolding)
        {
            _holdBodySpriteRenderer.transform.WithLocalScaleY(scaleY);
        }

        protected override void SetNoteSpriteColorRGB(Color color)
        {
            _noteSpriteRenderer.WithColorRGB(color);
        }

        protected override void OnStateChanged(NoteDisplayState state)
        {
            switch (state) {
                case NoteDisplayState.Invisible:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(false);
                    _explosionEffectSpriteRenderer.gameObject.SetActive(false);
                    _explosionEffectSpriteRenderer.gameObject.SetActive(false);
                    _circleEffectSpriteRenderer.gameObject.SetActive(false);
                    _waveEffectSpriteRenderer.gameObject.SetActive(false);
                    _glowEffectSpriteRenderer.gameObject.SetActive(false);
                    break;
                case NoteDisplayState.Fall:
                    _noteSpriteRenderer.gameObject.SetActive(true);
                    _holdBodySpriteRenderer.gameObject.SetActive(true);
                    _explosionEffectSpriteRenderer.gameObject.SetActive(false);
                    _circleEffectSpriteRenderer.gameObject.SetActive(false);
                    _waveEffectSpriteRenderer.gameObject.SetActive(false);
                    _glowEffectSpriteRenderer.gameObject.SetActive(false);
                    RefreshColoring();
                    break;
                case NoteDisplayState.Holding:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(true);
                    _explosionEffectSpriteRenderer.gameObject.SetActive(true);
                    _circleEffectSpriteRenderer.gameObject.SetActive(false);
                    _waveEffectSpriteRenderer.gameObject.SetActive(false);
                    _glowEffectSpriteRenderer.gameObject.SetActive(false);
                    break;
                case NoteDisplayState.HitEffect:
                    _noteSpriteRenderer.gameObject.SetActive(false);
                    _holdBodySpriteRenderer.gameObject.SetActive(false);
                    _explosionEffectSpriteRenderer.gameObject.SetActive(true);
                    _circleEffectSpriteRenderer.gameObject.SetActive(true);
                    _waveEffectSpriteRenderer.gameObject.SetActive(true);
                    _glowEffectSpriteRenderer.gameObject.SetActive(true);
                    break;
            }
        }
    }
}