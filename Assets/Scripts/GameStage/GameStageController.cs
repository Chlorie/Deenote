using Deenote.Edit;
using Deenote.GameStage.Elements;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.UI.Windows;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.Linq;
using UnityEngine;

namespace Deenote.GameStage
{
    public sealed partial class GameStageController : SingletonBehavior<GameStageController>
    {
        [field: SerializeField] public Camera Camera { get; private set; } = null!;
        [SerializeField] AudioSource _musicSource;

        [Header("Effect")]
        [SerializeField] Transform _judgeLineBreathingEffectTransform;
        [SerializeField] Transform _judgeLineHitEffectTransform;

        [Header("Notify")]
        [SerializeField] EditorController _editorController;
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;

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

        [Header("Settings")]
        private ChartModel _chart;

        private float __syncMusicTime;

        /// <remarks>
        /// If music is paused, maually set music time by this value,
        /// <br />
        /// If playing, this value should sync to _musicSource.velocity
        /// </remarks>
        [SerializeField] private float _manualPlaySpeedMultiplier;
        [SerializeField] private bool __isStageEffectOn;
        [SerializeField, Range(1, 19)] private int __noteSpeed;
        [SerializeField, Range(1, 30)] private int __musicSpeed;
        [SerializeField, Range(0, 100)] private int __effectVolume;
        [SerializeField, Range(0, 100)] private int __musicVolume;
        [SerializeField, Range(0, 100)] private int __pianoVolume;
        [SerializeField, Range(0f, 100f)] private int __suddenPlusRange;

        public GridController Grids => GridController.Instance;

        public ChartModel Chart => _chart;

        private Plane NotePanelPlane => new(Vector3.up, 0f);

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

        public bool IsStageEffectOn
        {
            get => __isStageEffectOn;
            set
            {
                if (__isStageEffectOn == value)
                    return;
                __isStageEffectOn = value;
                if (__isStageEffectOn is false)
                    StopStageEffect();
            }
        }

        public float StageNoteAheadTime => 9f / NoteSpeed * (100 - SuddenPlusRange) / 100f;

        /// <summary>
        /// Range [1, 19], display [0.5, 9.5]
        /// </summary>
        public int NoteSpeed
        {
            get => __noteSpeed;
            set
            {
                value = Mathf.Clamp(value, 1, 19);

                if (__noteSpeed == value)
                    return;
                __noteSpeed = value;
                UpdateStageNotes();
                _editorPropertiesWindow.NotifyNoteSpeedChanged(__noteSpeed);
            }
        }

        /// <summary>
        /// Range [1, 30], display [0.1, 3.0]
        /// </summary>
        public int MusicSpeed
        {
            get => __musicSpeed;
            set
            {
                value = Mathf.Clamp(value, 1, 30);

                if (__musicSpeed == value)
                    return;
                __musicSpeed = value;
                _musicSource.pitch = value / 10f;
                _editorPropertiesWindow.NotifyMusicSpeedChanged(__musicSpeed);
            }
        }

        /// <summary>
        /// Range [0, 100]
        /// </summary>
        public int EffectVolume
        {
            get => __effectVolume;
            set
            {
                value = Mathf.Clamp(value, 0, 100);

                if (__effectVolume == value)
                    return;
                __effectVolume = value;
                _musicSource.volume = __effectVolume / 100f;
                _editorPropertiesWindow.NotifyEffectVolumeChanged(__effectVolume);
            }
        }

        /// <summary>
        /// Range [0, 100]
        /// </summary>
        public int MusicVolume
        {
            get => __musicVolume;
            set
            {
                value = Mathf.Clamp(value, 0, 100);

                if (__musicVolume == value)
                    return;
                __musicVolume = value;
                _musicSource.volume = __musicVolume / 100f;
                _editorPropertiesWindow.NotifyMusicVolumeChanged(__musicVolume);
            }
        }

        /// <summary>
        /// Range [0, 100]
        /// </summary>
        public int PianoVolume
        {
            get => __pianoVolume;
            set
            {
                value = Mathf.Clamp(value, 0, 100);

                if (__pianoVolume == value)
                    return;
                __pianoVolume = value;
                _musicSource.volume = __pianoVolume / 100f;
                _editorPropertiesWindow.NotifyPianoVolumeChanged(__pianoVolume);
            }
        }

