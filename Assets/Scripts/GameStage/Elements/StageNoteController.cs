using Deenote.Project.Models;
using Deenote.Utilities;
using UnityEngine;

namespace Deenote.GameStage.Elements
{
    public sealed class StageNoteController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _noteSpriteRenderer = null!;
        [SerializeField] private SpriteRenderer _explosionHitEffectSpriteRenderer = null!;
        [SerializeField] private SpriteRenderer _circleHitEffectSpriteRenderer = null!;
        [SerializeField] private SpriteRenderer _waveHitEffectSpriteRenderer = null!;
        [SerializeField] private SpriteRenderer _glowHitEffectSpriteRenderer = null!;

        [Header("")]
        [SerializeField] private AudioSource _effectSoundSource = null!;

        private (Vector2, Vector2)? _linkLine;

        public GameStageController Stage => MainSystem.GameStage;

        private Color _waveColor;

        [Header("Datas")]
        [SerializeField]
        private NoteModel _note = null!;
        public NoteModel Model => _note;

        [SerializeField]
        private NoteState _state;
        public NoteState State
        {
            get => _state;
            set
            {
                if (value == _state)
                    return;

                _state = value;
                switch (_state)
                {
                    case NoteState.Fall:
                        _noteSpriteRenderer.gameObject.SetActive(true);
                        _explosionHitEffectSpriteRenderer.gameObject.SetActive(false);
                        _circleHitEffectSpriteRenderer.gameObject.SetActive(false);
                        _waveHitEffectSpriteRenderer.gameObject.SetActive(false);
                        _glowHitEffectSpriteRenderer.gameObject.SetActive(false);
                        break;
                    case NoteState.HitEffect:
                        _noteSpriteRenderer.gameObject.SetActive(false);
                        _explosionHitEffectSpriteRenderer.gameObject.SetActive(true);
                        _circleHitEffectSpriteRenderer.gameObject.SetActive(true);
                        _waveHitEffectSpriteRenderer.gameObject.SetActive(true);
                        _glowHitEffectSpriteRenderer.gameObject.SetActive(true);
                        break;
                    case NoteState.Inactive:
                        _noteSpriteRenderer.gameObject.SetActive(false);
                        _explosionHitEffectSpriteRenderer.gameObject.SetActive(false);
                        _circleHitEffectSpriteRenderer.gameObject.SetActive(false);
                        _waveHitEffectSpriteRenderer.gameObject.SetActive(false);
                        _glowHitEffectSpriteRenderer.gameObject.SetActive(false);
                        break;
                }
            }
        }

        public void Initialize(NoteModel noteModel)
        {
            _note = noteModel;
            State = NoteState.Activated;

            SyncNoteDataUpdate();
        }

        private void Update()
        {
            if (_linkLine is var (start, end))
                PerspectiveLinesRenderer.Instance.AddLine(start, end, MainSystem.Args.LinkLineColor, 2.0f);
        }

