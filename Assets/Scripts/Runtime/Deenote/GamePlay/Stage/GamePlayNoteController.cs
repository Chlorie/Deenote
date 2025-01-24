#nullable enable

using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.GamePlay.Audio;
using Deenote.Library;
using System.Timers;
using UnityEngine;

namespace Deenote.GamePlay.Stage
{
    internal sealed class GamePlayNoteController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRenderer = default!;

        [SerializeField] SpriteRenderer _explosionEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _circleEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _waveEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _glowEffectSpriteRenderer = default!;

        private GamePlayManager _game = default!;

        private (Vector2, Vector2)? _linkLine;
        private Color _waveColor;

        // Buffer
        private NoteDisplayState _state;

        public NoteModel NoteModel { get; private set; } = default!;

        internal void OnInstantiate(GamePlayManager gamePlayManager)
        {
            _game = gamePlayManager;
        }

        public void Initialize(NoteModel noteModel)
        {
            NoteModel = noteModel;
            RefreshVisual();
        }

        private void Update()
        {
            if (_linkLine is var (start, end)) {
                _game.AssertStageLoaded();
                _game.PerspectiveLinesRenderer.AddLine(start, end,
                    _game.Stage.GridLineArgs.LinkLineColor,
                    _game.Stage.GridLineArgs.LinkLineWidth);
            }
        }

        public void PlayHitSosund()
        {
            if (NoteModel.IsSlide)
                _game.HitSoundPlayer.PlaySlideSound();
            else
                _game.HitSoundPlayer.PlayClickSound();
        }

        public void PlayPianoSound()
        {
            if (NoteModel.HasSounds) {
                _game.PianoSoundPlayer.PlaySounds(NoteModel.Sounds);
            }
        }

