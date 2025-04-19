#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Mathematics;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    internal abstract class GameStageNoteController : MonoBehaviour
    {
        protected GamePlayManager _game = default!;

        public NoteModel NoteModel { get; private set; } = default!;

        private (Vector2, Vector2)? _linkLine;
        private float _noteColorAlpha;

        [SerializeField]
        private NoteDisplayState _state;

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
                    SetHoldBodyDisplayLength();
                    break;
                case NoteDisplayState.Holding:
                    SetNotePositionZ(0f);
                    SetLinkLine();
                    SetHoldBodyDisplayLength();
                    SetHoldingHitEffect();
                    break;
                case NoteDisplayState.HitEffect:
                    SetNotePositionZ(0f);
                    SetLinkLine();
                    SetHitEffect(-_stageDeltaTime - NoteModel.GetActualDuration());
                    break;
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

        public void RefreshHoldLength()
        {
            SetHoldBodyDisplayLength();
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
            SetNoteSize();
            RefreshColoring();
            SetLinkLine();
            RefreshHoldLength();
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

        protected abstract void SetHoldingHitEffect();

        protected abstract void SetHitEffect(float timeAfterHit);

        private void SetNotePositionX()
        {
            transform.WithLocalPositionX(_game.ConvertNoteCoordPositionToWorldX(NoteModel.Position));
        }

        protected abstract void SetNoteSprite();

        protected abstract void SetNoteSize();

        private void SetNotePositionZ(float time)
        {
            _game.AssertStageLoaded();

            float z = _game.ConvertNoteCoordTimeToWorldZ(time, NoteModel.Speed);
            transform.WithLocalPositionZ(z);
        }

        protected void SetNoteSpriteAlpha()
        {
            _game.AssertStageLoaded();
            Debug.Assert(_state is NoteDisplayState.Fall);

            var appearAheadTime = AppearAheadTime;
            var noteFadeInEndTime = appearAheadTime * (1 - _game.Stage.Args.NoteFadeInRangePercent);

            var maxAlpha = _game.IsFilterNoteSpeed && !Mathf.Approximately(NoteModel.Speed, _game.HighlightedNoteSpeed)
                ? _game.Stage.Args.NoteDownplayAlpha
                : 1f;
            _noteColorAlpha = MathUtils.MapTo(_stageDeltaTime, appearAheadTime, noteFadeInEndTime, 0, maxAlpha);

            SetNoteSpriteRendererAlpha(_noteColorAlpha);
        }

        protected abstract void SetNoteSpriteRendererAlpha(float alpha);

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

        private void SetHoldBodyDisplayLength()
        {
            float time;
            bool isHolding;
            if (_state is NoteDisplayState.Fall) {
                time = NoteModel.GetActualDuration();
                isHolding = false;
            }
            else if (_state is NoteDisplayState.Holding) {
                time = _stageDeltaTime + NoteModel.GetActualDuration();
                isHolding = true;
            }
            else {
                time = 0f;
                isHolding = false;
            }

            _game.AssertStageLoaded();

            var scaleY = _game.ConvertNoteCoordTimeToHoldScaleY(time, NoteModel.Speed);
            SetHoldScaleY(scaleY, isHolding);
        }

        protected abstract void SetHoldScaleY(float scaleY, bool isHolding);

        private void SetNoteSpriteColor()
        {
            _game.AssertStageLoaded();
            var stage = _game.Stage;

            Color color;
            if (NoteModel.IsSelected)
                color = stage.Args.NoteSelectedColor;
            else if (NoteModel.IsCollided)
                color = stage.Args.NoteCollidedColor;
            else
                color = Color.white;
            SetNoteSpriteColorRGB(color);
        }

        protected abstract void SetNoteSpriteColorRGB(Color color);

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
                OnStateChanged(state);
            }
        }

        protected abstract void OnStateChanged(NoteDisplayState state);

        #endregion

        protected enum NoteDisplayState
        {
            Inactive,
            Invisible,
            Fall,
            Holding,
            HitEffect,
        }
    }
}