        // GameStageController will select notes in stage when Update,
        // So we use LateUpdate so that it will correctly update display
        // after we confirmed which notes are on stage
        private void LateUpdate()
        {
            if (State is NoteState.Inactive)
                return;

            if (MainSystem.GameStage.StagePlaySpeed != 0)
                UpdateDisplay(MainSystem.GameStage.IsMusicPlaying);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This should be called when manually set music time when music is paused,
        /// as NoteController wont invoke UpdateDisplay() when paused.
        /// </remarks>
        public void ForceUpdate(bool noteDataChangedExceptTime)
        {
            if (noteDataChangedExceptTime)
                SyncNoteDataUpdate();
            UpdateDisplay(false);
        }

        public void SyncNoteDataUpdate()
        {
            gameObject.transform.localPosition = gameObject.transform.localPosition.WithX(MainSystem.Args.PositionToX(_note.Data.Position));
            var prefab = _note.Data switch
            {
                { IsSlide: true } => MainSystem.GameStage.SlideNoteSpritePrefab,
                { HasSound: true } => MainSystem.GameStage.BlackNoteSpritePrefab,
                _ => MainSystem.GameStage.NoSoundNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(_note.Data.Size, 1f, 1f) * prefab.Scale;
            _linkLine = null;
            _waveColor = prefab.WaveColor;
        }

        private void UpdateDisplay(bool musicPlaying)
        {
            float currentTime = Stage.CurrentMusicTime;
            float timeOffset = _note.Data.Time - currentTime;
            // Falling
            if (timeOffset > 0)
            {
                State = NoteState.Fall;
                OnFalling();
            }
            // Hit effect or released
            else
            {
                _linkLine = null;
                if (State != NoteState.HitEffect)
                {
                    if (musicPlaying)
                    {
                        PlayNoteSounds();
                    }
                    State = NoteState.HitEffect;
                }
                OnHitEffect();
            }
            return;

            void OnFalling()
            {
                // Color
                float alpha = Mathf.Clamp01((Stage.StageNoteAheadTime - timeOffset) / MainSystem.Args.StageNoteFadeInTime);
                _noteSpriteRenderer.color =
                    Model.IsSelected ? MainSystem.Args.NoteSelectedColor.WithAlpha(alpha) :
                    Model is { IsCollided: true, Data.IsSlide: false } ? MainSystem.Args.NoteCollidedColor.WithAlpha(alpha) :
                    Color.white.WithAlpha(alpha);

                // Position
                Debug.Assert(_note.Data.Time > Stage.CurrentMusicTime);
                float z = MainSystem.Args.OffsetTimeToZ(timeOffset);
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0f, z);

                // Link line
                if (!MainSystem.GameStage.IsShowLinkLines)
                    return;

                var from = Model.Data;
                var to = from.NextLink;
                if (to is null)
                    return;

                var (fromX, fromZ) = MainSystem.Args.NoteCoordToWorldPosition(from.PositionCoord, Stage.CurrentMusicTime);
                var (toX, toZ) = MainSystem.Args.NoteCoordToWorldPosition(to.PositionCoord, Stage.CurrentMusicTime);
                _linkLine = (new Vector2(fromX, fromZ), new Vector2(toX, toZ));
            }

            void OnHitEffect()
            {
                Debug.Assert(timeOffset <= 0);

                ref readonly var prefabs = ref Stage.HitEffectSpritePrefabs;

                // Initialize effect
                gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, 0f, 0f);
                float noteSize = _note.Data.Size;
                _explosionHitEffectSpriteRenderer.transform.localScale = noteSize * prefabs.ExplosionScale * Vector3.one;

                // Copied from Chlorie's
                var time = -timeOffset;
                if (time > prefabs.HitEffectTime)
                {
                    return;
                }

                // Explosion
                {
                    int frame = Mathf.FloorToInt(time / prefabs.ExplosionTime * (prefabs.Explosions.Length + 1));
                    // Here frame may be Explosions.Length
                    _explosionHitEffectSpriteRenderer.sprite = frame < prefabs.Explosions.Length ? prefabs.Explosions[frame] : null;
                }

                // Circle
                {
                    float ratio = time / prefabs.CircleTime;
                    // TODO: magic number?
                    float size = Mathf.Pow(ratio, 0.6f) * prefabs.CircleScale;
                    float alpha = Mathf.Pow(1 - ratio, 0.33f);
                    _circleHitEffectSpriteRenderer.transform.localScale = new Vector3(size, size, size);
                    _circleHitEffectSpriteRenderer.color = new Color(0f, 0f, 0f, alpha);
                }

                // Wave
                {
                    float ratio = time <= prefabs.WaveGrowTime
                        ? time / prefabs.WaveGrowTime
                        : 1 - (time - prefabs.WaveGrowTime) / prefabs.WaveFadeTime;
                    float alpha = Mathf.Pow(ratio, 0.5f);
                    _waveHitEffectSpriteRenderer.transform.localScale = _note.Data.Size * prefabs.WaveScale * new Vector3(1, ratio, ratio);
                    _waveHitEffectSpriteRenderer.color = _waveColor.WithAlpha(alpha);
                }

                // Glow
                {
                    const float GlowHeight = 1f;

                    float ratio = time <= prefabs.GlowGrowTime
                        ? time / prefabs.GlowGrowTime
                        : 1 - (time - prefabs.GlowGrowTime) / prefabs.GlowFadeTime;
                    float height = ratio * GlowHeight;
                    _glowHitEffectSpriteRenderer.transform.localScale = prefabs.GlowScale * new Vector3(1f, height, height);
                    _glowHitEffectSpriteRenderer.color = prefabs.GlowColor.WithAlpha(ratio);
                }
            }
        }

        private void PlayNoteSounds()
        {
            if (Stage.EffectVolume > 0f)
            {
                if (Model.Data.IsSlide)
                    _effectSoundSource.PlayOneShot(Stage.EffectSoundAudioClip, Stage.EffectVolume / 200f);
                else
                    _effectSoundSource.PlayOneShot(Stage.EffectSoundAudioClip, Stage.EffectVolume / 100f);
            }

            if (Stage.MusicVolume > 0f && Model.Data.HasSound)
            {
                MainSystem.PianoSoundManager.PlaySoundsAsync(Model.Data.Sounds, Stage.PianoVolume / 100f, Stage.MusicSpeed / 10f).Forget();
            }
        }

        public enum NoteState
        {
            Activated,
            Fall,
            HitEffect,
            Inactive,
        }
    }
}