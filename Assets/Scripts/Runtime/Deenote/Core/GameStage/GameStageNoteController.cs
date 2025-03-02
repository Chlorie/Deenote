#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal sealed class GameStageNoteController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _holdBodySpriteRenderer = default!;

        [SerializeField] SpriteRenderer _explosionEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _circleEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _waveEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _glowEffectSpriteRenderer = default!;

        private GamePlayManager _game = default!;

        public NoteModel NoteModel { get; private set; } = default!;

        private (Vector2, Vector2)? _linkLine;
        private Color _waveColor;

        [SerializeField]
        internal NoteDisplayState _state;

        // Appear ahead time of note when sudden+ is 0,
        // The value may be affected if the note is following a high-speed note
        private float _appearAheadTime;
        private float _stageDeltaTime;

        // The actual appear ahead time, the value 
        private float AppearAheadTime
        {
            get {
                _game.AssertStageLoaded();

                var suddenPlusAheadTime = _game.Stage.GetNoteAppearAheadTime(NoteModel.Speed);
                float aheadTime;
                if (_game.EarlyDisplaySlowNotes) {
                    aheadTime = suddenPlusAheadTime;
                }
                else {
                    // Keep notes after Next active node invisible
                    if (NoteModel.Time >= _game.NotesManager.GetNextActiveNodeInNonEarlyDisplayMode()?.Time)
                        aheadTime = 0f;
                    else
                        aheadTime = Mathf.Min(suddenPlusAheadTime, _appearAheadTime);
                }
                return aheadTime;
            }
        }

        internal void OnInstantiate(GamePlayManager gamePlayManager)
        {
            _game = gamePlayManager;
        }

        public void Initialize(NoteModel noteModel)
        {
            NoteModel = noteModel;
        }

        public void PostInitialize(GameStageNoteController? previousStageNote)
        {
            SetPrevStageNoteAndAppearAheadTime(previousStageNote);
            RefreshVisual();
            RefreshStageDeltaTime();
        }

        private void Update()
        {
            // TODO:FIX: 当前帧update之后，如果被其他地方调用setlinkline，会导致linkline延迟更新
            // 考虑改造PerspectiveLineRender的AddLine逻辑，依靠register获取每帧的渲染数据。
            if (_linkLine is var (start, end)) {
                _game.AssertStageLoaded();
                _game.PerspectiveLinesRenderer.AddLine(start, end,
                    _game.Stage.GridLineArgs.LinkLineColor with { a = GetNoteSpriteAlpha() },
                    _game.Stage.GridLineArgs.LinkLineWidth);
            }
        }

        #region Refresh

        /// <summary>
        /// Called when music time updated
        /// </summary>
        private void RefreshTimeState()
        {
            if (_stageDeltaTime >= AppearAheadTime) {
                SetState(NoteDisplayState.Invisible);
            }
            else if (_stageDeltaTime >= 0) {
                SetState(NoteDisplayState.Fall);
                OnFalling();
            }
            else if (_stageDeltaTime > -NoteModel.GetActualDuration()) {
                SetState(NoteDisplayState.Holding);
                OnHolding();
            }
            else {
                SetState(NoteDisplayState.HitEffect);
                OnHitEffect();
            }

            void OnFalling()
            {
                SetNotePositionZ(_stageDeltaTime);
                SetNoteSpriteAlpha();
                SetLinkLine(true);
                SetHoldBodyDisplayLength(NoteModel.GetActualDuration());
            }

            void OnHolding()
            {
                SetNotePositionZ(0f);
                SetLinkLine(false);
                SetHoldBodyDisplayLength(_stageDeltaTime + NoteModel.GetActualDuration());

                _game.AssertStageLoaded();

                var stage = _game.Stage;

                ref readonly var prefabs = ref stage.Args.HoldSpritePrefab;
                // TODO: hold's hit effect on judgeline
                ref readonly var effectPrefab = ref stage.Args.HitEffectSpritePrefabs;
                _explosionEffectSpriteRenderer.transform.localScale = NoteModel.Size * effectPrefab.ExplosionScale * Vector3.one;
                _explosionEffectSpriteRenderer.sprite = effectPrefab.Explosions[8];
            }

            void OnHitEffect()
            {
                SetNotePositionZ(0f);
                SetLinkLine(false);
                SetHoldBodyDisplayLength(0f);

                _game.AssertStageLoaded();
                var stage = _game.Stage;

                ref readonly var prefabs = ref stage.Args.HitEffectSpritePrefabs;
                _explosionEffectSpriteRenderer.transform.localScale = NoteModel.Size * prefabs.ExplosionScale * Vector3.one;

                var time = -_stageDeltaTime - NoteModel.GetActualDuration();

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

        public void RefreshStageDeltaTime()
        {
            _stageDeltaTime = NoteModel.Time - _game.MusicPlayer.Time;
            RefreshTimeState();
        }

        public void RefreshLinkLine()
        {
            SetLinkLine(_state is NoteDisplayState.Fall);
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

        public void RefreshNoteSpriteAlpha()
        {
            if (_state is NoteDisplayState.Fall) {
                SetNoteSpriteAlpha();
            }
        }

        internal void RefreshColoring()
        {
            if (_state is NoteDisplayState.Fall) {
                SetNoteSpriteColor();
            }
        }

        #endregion

        #region Setters

        private void SetNotePositionZ(float time)
        {
            _game.AssertStageLoaded();

            float z = _game.Stage.ConvertNoteCoordTimeToWorldZ(time, NoteModel.Speed);
            transform.WithLocalPositionZ(z);
        }

        private void SetNoteSpriteAlpha()
        {
            _noteSpriteRenderer.WithColorAlpha(GetNoteSpriteAlpha());
        }

        private float GetNoteSpriteAlpha()
        {
            _game.AssertStageLoaded();
            Debug.Assert(_state is NoteDisplayState.Fall);

            var appearAheadTime = AppearAheadTime;
            var noteFadeInEndTime = appearAheadTime * (1 - _game.Stage.Args.NoteFadeInRangePercent);
            return MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0f, 1f);
        }

        private void SetLinkLine(bool show)
        {
            _game.AssertStageLoaded();

            if (show && _game.IsShowLinkLines) {
                var stage = _game.Stage;
                var currentTime = _game.MusicPlayer.Time;

                var to = NoteModel.NextLink;
                if (to is null)
                    return;
                var from = NoteModel;

                var (fromX, fromZ) = stage.ConvertNoteCoordToWorldPosition(from.PositionCoord - new NoteCoord(0f, currentTime), from.Speed);
                var (toX, toZ) = stage.ConvertNoteCoordToWorldPosition(to.PositionCoord - new NoteCoord(0f, currentTime), to.Speed);
                _linkLine = (new Vector2(fromX, fromZ), new Vector2(toX, toZ));
            }
            else {
                _linkLine = null;
            }
        }

        private void SetHoldBodyDisplayLength(float time)
        {
            _game.AssertStageLoaded();

            var scaleY = _game.Stage.ConvertNoteCoordTimeToHoldScaleY(time, NoteModel.Speed);
            _holdBodySpriteRenderer.transform.WithLocalScaleY(scaleY);
        }

        private void SetNoteSpriteColor()
        {
            _game.AssertStageLoaded();
            var stage = _game.Stage;

            if (NoteModel.IsSelected)
                _noteSpriteRenderer.WithColorSolid(stage.Args.NoteSelectedColor);
            else if (NoteModel.IsCollided)
                _noteSpriteRenderer.WithColorSolid(stage.Args.NoteCollidedColor);
            else
                _noteSpriteRenderer.WithColorSolid(Color.white);
        }

        private void SetPrevStageNoteAndAppearAheadTime(GameStageNoteController? previousStageNote)
        {
            _game.AssertStageLoaded();

            if (previousStageNote is null) {
                _appearAheadTime = _game.Stage.GetNoteAppearAheadTime(NoteModel.Speed);
                return;
            }

            var prevNoteAppearAheadTime = previousStageNote._appearAheadTime;
            var prevNoteAppearTime = previousStageNote.NoteModel.Time - prevNoteAppearAheadTime;
            var noteAppearTime = _game.Stage.GetNoteAppearTime(NoteModel);
            if (prevNoteAppearTime <= noteAppearTime) {
                _appearAheadTime = _game.Stage.GetNoteAppearAheadTime(NoteModel.Speed);
                return;
            }

            _appearAheadTime = NoteModel.Time - prevNoteAppearTime;
        }

        #endregion

        public enum NoteDisplayState
        {
            InActive,
            Invisible,
            Fall,
            Holding,
            HitEffect,
        }
    }
}