        /// <summary>
        /// Range [0, 100], means percent
        /// </summary>
        public int SuddenPlusRange
        {
            get => __suddenPlusRange;
            set
            {
                value = Mathf.Clamp(value, 0, 100);

                if (__suddenPlusRange == value)
                    return;
                __suddenPlusRange = value;
                UpdateStageNotes();
                _editorPropertiesWindow.NotifySuddenPlusRangeChanged(__suddenPlusRange);
            }
        }

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
            }
            if (notifyWindows)
            {
                _perspectiveViewWindow.NotifyMusicTimeChanged(__syncMusicTime);
            }
            ForceUpdateNotesDisplay();
        }

        public void Play()
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

        public void Pause()
        {
            if (!IsMusicPlaying)
                return;
            _musicSource.Pause();
        }

        public void TogglePlayingState()
        {
            if (IsMusicPlaying)
                Pause();
            else
                Play();
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

            _perspectiveViewWindow.NotifyChartChanged(project, chart);

            void CheckCollision()
            {
                for (int i = 0; i < _chart.Notes.Count; i++)
                {
                    var note = _chart.Notes[i];
                    for (int j = i + 1; j < _chart.Notes.Count; j++)
                    {
                        var noteCmp = _chart.Notes[j];
                        if (MainSystem.Args.IsCollided(note.Data, noteCmp.Data))
                        {
                            note.IsCollided = true;
                            noteCmp.IsCollided = true;
                        }
                        else
                        {
                            break;
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
        /// cached indices to save times
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
                var disappearNoteTime = CurrentMusicTime - HitEffectSpritePrefabs.HitEffectTime;
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
                var disappearNoteTime = CurrentMusicTime - HitEffectSpritePrefabs.HitEffectTime;
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
                var disappearNoteTime = CurrentMusicTime - HitEffectSpritePrefabs.HitEffectTime;
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
            #region Legacy
            //if (_onStageNotes.Count > 0) {
            //    foreach (var note in _onStageNotes)
            //        _stageNotePool.Release(note);
            //    _onStageNotes.Clear();
            //}

            //var appearNoteTime = CurrentMusicTime + StageNoteAheadTime;
            //var disappearNoteTime = CurrentMusicTime - HitEffectSpritePrefabs.HitEffectTime;
            //int i = 0;
            //for (; i < _chart.Notes.Count; i++) {
            //    var note = _chart.Notes[i];
            //    if (note.Data.Time > disappearNoteTime) {
            //        break;
            //    }
            //}
            //_nextDisappearNoteIndex = i;

            //for (; i < _chart.Notes.Count; i++) {
            //    var note = _chart.Notes[i];
            //    if (note.Data.Time > CurrentMusicTime) {
            //        break;
            //    }

            //    var noteController = _stageNotePool.Get();
            //    noteController.Initialize(note);
            //    _onStageNotes.Add(noteController);
            //}
            //_nextHitNoteIndex = i;

            //for (; i < _chart.Notes.Count; i++) {
            //    var note = _chart.Notes[i];
            //    if (note.Data.Time > appearNoteTime) {
            //        break;
            //    }

            //    var noteController = _stageNotePool.Get();
            //    noteController.Initialize(note);
            //    _onStageNotes.Add(noteController);
            //}
            //_nextAppearNoteIndex = i;

            #endregion

            OnStageNoteUpdated();
        }

        #endregion

        #region Effect

        private void UpdateStageEffect()
        {
            const float JudgeLinePeriod = 3.5f;
            var ratio = Mathf.Sin(Time.time * (2 * Mathf.PI / JudgeLinePeriod));
            _judgeLineBreathingEffectTransform.localScale = new(2f, (ratio + 1f) / 4f, 1f);

            _perspectiveViewWindow.NotifyStageEffectUpdate(true);

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
            _judgeLineBreathingEffectTransform.localScale = new Vector3(2f, 0f, 1f);
            _perspectiveViewWindow.NotifyStageEffectUpdate(false);
        }

        private void UpdateJudgeLineHitEffect()
        {
            var noteIndex = _nextHitNoteIndex - 1;
            if (noteIndex < 0)
                goto Reset;
            var hitTime = _chart.Notes[_nextHitNoteIndex - 1].Data.Time;
            var deltaTime = CurrentMusicTime - hitTime;
            Debug.Assert(deltaTime >= 0);

            const float EffectDecTime = 1f;
            const float EffectMaxScale = 2f;
            if (deltaTime < EffectDecTime)
            {
                var ratio = 1 - deltaTime / EffectDecTime;
                _judgeLineHitEffectTransform.localScale = new(2f, ratio * EffectMaxScale, 1f);
                return;
            }

        Reset:
            _judgeLineHitEffectTransform.localScale = Vector3.zero;
        }

        #endregion

        private void OnStageNoteUpdated()
        {
            UpdateJudgeLineHitEffect();
            GridController.Instance.NotifyGameStageProgressChanged();
            _perspectiveViewWindow.NotifyGameStageProgressChanged(_nextHitNoteIndex);
        }

        public bool TryConvertViewPointToNoteCoord(Vector3 viewPoint, out NoteCoord coord)
        {
            Ray ray = Camera.ViewportPointToRay(viewPoint);
            if (NotePanelPlane.Raycast(ray, out var distance))
            {
                var hitp = ray.GetPoint(distance);
                coord = new(MainSystem.Args.XToPosition(hitp.x), MainSystem.Args.ZToOffsetTime(hitp.z) + CurrentMusicTime);
                return true;
            }
            coord = default;
            return false;
        }

        protected override void Awake()
        {
            base.Awake();
            _stageNotes = new PooledObjectListView<StageNoteController>(
                UnityUtils.CreateObjectPool(_notePrefab, _noteParentTransform));
        }

        private void Start()
        {
            IsStageEffectOn = true;
            LoadChart(Fake.Project, 0);
        }

        private void Update()
        {
            if (IsStageEffectOn) UpdateStageEffect();

            if (IsMusicPlaying || _musicSource.time >= MusicLength)
            { // Music playing or play to end
                // Note that there's one frame when _musicSource.time == _musicSource.clip.length while audio playing,
                // and SetMusicTime wont set _musicSource.Time to clip.length.
                // So when _musicSource.time == clip.length, it means the music is playing to end,
                // not manually set to end
                SetMusicTime(_musicSource.time, syncMusicSource: false, setStageNotes: false);
            }
            else if (_manualPlaySpeedMultiplier is not 0f)
            { // Manually playing
                var newTime = CurrentMusicTime + Time.deltaTime * _manualPlaySpeedMultiplier;
                SetMusicTime(newTime, setStageNotes: false);
            }

            if (StagePlaySpeed != 0)
                UpdateStageNotesRelatively(StagePlaySpeed > 0);
        }
    }
}