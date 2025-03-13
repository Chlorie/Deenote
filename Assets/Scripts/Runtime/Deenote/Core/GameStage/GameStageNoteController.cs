#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Numerics;
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
        private float _noteColorAlpha;

        [SerializeField]
        internal NoteDisplayState _state;

        // Appear ahead time of note when sudden+ is 0,
        // The value may be affected if the note is following a high-speed note
        private float _appearAheadTime0SuddenPlus;
        private float _stageDeltaTime;

        // The actual appear ahead time, the value 
        private float AppearAheadTime
        {
            get {
                _game.AssertStageLoaded();

                var suddenPlusAheadTime = _game.GetStageNoteAppearAheadTime(NoteModel.Speed);
                float aheadTime;
                if (_game.EarlyDisplaySlowNotes) {
                    aheadTime = suddenPlusAheadTime;
                }
                else {
                    aheadTime = Mathf.Min(suddenPlusAheadTime, _appearAheadTime0SuddenPlus);
                }
                return aheadTime;
            }
        }

        internal void OnInstantiate(GamePlayManager gamePlayManager)
        {
            _game = gamePlayManager;

            _game.AssertStageLoaded();
            _game.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
        }

        internal void Initialize(NoteModel noteModel)
        {
            NoteModel = noteModel;
        }

        internal void PostInitialize(GameStageNoteController? previousStageNote)
        {
            SetAppearAheadTime0SuddenPlus(previousStageNote);
            RefreshVisual();
            RefreshStageDeltaTime();
        }

        private void OnDestroy()
        {
            if (_game.IsStageLoaded())
                _game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        private void OnDisable()
        {
            _state = NoteDisplayState.Inactive;
            SetLinkLine();
        }

        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            if (_linkLine is var (start, end)) {
                _game.AssertStageLoaded();
                collector.AddLine(start, end,
                    _game.Stage.GridLineArgs.LinkLineColor with { a = _noteColorAlpha },
                    _game.Stage.GridLineArgs.LinkLineWidth);
            }
        }

        #region Refresh

        /// <summary>
        /// Called when music time updated
        /// </summary>
        private void RefreshTimeDisplayState()
        {
            SetState();
            switch (_state) {
                case NoteDisplayState.Invisible:
                    SetLinkLine();
                    break;
                case NoteDisplayState.Fall:
                    SetNotePositionZ(_stageDeltaTime);
                    SetNoteSpriteAlpha();
                    SetLinkLine();
                    SetHoldBodyDisplayLength(NoteModel.GetActualDuration());
                    break;
                case NoteDisplayState.Holding:
                    OnHolding();
                    break;
                case NoteDisplayState.HitEffect:
                    OnHitEffect();
                    break;
            }

            void OnHolding()
            {
                SetNotePositionZ(0f);
                SetLinkLine();
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
                SetLinkLine();
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
        }

        private NoteDisplayState GetState()
        {
            if (IsInvisible())
                return NoteDisplayState.Invisible;
            if (_stageDeltaTime >= 0)
                return NoteDisplayState.Fall;
            if (_stageDeltaTime > -NoteModel.GetActualDuration())
                return NoteDisplayState.Holding;
            return NoteDisplayState.HitEffect;

            bool IsInvisible()
            {
                if (_stageDeltaTime >= AppearAheadTime)
                    return true;

                if (!_game.EarlyDisplaySlowNotes) {
                    // In TimeOrder mode, the note should display only after its previous note displayed
                    if (_game.NotesManager.GetNextActiveNodeInTimeOrderDisplayMode() is { } next) {
                        if (NodeTimeUniqueComparer.Instance.Compare(NoteModel, next) >= 0) {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public void RefreshStageDeltaTime()
        {
            _stageDeltaTime = NoteModel.Time - _game.MusicPlayer.Time;
            RefreshTimeDisplayState();
        }

        public void RefreshLinkLine()
        {
            SetLinkLine();
        }

        /// <summary>
        /// Set note's properties according to <see cref="NoteModel"/>, except time
        /// </summary>
        public void RefreshVisual()
        {
            _game.AssertStageLoaded();

            SetNotePositionX();
            SetNoteSprite();
            RefreshColoring();
            SetLinkLine();
        }

        public void RefreshColorAlpha()
        {
            if (_state is NoteDisplayState.Invisible or NoteDisplayState.Fall) {
                SetState();
                if (_state is NoteDisplayState.Invisible)
                    SetLinkLine();
                if (_state is NoteDisplayState.Fall)
                    SetNoteSpriteAlpha();
            }
        }

        public void RefreshColoring()
        {
            if (_state is NoteDisplayState.Fall) {
                SetNoteSpriteColor();
            }
        }

        #endregion

        #region Setters

        private void SetNotePositionX()
        {
            transform.WithLocalPositionX(_game.ConvertNoteCoordPositionToWorldX(NoteModel.Position));
        }

        private void SetNoteSprite()
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
            _noteSpriteRenderer.gameObject.transform.localScale = new Vector3(NoteModel.Size, 1f, 1f) * prefab.Scale;
            _waveColor = prefab.WaveColor;

            if (NoteModel.IsHold) {
                ref readonly var holdPrefab = ref _game.Stage.Args.HoldSpritePrefab;
                _holdBodySpriteRenderer.gameObject.SetActive(true);
                _holdBodySpriteRenderer.transform.WithLocalScaleX(NoteModel.Size * holdPrefab.ScaleX);
                // Scale.y is set when time changed
            }
            else {
                _holdBodySpriteRenderer.gameObject.SetActive(false);
            }
        }

        private void SetNotePositionZ(float time)
        {
            _game.AssertStageLoaded();

            float z = _game.ConvertNoteCoordTimeToWorldZ(time, NoteModel.Speed);
            transform.WithLocalPositionZ(z);
        }

        private void SetNoteSpriteAlpha()
        {
            _game.AssertStageLoaded();
            Debug.Assert(_state is NoteDisplayState.Fall);

            var appearAheadTime = AppearAheadTime;
            var noteFadeInEndTime = appearAheadTime * (1 - _game.Stage.Args.NoteFadeInRangePercent);

            var maxAlpha = _game.IsFilterNoteSpeed && !Mathf.Approximately(NoteModel.Speed, _game.HighlightedNoteSpeed)
                ? _game.Stage.Args.NoteDownplayAlpha
                : 1f;
            _noteColorAlpha = MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0, maxAlpha);

            _noteSpriteRenderer.WithColorAlpha(_noteColorAlpha);
        }

        private void SetLinkLine()
        {
            _game.AssertStageLoaded();

            if (_state is NoteDisplayState.Fall && _game.IsShowLinkLines && NoteModel.NextLink is not null) {
                var currentTime = _game.MusicPlayer.Time;

                var to = NoteModel.NextLink;
                var from = NoteModel;

                var (fromX, fromZ) = _game.ConvertNoteCoordToWorldPosition(from.PositionCoord - new NoteCoord(0f, currentTime), from.Speed);
                var (toX, toZ) = _game.ConvertNoteCoordToWorldPosition(to.PositionCoord - new NoteCoord(0f, currentTime), to.Speed);
                _linkLine = (new Vector2(fromX, fromZ), new Vector2(toX, toZ));
            }
            else {
                _linkLine = null;
            }
        }

        private void SetHoldBodyDisplayLength(float time)
        {
            _game.AssertStageLoaded();

            var scaleY = _game.ConvertNoteCoordTimeToHoldScaleY(time, NoteModel.Speed);
            _holdBodySpriteRenderer.transform.WithLocalScaleY(scaleY);
        }

        private void SetNoteSpriteColor()
        {
            _game.AssertStageLoaded();
            var stage = _game.Stage;

            if (NoteModel.IsSelected)
                _noteSpriteRenderer.WithColorRGB(stage.Args.NoteSelectedColor);
            else if (NoteModel.IsCollided)
                _noteSpriteRenderer.WithColorRGB(stage.Args.NoteCollidedColor);
            else
                _noteSpriteRenderer.WithColorRGB(Color.white);
        }

        private void SetAppearAheadTime0SuddenPlus(GameStageNoteController? previousStageNote)
        {
            _game.AssertStageLoaded();

            if (previousStageNote is null) {
                _appearAheadTime0SuddenPlus = _game.GetStageNoteActiveAheadTime(NoteModel.Speed);
                return;
            }

            var prevNoteAppearAheadTime = previousStageNote._appearAheadTime0SuddenPlus;
            var prevNoteAppearTime = previousStageNote.NoteModel.Time - prevNoteAppearAheadTime;
            var noteAppearTime = _game.GetStageNoteActiveTime(NoteModel);
            if (prevNoteAppearTime <= noteAppearTime) {
                _appearAheadTime0SuddenPlus = _game.GetStageNoteActiveAheadTime(NoteModel.Speed);
                return;
            }

            _appearAheadTime0SuddenPlus = NoteModel.Time - prevNoteAppearTime;
        }

        private void SetState()
        {
            var state = GetState();
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

        #endregion

        public enum NoteDisplayState
        {
            Inactive,
            Invisible,
            Fall,
            Holding,
            HitEffect,
        }
    }
}