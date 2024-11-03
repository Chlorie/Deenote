#nullable enable

using Deenote.Audio;
using Deenote.GameStage.Elements;
using Deenote.Project.Models;
using Deenote.UI.ComponentModel;
using Deenote.Utilities;
using Reflex.Attributes;
using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Deenote.GameStage
{
    public sealed partial class GameStageController : SingletonBehavior<GameStageController>, INotifyPropertyChange<GameStageController, GameStageController.NotifyProperty>
    {
        [Header("Effect")]
        [SerializeField] SpriteRenderer _judgeLineBreathingEffectSpriteRenderer;
        [SerializeField] SpriteRenderer _judgeLineHitEffectSpriteRenderer;
        [SerializeField] Image _backgroundBreathingMaskImage;
        [SerializeField] PerspectiveViewController _perspectiveViewController;

        [Header("Prefabs")]
        [SerializeField] Transform _noteParentTransform;
        [SerializeField] StageNoteController _notePrefab;

        [SerializeField] private StageNoteManager _stageNoteManager;

        [Header("Settings")]
        private ChartModel _chart;

        /// <remarks>
        /// If music is paused, maually set music time by this value,
        /// <br />
        /// If playing, this value should sync to _musicSource.velocity
        /// </remarks>
        [SerializeField] private float _manualPlaySpeedMultiplier;

        [Inject] private MusicController _musicController = null!;

        public GridController Grids => GridController.Instance;

        public MusicController MusicController => _musicController;

        public PerspectiveViewController PerspectiveView => _perspectiveViewController;

        /// <summary>
        /// Maybe null if ProjectManager.CurrentProject is null
        /// </summary>
        public ChartModel Chart => _chart;

        public bool IsActive => Chart is not null;

        public float StagePlaySpeed
        {
            get => IsMusicPlaying ? _musicController.Pitch : _manualPlaySpeedMultiplier;
            set {
                if (value == 0f) {
                    _musicController.Pitch = MusicSpeed / 10f;
                    _manualPlaySpeedMultiplier = 0f;
                }
                else {
                    _musicController.Pitch = _manualPlaySpeedMultiplier = value;
                }
            }
        }

        public float CurrentMusicTime => _musicController.Time;

        public bool IsMusicPlaying => _musicController.IsPlaying;

        public float MusicLength => _musicController.Length;

        public float StageNoteAheadTime =>
            _args.NotePanelLength / NoteSpeed *
            _perspectiveViewController.SuddenPlusRangeToVisibleRangePercent(SuddenPlusRange);

        #region Music State

        private void OnMusicTimeChanged(float oldTime, float newTime, bool isManuallyChanged)
        {
            SearchForNotesOnStage(oldTime, newTime);
            if (isManuallyChanged) ForceUpdateNotesDisplay();
        }

        public void PauseStage()
        {
            _musicController.Stop();
            _manualPlaySpeedMultiplier = 0f;
        }

        #endregion

        public void LoadChartInCurrentProject(ChartModel? chart)
        {
            Debug.Assert(chart is null || MainSystem.ProjectManager.CurrentProject.Charts.Contains(chart));

            _musicController.Stop();
            _musicController.Time = 0f;
            _chart = chart!;
            _stageNoteManager.ResetIndices();

            if (chart is not null) {
                CheckCollision();
                SearchForNotesFromStart();
            }

            ForceUpdateNotesDisplay();
            _propertyChangeNotifier.Invoke(this, NotifyProperty.CurrentChart);

            void CheckCollision()
            {
                for (int i = 0; i < _chart.Notes.Count; i++) {
                    if (_chart.Notes[i] is not NoteModel note)
                        continue;

                    for (int j = i + 1; j < _chart.Notes.Count; j++) {
                        if (_chart.Notes[j] is not NoteModel noteCmp)
                            continue;

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
            foreach (var note in _stageNoteManager.OnStageNotes) {
                note.ForceUpdate(false);
            }
            foreach (var note in _stageNoteManager.TrackingNotes) {
                note.ForceUpdate(false);
            }
        }

        public void ForceUpdateStageNotes(bool notesOrderChanged, bool noteDataChangedExceptTime)
        {
            if (notesOrderChanged)
                SearchForNotesFromStart();
            //UpdateStageNotes();
            foreach (var note in _stageNoteManager.OnStageNotes) {
                note.ForceUpdate(noteDataChangedExceptTime);
            }
            foreach (var note in _stageNoteManager.TrackingNotes) {
                note.ForceUpdate(noteDataChangedExceptTime);
            }
        }

        /// <remarks>
        /// This is optimized version of <see cref="SearchForNotesFromStart"/>,
        /// If we know the time of previous update, we can iterate from
        /// cached indices
        /// </remarks>
        /// <param name="forward">
        /// <see langword="true"/> if current time is greater than time on previous update
        /// </param>
        private void SearchForNotesOnStage(float oldTime, float newTime)
        {
            Debug.Assert(newTime == CurrentMusicTime);

            if (newTime > oldTime) OnPlayForward();
            else OnPlayBackward();

            OnStageNoteUpdated();
            return;

            void OnPlayForward()
            {
                _stageNoteManager.AssertOnStageNotesInOrder("In forward");

                // Notes in _onStageNotes are sorted by time
                // so inactive notes always appears at leading of list
                var appearNoteTime = newTime + StageNoteAheadTime;
                var disappearNoteTime = newTime - Args.HitEffectSpritePrefabs.HitEffectTime;
                var old_disappearNoteTime = oldTime - Args.HitEffectSpritePrefabs.HitEffectTime;

                // Update NextDisappear
                // Remove inactive notes

                int newNextDisappearNoteIndex = _stageNoteManager.NextDisappearNoteIndex;
                IterateNotesUntil(ref newNextDisappearNoteIndex, disappearNoteTime);

                // Remove all notes on stage
                if (newNextDisappearNoteIndex >= _stageNoteManager.NextAppearNoteIndex) {
                    _stageNoteManager.RemoveOnStageNotes(Range.All);
                    _stageNoteManager.RemoveAllTrackingNotes(n => n.Model.Data.EndTime <= disappearNoteTime);
                }
                // Remove notes in [old DisappearIndex..new DisappearIndex]
                else {
                    int iController = 0;
                    for (int i = _stageNoteManager.NextDisappearNoteIndex; i < newNextDisappearNoteIndex; i++) {
                        IStageNoteModel note = _chart.Notes[i];
                        if (note is NoteTailModel noteTail) {
                            // For note tail
                            NoteModel noteHead = noteTail.HeadModel;
                            //   - Note head is in _trackingNotes, remove it
                            if (noteHead.Data.Time <= old_disappearNoteTime) {
                                _stageNoteManager.RemoveTrackingNote(noteHead);
                            }
                            //   - Note head is removed in this loop, do nothing.
                            else { }
                        }
                        else {
                            Debug.Assert(note is NoteModel);
                            // For note model
                            NoteModel noteModel = (NoteModel)note;
                            //   - Hold note. Check if tail is still on stage, if true move controller to _keepingNotes;
                            if (noteModel.Data.EndTime > disappearNoteTime) {
                                _stageNoteManager.MoveNoteToTracking_NonRemove(iController);
                            }
                            //   - Hold note, if tail will be removed, remove this controller,
                            //   - Note is not hold, just remove this controller.
                            else { /* Remove outside the for loop */}
                            iController++;
                        }
                    }
                    _stageNoteManager.RemoveOnStageNotesWithTrackingCheck(..iController);
                }
                _stageNoteManager.NextDisappearNoteIndex = newNextDisappearNoteIndex;

                // Update NextHit & Combo

                int newNextHitNoteIndex = Math.Max(
                    _stageNoteManager.NextDisappearNoteIndex,
                    _stageNoteManager.NextHitNoteIndex);
                IterateNotesUntil(ref newNextHitNoteIndex, CurrentMusicTime);
                int newCombo = _stageNoteManager.CurrentCombo;
                for (int i = _stageNoteManager.NextHitNoteIndex; i < newNextHitNoteIndex; i++) {
                    var note = _chart.Notes[i];
                    if (note.IsComboNote())
                        _stageNoteManager.CurrentCombo++;
                }
                _stageNoteManager.NextHitNoteIndex = newNextHitNoteIndex;

                // Update NextAppear
                // Add active notes

                int newNextAppearNoteIndex = Math.Max(
                    _stageNoteManager.NextDisappearNoteIndex,
                    _stageNoteManager.NextAppearNoteIndex);
                int appendStartIndex = newNextAppearNoteIndex;
                IterateNotesUntil(ref newNextAppearNoteIndex, appearNoteTime);

                for (int i = appendStartIndex; i < newNextAppearNoteIndex; i++) {
                    IStageNoteModel note = _chart.Notes[i];
                    if (note is NoteTailModel noteTail) {
                        // For note tail
                        NoteModel noteHead = noteTail.HeadModel;
                        // If newDisappear > oldAppear, then hold notes
                        // in [old Appear.. new Disappear] may match this branch
                        if (_stageNoteManager.NextDisappearNoteIndex > _stageNoteManager.NextAppearNoteIndex) {
                            //   - Note head < disappearTime, should in _trackingNotes 
                            if (noteHead.Data.Time <= disappearNoteTime) {
                                _stageNoteManager.AddTrackingNote(noteHead);
                            }
                            // If note head is on the new stage, do nothing.
                            // The head should have been added to _onStageNotes;
                            else { }
                        }
                        // Else, all found hold tail's head model is in _trackingNotes or _onStageNotes
                        else { }
                    }
                    else {
                        Debug.Assert(note is NoteModel);
                        // For note model
                        NoteModel noteModel = (NoteModel)note;
                        _stageNoteManager.AddOnStageNote(noteModel);
                    }
                }
                _stageNoteManager.NextAppearNoteIndex = newNextAppearNoteIndex;

                void IterateNotesUntil(ref int index, float compareTime)
                {
                    for (; index < _chart.Notes.Count; index++) {
                        var note = _chart.Notes[index];
                        if (note.Time > compareTime)
                            break;
                    }
                }
            }

            void OnPlayBackward()
            {
                _stageNoteManager.AssertOnStageNotesInOrder("In backward");

                var appearNoteTime = CurrentMusicTime + StageNoteAheadTime;
                var disappearNoteTime = CurrentMusicTime - Args.HitEffectSpritePrefabs.HitEffectTime;

                // Update NextAppear

                int newNextAppearNoteIndex = _stageNoteManager.NextAppearNoteIndex;
                IterateNotesUntil(ref newNextAppearNoteIndex, appearNoteTime);

                // Remove all notes on stage
                if (newNextAppearNoteIndex <= _stageNoteManager.NextDisappearNoteIndex) {
                    _stageNoteManager.RemoveOnStageNotes(Range.All);
                    _stageNoteManager.RemoveAllTrackingNotes(n => n.Model.Data.Time >= appearNoteTime);
                }
                // Remove notes in [new AppearIndex..old AppearIndex]
                else {
                    int iController = _stageNoteManager.OnStageNotes.Length - 1;
                    for (int i = _stageNoteManager.NextAppearNoteIndex - 1; i >= newNextAppearNoteIndex; i--) {
                        IStageNoteModel note = _chart.Notes[i];
                        if (note is NoteModel) {
                            iController--;
                        }
                    }
                    _stageNoteManager.RemoveOnStageNotes((iController + 1)..);
                }
                _stageNoteManager.NextAppearNoteIndex = newNextAppearNoteIndex;

                // Update NextHit & Combo

                int newNextHitNoteIndex = Math.Min(
                    _stageNoteManager.NextAppearNoteIndex,
                    _stageNoteManager.NextHitNoteIndex);
                IterateNotesUntil(ref newNextHitNoteIndex, CurrentMusicTime);
                int newCombo = _stageNoteManager.CurrentCombo;
                for (int i = _stageNoteManager.NextHitNoteIndex - 1; i >= newNextHitNoteIndex; i--) {
                    var note = _chart.Notes[i];
                    if (note.IsComboNote())
                        _stageNoteManager.CurrentCombo--;
                }
                _stageNoteManager.NextHitNoteIndex = newNextHitNoteIndex;

                // Update NextDisappear

                int newNextDisappearNoteIndex = Math.Min(
                    _stageNoteManager.NextAppearNoteIndex,
                    _stageNoteManager.NextDisappearNoteIndex);
                int prependStartIndex = newNextDisappearNoteIndex;
                IterateNotesUntil(ref newNextDisappearNoteIndex, disappearNoteTime);

                using var _n = ListPool<NoteModel>.Get(out var buffer);
                for (int i = prependStartIndex - 1; i >= newNextDisappearNoteIndex; i--) {
                    IStageNoteModel note = _chart.Notes[i];
                    if (note is NoteTailModel noteTail) {
                        // For note tail
                        NoteModel noteHead = noteTail.HeadModel;
                        // - Note head is < disappearTime, add into _trackingNotes
                        if (noteHead.Data.Time <= disappearNoteTime) {
                            _stageNoteManager.AddTrackingNote(noteHead);
                        }
                        // - Note head is on new stage, do nothing,
                        //   The head should will be added to _onStageNotes
                        else { }
                    }
                    else {
                        // For Note Head
                        Debug.Assert(note is NoteModel);
                        NoteModel noteModel = (NoteModel)note;
                        buffer.Add(noteModel);
                    }
                }
                _stageNoteManager.PrependOnStageNotes(buffer.AsSpan());
                _stageNoteManager.NextDisappearNoteIndex = newNextDisappearNoteIndex;

                void IterateNotesUntil(ref int index, float compareTime)
                {
                    index--;
                    for (; index >= 0; index--) {
                        var note = _chart.Notes[index];
                        if (note.Time <= compareTime) {
                            break;
                        }
                    }
                    index++;
                }
            }
        }

        /// <summary>
        /// Find notes that should display on stage, this method won't update note display,
        /// if music is not playing, you should manually call <see cref="ForceUpdateNotesDisplay"/>
        /// </summary>
        private void SearchForNotesFromStart()
        {
            _stageNoteManager.ClearAll();

            var appearNoteTime = CurrentMusicTime + StageNoteAheadTime;
            var disappearNoteTime = CurrentMusicTime - Args.HitEffectSpritePrefabs.HitEffectTime;
            int index = 0;
            int combo = 0;

            for (; index < _chart.Notes.Count; index++) {
                var note = _chart.Notes[index];
                if (note.Time > disappearNoteTime)
                    break;
                AdjustCombo(note);
            }
            _stageNoteManager.NextDisappearNoteIndex = index;

            for (; index < _chart.Notes.Count; index++) {
                var note = _chart.Notes[index];
                if (note.Time > CurrentMusicTime)
                    break;
                AdjustCombo(note);
                AdjustDisplay(note);
            }
            _stageNoteManager.NextHitNoteIndex = index;
            _stageNoteManager.CurrentCombo = combo;

            for (; index < _chart.Notes.Count; ++index) {
                var note = _chart.Notes[index];
                if (note.Time > appearNoteTime)
                    break;
                AdjustDisplay(note);
            }
            _stageNoteManager.NextAppearNoteIndex = index;

            OnStageNoteUpdated();

            void AdjustCombo(IStageNoteModel note)
            {
                if (note.IsComboNote())
                    combo++;
            }

            void AdjustDisplay(IStageNoteModel note)
            {
                if (note is NoteTailModel noteTail) {
                    NoteModel noteHead = noteTail.HeadModel;
                    if (noteHead.Data.Time <= disappearNoteTime)
                        _stageNoteManager.AddTrackingNote(noteHead);
                    // Handled in another branch
                    else { }
                }
                else {
                    Debug.Assert(note is NoteModel);
                    NoteModel noteModel = (NoteModel)note;
                    _stageNoteManager.AddOnStageNote(noteModel);
                }
            }
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
                _judgeLineBreathingEffectSpriteRenderer.color = Color.white with { a = ratio };
            }

            // Background
            {
                var ratio = Mathf.Sin(time * (2f * Mathf.PI / _args.BackgroundMaskPeriod));
                ratio = Mathf.InverseLerp(-1f, 1f, ratio);
                _backgroundBreathingMaskImage.color = Color.white with { a = Mathf.Lerp(_args.BackgroundMaskMinAlpha, _args.BackgroundMaskMaxAlpha, ratio) };
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
            var previousHitNote = GetPreviousHitNote();
            if (previousHitNote is null) {
                _judgeLineHitEffectSpriteRenderer.color = Color.clear;
                return;
            }
            var hitTime = previousHitNote.Data.Time;
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
            _judgeLineHitEffectSpriteRenderer.color = Color.white with { a = alpha };

            NoteModel? GetPreviousHitNote()
            {
                for (int i = _stageNoteManager.NextHitNoteIndex - 1; i >= 0; i--) {
                    if (_chart.Notes[i] is NoteModel noteModel) {
                        return noteModel;
                    }
                }
                return null;
            }
        }

        #endregion

        public int CurrentCombo => _stageNoteManager.CurrentCombo;

        /// <summary>
        /// Non-hold's NoteModel or hold's NoteTailModel that just reached judge line,
        /// <see langword="null"/> if current combo is 0
        /// </summary>
        public IStageNoteModel? PrevHitNote
        {
            get {
                // Most of time the first note is the result, so the for loop is acceptable
                // When a hold head just reached judge line, it may require iteration.
                for (int i = _stageNoteManager.NextHitNoteIndex - 1; i >= 0; i--) {
                    var note = _chart.Notes[i];
                    if (note.IsComboNote())
                        return note;
                }
                return null;
            }
        }

        private void OnStageNoteUpdated()
        {
            UpdateJudgeLineHitEffect();
            _propertyChangeNotifier.Invoke(this, NotifyProperty.StageNotesUpdated);
            //GridController.Instance.NotifyGameStageProgressChanged();
            //_perspectiveViewWindow.NotifyGameStageProgressChanged(GetPrevComboNoteIndex(), _stageNoteManager.CurrentCombo);
        }

        protected override void Awake()
        {
            base.Awake();
            _stageNoteManager = new StageNoteManager(
                UnityUtils.CreateObjectPool(_notePrefab, _noteParentTransform));
            //_stageNotes = new PooledObjectListView<StageNoteController>(
            //    UnityUtils.CreateObjectPool(_notePrefab, _noteParentTransform));
            _musicController.OnTimeChanged += OnMusicTimeChanged;
        }

        private void Start()
        {
            IsPianoNotesDistinguished = true;

            // TODO: Fake
            IsStageEffectOn = true;

            MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                Project.ProjectManager.NotifyProperty.CurrentProject,
                projm =>
                {
                    var proj = projm.CurrentProject;
                    if (proj is null) {
                        LoadChartInCurrentProject(null);
                        return;
                    }

                    // TODO: try out streaming clip provider
                    _musicController.ReplaceClip(new DecodedClipProvider(proj.AudioClip));

                    if (proj.Charts.Count == 0) {
                        LoadChartInCurrentProject(null);
                    }
                    else {
                        LoadChartInCurrentProject(proj.Charts[0]);
                    }
                });

            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                Project.ProjectManager.NotifyProperty.Audio,
                projm => _musicController.ReplaceClip(new DecodedClipProvider(projm.CurrentProject.AudioClip)));
        }

        private void Update()
        {
            if (IsStageEffectOn) UpdateStageEffect();
            if (!IsActive) return;
            if (!IsMusicPlaying && _manualPlaySpeedMultiplier is not 0f)
                _musicController.NudgePlaybackPosition(Time.deltaTime * _manualPlaySpeedMultiplier);
        }

        private PropertyChangeNotifier<GameStageController, NotifyProperty> _propertyChangeNotifier;
        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<GameStageController> action)
            => _propertyChangeNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            NoteSpeed,
            MusicSpeed,
            EffectVolume,
            MusicVolume,
            PianoVolume,
            IsShowLinkLines,
            SuddenPlus,

            CurrentChart,
            ChartName,
            ChartDifficulty,
            ChartLevel,
            ChartSpeed,
            ChartRemapMinVolume,
            ChartRemapMaxVolume,

            StageEffect,
            DistinguishPianoNotes,
            StageNotesUpdated,
        }
    }
}