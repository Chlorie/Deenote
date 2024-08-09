using Deenote.Edit;
using Deenote.GameStage.Elements;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.UI.Windows;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.GameStage
{
    public sealed partial class GameStageController : SingletonBehavior<GameStageController>
    {
        [field: SerializeField] public Camera PerspectiveCamera { get; private set; } = null!;
        [SerializeField] AudioSource _musicSource;

        [Header("Effect")]
        [SerializeField] SpriteRenderer _judgeLineBreathingEffectSpriteRenderer;
        [SerializeField] SpriteRenderer _judgeLineHitEffectSpriteRenderer;
        [SerializeField] Image _backgroundBreathingMaskImage;

        [Header("Notify")]
        [SerializeField] EditorController _editorController;
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;
        [SerializeField] PropertiesWindow _propertiesWindow;
        [SerializeField] PerspectiveViewController _perspectiveViewController;

        [Header("Prefabs")]
        [SerializeField] Transform _noteParentTransform;
        [SerializeField] StageNoteController _notePrefab;

        private PooledObjectListView<StageNoteController> _stageNotes;

        /// <summary>
        /// Index of the note that will next touch the 
        /// judgeline when player is playing forward.
        /// </summary>
        [Header("ReadOnly")]
        [SerializeField] private int _nextHitNoteIndex;
        /// <summary>
        /// Index of the note that will next appear when
        /// player is playing forward.
        /// </summary>
        [SerializeField] private int _nextAppearNoteIndex;
        /// <summary>
        /// Index of the note that will disappear next when
        /// player is playing forward
        /// </summary>
        [SerializeField] private int _nextDisappearNoteIndex;

        public int HittedNoteCount => Mathf.Max(0, _nextHitNoteIndex - 1);

        [Header("Settings")]
        private ChartModel _chart;

        private float __syncMusicTime;

        /// <remarks>
        /// If music is paused, maually set music time by this value,
        /// <br />
        /// If playing, this value should sync to _musicSource.velocity
        /// </remarks>
        [SerializeField] private float _manualPlaySpeedMultiplier;

        public GridController Grids => GridController.Instance;

        public PerspectiveViewController PerspectiveView => _perspectiveViewController;

        public ChartModel Chart => _chart;

        public bool IsActive => Chart is not null;

        public float StagePlaySpeed
        {
            get => IsMusicPlaying ? _musicSource.pitch : _manualPlaySpeedMultiplier;
            set
            {
                if (value == 0f)
                {
                    _musicSource.pitch = MusicSpeed / 10f;
                    _manualPlaySpeedMultiplier = 0f;
                }
                else
                {
                    _musicSource.pitch = _manualPlaySpeedMultiplier = value;
                }
            }
        }

        public float CurrentMusicTime
        {
            get => __syncMusicTime;
            set => SetMusicTime(value);
        }

        public bool IsMusicPlaying => _musicSource.isPlaying;

        public float MusicLength => _musicSource.clip.length;

        public float StageNoteAheadTime => _args.NotePanelLength / NoteSpeed * _perspectiveViewController.SuddenPlusRangeToVisibleRangePercent(SuddenPlusRange);

        #region Music State

        private void SetMusicTime(float time, bool syncMusicSource = true, bool notifyWindows = true, bool setStageNotes = true)
        {
            var prevTime = __syncMusicTime;
            var maxTime = MusicLength;
            time = Mathf.Clamp(time, 0f, maxTime);
            __syncMusicTime = time;
            if (syncMusicSource)
            {
                if (__syncMusicTime >= maxTime)
                {
                    // Do not set _musicSource.time when end, see Update() for reason
                    _musicSource.Pause();
                }
                else
                    _musicSource.time = __syncMusicTime;
            }
            if (setStageNotes)
            {
                UpdateStageNotesRelatively(forward: __syncMusicTime > prevTime);
                ForceUpdateNotesDisplay();
            }
            if (notifyWindows)
            {
                _perspectiveViewWindow.NotifyMusicTimeChanged(__syncMusicTime);
            }
        }

        public void PlayMusic()
        {
            if (IsMusicPlaying)
                return;

            if (CurrentMusicTime < MusicLength)
            {
                _musicSource.time = CurrentMusicTime;
                _musicSource.Play();
            }
            // Play froms start
            else
            {
                SetMusicTime(0f);
                _musicSource.Play();
            }
        }

        public void PauseMusic()
        {
            if (!IsMusicPlaying)
                return;
            _musicSource.Pause();
        }

        public void PauseStage()
        {
            PauseMusic();
            _manualPlaySpeedMultiplier = 0f;
        }

        public void ToggleMusicPlayingState()
        {
            if (IsMusicPlaying)
                PauseMusic();
            else
                PlayMusic();
        }

        #endregion

        public void LoadChart(ProjectModel project, int chartIndex)
        {
            var chart = project.Charts[chartIndex];

            _musicSource.Stop();
            _musicSource.clip = project.AudioClip;

            _chart = chart;

            _nextAppearNoteIndex = 0;
            _nextDisappearNoteIndex = 0;
            _nextHitNoteIndex = 0;

            CheckCollision();

            SetMusicTime(0f, setStageNotes: false);
            UpdateStageNotes();
            ForceUpdateNotesDisplay();

            _propertiesWindow.NotifyChartChanged(project, chartIndex);
            _perspectiveViewWindow.NotifyChartChanged(project, chart);

            void CheckCollision()
            {
                for (int i = 0; i < _chart.Notes.Count; i++)
                {
                    var note = _chart.Notes[i];
                    for (int j = i + 1; j < _chart.Notes.Count; j++)
                    {
                        var noteCmp = _chart.Notes[j];
                        if (!MainSystem.Args.IsTimeCollided(note.Data, noteCmp.Data))
                            break;

                        if (MainSystem.Args.IsPositionCollided(note.Data, noteCmp.Data)) {
                            note.CollisionCount++;
                            noteCmp.CollisionCount++;
                        }
                    }
                }
            }
        }

        #region Note

        private void ForceUpdateNotesDisplay()
        {
            foreach (var note in _stageNotes)
            {
                note.ForceUpdate(false);
            }
        }

        public void ForceUpdateStageNotes(bool notesOrderChanged, bool noteDataChangedExceptTime)
        {
            if (notesOrderChanged)
                UpdateStageNotes();
            foreach (var note in _stageNotes)
            {
                note.ForceUpdate(noteDataChangedExceptTime);
            }
        }

        /// <remarks>
        /// This is optimized version of <see cref="UpdateStageNotes"/>,
        /// If we know the time of previous update, we can iterate from
        /// cached indices
        /// </remarks>
        /// <param name="forward">
        /// <see langword="true"/> if current time is greater than time on previous update
        /// </param>
        private void UpdateStageNotesRelatively(bool forward)
        {
            if (forward) OnPlayForward();
            else OnPlayBackward();

            OnStageNoteUpdated();

            void OnPlayForward()
            {
                NoteTimeComparer.AssertInOrder(_stageNotes.Select(n => n.Model), "In forward");
                // Notes in _onStageNotes are sorted by time
                // so inactive notes always appears at leading of list
                var appearNoteTime = CurrentMusicTime + StageNoteAheadTime;
                var disappearNoteTime = CurrentMusicTime - Args.HitEffectSpritePrefabs.HitEffectTime;
                // Update _nextDisappear
                // Remove inactive notes
                int removeCount;
                for (removeCount = 0; removeCount < _stageNotes.Count; removeCount++)
                {
                    var note = _stageNotes[removeCount];
                    if (note.Model.Data.Time > disappearNoteTime)
                        break;
                }
                _stageNotes.RemoveRange(..removeCount);
                _nextDisappearNoteIndex += removeCount;
                // All notes on stage are removed, we need to continue iterating
                // to find the first note that should display
                if (_stageNotes.Count == 0)
                {
                    for (; _nextDisappearNoteIndex < _chart.Notes.Count; _nextDisappearNoteIndex++)
                    {
                        var note = _chart.Notes[_nextDisappearNoteIndex];
                        if (note.Data.Time > disappearNoteTime)
                        {
                            break;
                        }
                    }
                }

                // Update _nextHit, Add active note that playing hit effect
                if (_nextDisappearNoteIndex > _nextHitNoteIndex)
                    _nextHitNoteIndex = _nextDisappearNoteIndex;
                for (; _nextHitNoteIndex < _chart.Notes.Count; _nextHitNoteIndex++)
                {
                    var note = _chart.Notes[_nextHitNoteIndex];
                    if (note.Data.Time > CurrentMusicTime)
                    {
                        break;
                    }
                }

                // Update _nextAppear
                // Add active notes
                // If new _nextDiappear is greater, we search start from _nextDisappear
                if (_nextDisappearNoteIndex > _nextAppearNoteIndex)
                {
                    _nextAppearNoteIndex = _nextDisappearNoteIndex;
                }
                for (; _nextAppearNoteIndex < _chart.Notes.Count; _nextAppearNoteIndex++)
                {
                    var note = _chart.Notes[_nextAppearNoteIndex];
                    if (note.Data.Time > appearNoteTime)
                    {
                        break;
                    }
                    _stageNotes.Add(out var noteController);
                    noteController.Initialize(note);
                }
            }

            void OnPlayBackward()
            {
                NoteTimeComparer.AssertInOrder(_stageNotes.Select(n => n.Model), "In backward");
                var appearNoteTime = CurrentMusicTime + StageNoteAheadTime;
                var disappearNoteTime = CurrentMusicTime - Args.HitEffectSpritePrefabs.HitEffectTime;
                // Remove trailing inactive notes
                int removeStart;
                for (removeStart = _stageNotes.Count - 1; removeStart >= 0; removeStart--)
                {
                    var note = _stageNotes[removeStart];
                    if (note.Model.Data.Time <= appearNoteTime)
                        break;
                }
                removeStart++;
                var removeCount = _stageNotes.Count - removeStart;
                _stageNotes.RemoveRange(removeStart..);
                _nextAppearNoteIndex -= removeCount;
                // All notes on stage removed, continue iterating for display
                if (_stageNotes.Count == 0)
                {
                    var prevAppearNoteIndex = _nextAppearNoteIndex - 1;
                    for (; prevAppearNoteIndex >= 0; prevAppearNoteIndex--)
                    {
                        var note = _chart.Notes[prevAppearNoteIndex];
                        if (note.Data.Time <= appearNoteTime)
                            break;
                    }
                    _nextAppearNoteIndex = prevAppearNoteIndex + 1;
                }

                // Update _nextHit
                if (_nextAppearNoteIndex < _nextHitNoteIndex)
                    _nextHitNoteIndex = _nextAppearNoteIndex;
                var prevHitNoteIndex = _nextHitNoteIndex - 1;
                for (; prevHitNoteIndex >= 0; prevHitNoteIndex--)
                {
                    var note = _chart.Notes[prevHitNoteIndex];
                    if (note.Data.Time <= CurrentMusicTime)
                    {
                        break;
                    }
                }
                _nextHitNoteIndex = prevHitNoteIndex + 1;

                if (_nextAppearNoteIndex < _nextDisappearNoteIndex)
                    _nextDisappearNoteIndex = _nextAppearNoteIndex;
                var prevDisappearNoteIndex = _nextDisappearNoteIndex - 1;
                for (; prevDisappearNoteIndex >= 0; prevDisappearNoteIndex--)
                {
                    var note = _chart.Notes[prevDisappearNoteIndex];
                    if (note.Data.Time <= disappearNoteTime)
                    {
                        break;
                    }
                    _stageNotes.Insert(0, out var noteController);
                    noteController.Initialize(note);
                }
                _nextDisappearNoteIndex = prevDisappearNoteIndex + 1;
            }
        }

        /// <summary>
        /// Find notes that should display on stage, this method won't update note display,
        /// if music is not playing, you should manually call <see cref="ForceUpdateNotesDisplay"/>
        /// </summary>
        private void UpdateStageNotes()
        {
            using (var stageNotes = _stageNotes.Resetting())
            {
                var appearNoteTime = CurrentMusicTime + StageNoteAheadTime;
                var disappearNoteTime = CurrentMusicTime - Args.HitEffectSpritePrefabs.HitEffectTime;
                int i = 0;
                for (; i < _chart.Notes.Count; i++)
                {
                    var note = _chart.Notes[i];
                    if (note.Data.Time > disappearNoteTime)
                    {
                        break;
                    }
                }
                _nextDisappearNoteIndex = i;

                for (; i < _chart.Notes.Count; i++)
                {
                    var note = _chart.Notes[i];
                    if (note.Data.Time > CurrentMusicTime)
                    {
                        break;
                    }
                    stageNotes.Add(out var noteController);
                    noteController.Initialize(note);
                }
                _nextHitNoteIndex = i;

                for (; i < _chart.Notes.Count; i++)
                {
                    var note = _chart.Notes[i];
                    if (note.Data.Time > appearNoteTime)
                    {
                        break;
                    }
                    stageNotes.Add(out var noteController);
                    noteController.Initialize(note);
                }
                _nextAppearNoteIndex = i;

            }

            OnStageNoteUpdated();
        }

        #endregion

        #region Effect

        private void UpdateStageEffect()
        {
            var time = Time.time;
            // Judgeline
            {
                var ratio = Mathf.Sin(time * (2f * Mathf.PI / _args.JudgeLinePeriod));
                ratio = Mathf.InverseLerp(-1f, 1f, ratio);
                _judgeLineBreathingEffectSpriteRenderer.color = Color.white.WithAlpha(ratio);
            }

            // Background
            {
                var ratio = Mathf.Sin(time * (2f * Mathf.PI / _args.BackgroundMaskPeriod));
                ratio = Mathf.InverseLerp(-1f, 1f, ratio);
                _backgroundBreathingMaskImage.color = Color.white
                    .WithAlpha(Mathf.Lerp(_args.BackgroundMaskMinAlpha, _args.BackgroundMaskMaxAlpha, ratio));
            }

            // if (lightEffectState) stageLight.intensity = 2.5f + 2.5f * Mathf.Sin(2 * currentTime);

            // Note: DEEMO 4.x have different background effect from 3.x,
            // The above code is the effect that Chlorie made, which is more like 3.x ver, so we remain it here.
            // Also the commented line in StopBackgroundBreatheEffect()

            // Related game objects: Stage.Spotlight, Stage.StagePlane, 
            // Related material: White

            // Note: Change scale of bg image to simulate 3.x effect, the above code
            // is no longer needed.
        }

        private void StopStageEffect()
        {
            _judgeLineBreathingEffectSpriteRenderer.color = Color.clear;
            _backgroundBreathingMaskImage.color = Color.white;
        }

        private void UpdateJudgeLineHitEffect()
        {
            var noteIndex = _nextHitNoteIndex - 1;
            if (noteIndex < 0) {
                _judgeLineHitEffectSpriteRenderer.color = Color.clear;
                return;
            }
            var hitTime = _chart.Notes[_nextHitNoteIndex - 1].Data.Time;
            var deltaTime = CurrentMusicTime - hitTime;
            Debug.Assert(deltaTime >= 0);

            float alpha;
            if (deltaTime < _args.JudgeLineHitEffectAlphaDecTime) {
                float x = deltaTime / _args.JudgeLineHitEffectAlphaDecTime;
                alpha = Mathf.Pow(1 - x, 0.5f);
            }
            else {
                alpha = 0f;
            }
            _judgeLineHitEffectSpriteRenderer.color = Color.white.WithAlpha(alpha);
        }

        #endregion

        private void OnStageNoteUpdated()
        {
            UpdateJudgeLineHitEffect();
            GridController.Instance.NotifyGameStageProgressChanged();
            _perspectiveViewWindow.NotifyGameStageProgressChanged(_nextHitNoteIndex);
        }

        protected override void Awake()
        {
            base.Awake();
            _stageNotes = new PooledObjectListView<StageNoteController>(
                UnityUtils.CreateObjectPool(_notePrefab, _noteParentTransform));
        }

        private void Start()
        {
            IsPianoNotesDistinguished = true;

            // TODO: Fake
            IsStageEffectOn = true;
        }

        private void Update()
        {
            if (IsStageEffectOn) UpdateStageEffect();

            if (IsActive) {

                if (IsMusicPlaying || _musicSource.time >= MusicLength) { // Music playing or play to end

                    // Note that there's one frame when _musicSource.time == _musicSource.clip.length while audio playing,
                    // and SetMusicTime wont set _musicSource.Time to clip.length.
                    // So when _musicSource.time == clip.length, it means the music is playing to end,
                    // not manually set to end
                    SetMusicTime(_musicSource.time, syncMusicSource: false, setStageNotes: false);
                }
                else if (_manualPlaySpeedMultiplier is not 0f) { // Manually playing
                    var newTime = CurrentMusicTime + Time.deltaTime * _manualPlaySpeedMultiplier;
                    SetMusicTime(newTime, setStageNotes: false);
                }

                if (StagePlaySpeed != 0)
                    UpdateStageNotesRelatively(StagePlaySpeed > 0);
            }
        }
    }
}