        /// <summary>
        /// Called when music time updated
        /// </summary>
        internal void RefreshTimeState()
        {
            _game.AssertStageLoaded();

            var currentTime = _game.MusicPlayer.Time;
            var timeDelta = NoteModel.Time - currentTime;
            if (timeDelta >= 0) {
                SetState(NoteDisplayState.Fall);
                OnFalling();
            }
            else if (-timeDelta < NoteModel.GetActualDuration()) {
                SetState(NoteDisplayState.Holding);
                OnHolding();
            }
            else {
                _linkLine = null;
                SetState(NoteDisplayState.HitEffect);
                OnHitEffect();
            }

            void OnFalling()
            {
                var stage = _game.Stage;
                float noteAppearTime = stage.NoteAppearAheadTime;
                if (timeDelta > noteAppearTime) {
                    _noteSpriteRenderer.WithColorAlpha(0f);
                    SetHoldBodyDisplayLength(0);
                    return;
                }

                float noteFadeInTime = noteAppearTime * stage.Args.NoteFadeInRangePercent;
                float alpha = Mathf.Clamp01((noteAppearTime - timeDelta) / noteFadeInTime);

                _noteSpriteRenderer.WithColorAlpha(alpha);

                // Position

                float z = stage.ConvertNoteCoordTimeToWorldZ(timeDelta);
                this.transform.WithLocalPositionZ(z);

                // Link lines

                if (_game.IsShowLinkLines) {
                    var to = NoteModel.NextLink;
                    if (to is null)
                        goto EndLinkLine;
                    var from = NoteModel;

                    var (fromX, fromZ) = stage.ConvertNoteCoordToWorldPosition(from.PositionCoord - new NoteCoord(0f, currentTime));
                    var (toX, toZ) = stage.ConvertNoteCoordToWorldPosition(to.PositionCoord - new NoteCoord(0f, currentTime));
                    _linkLine = (new Vector2(fromX, fromZ), new Vector2(toX, toZ));
                }
            EndLinkLine:

                // Hold body
                SetHoldBodyDisplayLength(NoteModel.GetActualDuration());
            }

            void OnHolding()
            {
                Debug.Assert(NoteModel.IsHold);
                var stage = _game.Stage;
                this.transform.WithLocalPositionZ(stage.ConvertNoteCoordTimeToWorldZ(0f));
                ref readonly var prefabs = ref stage.Args.HoldSpritePrefab;

                var restDuration = NoteModel.EndTime - currentTime;
                SetHoldBodyDisplayLength(restDuration);

                // TODO: hold's hit effect on judgeline
                ref readonly var effectPrefab = ref stage.Args.HitEffectSpritePrefabs;
                _explosionEffectSpriteRenderer.transform.localScale = NoteModel.Size * effectPrefab.ExplosionScale * Vector3.one;
                _explosionEffectSpriteRenderer.sprite = effectPrefab.Explosions[8];
            }

            void OnHitEffect()
            {
                Debug.Assert(currentTime >= NoteModel.EndTime);
                var stage = _game.Stage;
                this.transform.WithLocalPositionZ(stage.ConvertNoteCoordTimeToWorldZ(0f));
                _holdBodySpriteRenderer.transform.WithLocalScaleY(0f);
                ref readonly var prefabs = ref stage.Args.HitEffectSpritePrefabs;
                _explosionEffectSpriteRenderer.transform.localScale = NoteModel.Size * prefabs.ExplosionScale * Vector3.one;

                var time = currentTime - NoteModel.EndTime;

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

            void SetState(NoteDisplayState state)
            {
                if (Utils.SetField(ref _state, state)) {
                    switch (state) {
                        case NoteDisplayState.Fall:
                            _noteSpriteRenderer.gameObject.SetActive(true);
                            RefreshColoring();
                            _explosionEffectSpriteRenderer.gameObject.SetActive(false);
                            _circleEffectSpriteRenderer.gameObject.SetActive(false);
                            _waveEffectSpriteRenderer.gameObject.SetActive(false);
                            _glowEffectSpriteRenderer.gameObject.SetActive(false);
                            break;
                        case NoteDisplayState.Holding:
                            _noteSpriteRenderer.gameObject.SetActive(false);
                            _explosionEffectSpriteRenderer.gameObject.SetActive(true);
                            _circleEffectSpriteRenderer.gameObject.SetActive(false);
                            _waveEffectSpriteRenderer.gameObject.SetActive(false);
                            _glowEffectSpriteRenderer.gameObject.SetActive(false);
                            break;
                        case NoteDisplayState.HitEffect:
                            _noteSpriteRenderer.gameObject.SetActive(false);
                            _explosionEffectSpriteRenderer.gameObject.SetActive(true);
                            _circleEffectSpriteRenderer.gameObject.SetActive(true);
                            _waveEffectSpriteRenderer.gameObject.SetActive(true);
                            _glowEffectSpriteRenderer.gameObject.SetActive(true);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Set note's properties according to <see cref="NoteModel"/>, except time
        /// </summary>
        public void RefreshVisual()
        {
            _game.AssertStageLoaded();

            gameObject.transform.WithLocalPositionX(_game.Stage.ConvertNoteCoordPositionToWorldX(NoteModel.Position));
            var prefab = NoteModel switch {
                { Kind: NoteModel.NoteKind.Swipe } => _game.Stage.Args.SwipeNoteSpritePrefab,
                { Kind: NoteModel.NoteKind.Slide } => _game.Stage.Args.SlideNoteSpritePrefab,
                { HasSounds: true } => _game.Stage.Args.BlackNoteSpritePrefab,
                _ when _game.IsPianoNotesDistinguished => _game.Stage.Args.NoSoundNoteSpritePrefab,
                _ => _game.Stage.Args.BlackNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(NoteModel.Size, 1f, 1f) * prefab.Scale;

            if (NoteModel.IsHold) {
                ref readonly var holdPrefab = ref _game.Stage.Args.HoldSpritePrefab;
                _holdBodySpriteRenderer.gameObject.SetActive(true);
                _holdBodySpriteRenderer.transform.WithLocalScaleX(NoteModel.Size * holdPrefab.ScaleX);
                // Scale.y is set when time changed
            }
            else {
                _holdBodySpriteRenderer.gameObject.SetActive(false);
            }

            RefreshColoring();

            _linkLine = null;
            _waveColor = prefab.WaveColor;
        }

        private void RefreshColoring()
        {
            _game.AssertStageLoaded();

            if (NoteModel.IsSelected)
                _noteSpriteRenderer.WithColorRGB(_game.Stage.Args.NoteSelectedColor);
            else if (NoteModel.IsCollided)
                _noteSpriteRenderer.WithColorRGB(_game.Stage.Args.NoteCollidedColor);
            else
                _noteSpriteRenderer.WithColorRGB(Color.white);
        }

        #region Setters

        private void SetHoldBodyDisplayLength(float time)
        {
            _game.AssertStageLoaded();

            var scaleY = _game.Stage.ConvertNoteCoordTimeToHoldScaleY(time);
            _holdBodySpriteRenderer.transform.WithLocalScaleY(scaleY);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            var b = _holdBodySpriteRenderer.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        public enum NoteDisplayState
        {
            InActive,
            Fall,
            Holding,
            HitEffect,
        }
    }
}