#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Core.GameStage
{
    // 3个index依次为NextDisappearNote，NextHitNote，NextAppearNote
    // |        |                |
    // D        H                A
    // 管理两个List<StageNoteController>
    // - _onStageNotes: NoteHead在范围[D,A)中
    // - _trackingNotes: Note已经出界(-,D]，但是NoteTail依然处于范围(D,A)中
    // _trackingNotes将全部都是Hold
    // _onStageNotes的顺序将和(D,A).OfType<NoteModel>()一致
    public sealed partial class NotesManager
    {
        private readonly GamePlayManager _game;
        private float _time;

        /// <summary>
        /// Index of the note that will disappear next when
        /// player is playing forward
        /// </summary>
        private int _nextInactiveNoteIndex;
        /// <summary>
        /// Index of the note that will next touch the 
        /// judgeline when player is playing forward.
        /// </summary>
        private int _nextHitNoteIndex;
        /// <summary>
        /// Current combo
        /// </summary>
        /// <remarks>
        /// If note is hold, combo++ when hold tail reach judgeline,
        /// else combo++ when note reach judgeline
        /// </remarks>
        public int CurrentCombo { get; private set; }
        /// <summary>
        /// Index of the note that will next appear when
        /// player is playing forward.
        /// </summary>
        private int _nextActiveNoteIndex;
        private int _nextActiveNoteIndexInAppearOrder;

        private SortedList<IStageNoteNode> _stageNoteNodesInAppearOrder = default!;

        internal NotesManager(GamePlayManager game)
        {
            _game = game;
            _trackingNotesInTimeOrder = new(Comparer<GameStageNoteController>.Create(
                (l, r) => Comparer<float>.Default.Compare(l.NoteModel.Time, r.NoteModel.Time)));
            _trackingNotesAppearTimeOrder = new(Comparer<GameStageNoteController>.Create(
                (l, r) =>
                {
                    _game.AssertStageLoaded();
                    var ltime = _game.GetStageNoteAppearTime(l.NoteModel);
                    var rtime = _game.GetStageNoteAppearTime(r.NoteModel);
                    return Comparer<float>.Default.Compare(ltime, rtime);
                }));
        }

        internal void Initialize(ObjectPool<GameStageNoteController> gameStageNotePool)
        {
            if (_pool is not null) {
                ClearTrackNotes();
                _pool.Dispose();
            }

            _pool = gameStageNotePool;
            _nextInactiveNoteIndex = 0;
            _nextHitNoteIndex = 0;
            CurrentCombo = 0;
            _nextActiveNoteIndex = 0;
        }

        private void UpdateNotesAppearOrder()
        {
            if (_stageNoteNodesInAppearOrder is null) {
                _stageNoteNodesInAppearOrder = new SortedList<IStageNoteNode>(Comparer<IStageNoteNode>.Create((l, r) =>
                {
                    _game.AssertStageLoaded();
                    var ltime = _game.GetStageNoteAppearTime(l);
                    var rtime = _game.GetStageNoteAppearTime(r);
                    return Comparer<float>.Default.Compare(ltime, rtime);
                }));
            }
            else {
                _stageNoteNodesInAppearOrder.Clear();
            }

            foreach (var node in _game.CurrentChart!.NoteNodes) {
                _stageNoteNodesInAppearOrder.AddFromEnd(node);
            }
        }

        #region Get

        /// <returns>
        /// ComboNode that just reached judge line,
        /// <see langword="null"/> if current combo is 0
        /// </returns>
        public IStageNoteNode? GetPreviousHitComboNode()
        {
            _game.AssertChartLoaded();

            for (int i = _nextHitNoteIndex - 1; i >= 0; i--) {
                var note = _game.CurrentChart.NoteNodes[i];
                if (note.IsComboNode)
                    return note;
            }
            return null;
        }

        public NoteModel? GetPreviousHitNote()
        {
            _game.AssertChartLoaded();

            for (int i = _nextHitNoteIndex - 1; i >= 0; i--) {
                if (_game.CurrentChart.NoteNodes[i] is NoteModel note)
                    return note;
            }
            return null;
        }

        public IStageNoteNode? GetNextActiveNodeInTimeOrderDisplayMode()
        {
            _game.AssertChartLoaded();

            var nodes = _game.CurrentChart.NoteNodes;
            if (_nextActiveNoteIndex >= nodes.Length)
                return null;
            else
                return nodes[_nextActiveNoteIndex];
        }

        #endregion

        private void ReselectActiveVisibleNotes()
        {
            if (_game.IsApplySpeedDifference) {
                ReselectActiveVisibleNotesInternal(_stageNoteNodesInAppearOrder.AsSpan());
            }
            else {
                _game.AssertChartLoaded();
                ReselectActiveVisibleNotesInternal(_game.CurrentChart.NoteNodes);
            }
        }

        private void ReselectActiveVisibleNotesInternal(ReadOnlySpan<IStageNoteNode> noteNodesInAppearOrder)
        {
            _game.AssertStageLoaded();
            _game.AssertChartLoaded();

            ClearTrackNotes();
            var stage = _game.Stage;
            var chart = _game.CurrentChart;

            var currentTime = _game.MusicPlayer.Time;
            var deactiveDeltaTime = stage.Args.HitEffectSpritePrefabs.HitEffectTime;
            var deactiveNoteTime = currentTime - deactiveDeltaTime;
            var activateDeltaTime = _game.StageNoteActiveAheadTime;
            var activateNoteTime = currentTime + activateDeltaTime;

            int index = 0, combo = 0;

            // Iterate nodes before current stage
            for (; index < chart.NoteNodes.Length; index++) {
                var node = chart.NoteNodes[index];
                if (node.Time > deactiveNoteTime)
                    break;
                IncrementCombo(node);
                TrackNote(node);
            }
            _nextInactiveNoteIndex = index;

            // Iterate nodes in hiteffect time
            for (; index < chart.NoteNodes.Length; index++) {
                var node = chart.NoteNodes[index];
                if (node.Time > currentTime)
                    break;
                IncrementCombo(node);
                TrackNote(node);
            }
            _nextHitNoteIndex = index;
            CurrentCombo = combo;

            // Iterate nodes in falling time to find _nextActiveNoteIndex
            // Notes may have different speed, we do not track note here
            for (; index < chart.NoteNodes.Length; index++) {
                var node = chart.NoteNodes[index];
                if (node is NoteModel && ToPseudoTime(node.Time, node.Speed) > activateNoteTime)
                    break;
            }
            _nextActiveNoteIndex = index;

            // Iterate node in falling time, by appear time order
            var indexInAppearOrder = noteNodesInAppearOrder
                .FindLowerBoundIndex(new AppearTimeComparable(currentTime, this));

            for (int i = indexInAppearOrder - 1; i >= 0; i--) {
                var node = noteNodesInAppearOrder[i];
                if (node is NoteModel note) {
                    if (note.Time > currentTime)
                        AddTrackNote(note);
                }
            }
            _nextActiveNoteIndexInAppearOrder = indexInAppearOrder;

            // Chain stage notes
            GameStageNoteController? prevStageNote = null;
            foreach (var note in _trackingNotesInTimeOrder) {
                note.PostInitialize(prevStageNote);
                prevStageNote = note;
            }

            void IncrementCombo(IStageNoteNode node)
            {
                if (node.IsComboNode)
                    combo++;
            }

            void TrackNote(IStageNoteNode node)
            {
                if (node is not NoteModel note)
                    return;
                if (note.EndTime > deactiveNoteTime) {
                    AddTrackNote(note);
                }
            }

            float ToPseudoTime(float time, float noteSpeed)
            {
                var currentTime = _game.MusicPlayer.Time;
                return currentTime + (time - currentTime) * _game.GetDisplayNoteSpeed(noteSpeed);
            }
        }

        private void ResetIndices()
        {
            _nextInactiveNoteIndex = 0;
            _nextHitNoteIndex = 0;
            CurrentCombo = 0;
            _nextActiveNoteIndex = 0;
        }

        /// <remarks>
        /// This is optimized version of <see cref="UpdateActiveNotes(bool)"/>,
        /// If we know the time of previous update, we can iterate from
        /// cached indices
        /// </remarks>
        /// <param name="forward">
        /// <see langword="true"/> if current time is greater than time on previous update
        /// </param>
        private void ShiftActiveVisibleNotes(bool forward)
        {

            ReselectActiveVisibleNotes();
            return;

            if (_game.CurrentChart is null)
                return;
            if (forward) OnPlayForward();
            else OnPlayBackward();

            // RefreshNotesTimeState();

            void OnPlayForward()
            {

            }
            void OnPlayBackward() { }

#if false
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
                        IStageNoteNode note = Chart.Notes[i];
                        if (note is NoteTailNode noteTail) {
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
                    var note = Chart.Notes[i];
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
                    IStageNoteNode note = Chart.Notes[i];
                    if (note is NoteTailNode noteTail) {
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
                    for (; index < Chart.Notes.Count; index++) {
                        var note = Chart.Notes[index];
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
                        IStageNoteNode note = Chart.Notes[i];
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
                    var note = Chart.Notes[i];
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
                    IStageNoteNode note = Chart.Notes[i];
                    if (note is NoteTailNode noteTail) {
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
                        var note = Chart.Notes[index];
                        if (note.Time <= compareTime) {
                            break;
                        }
                    }
                    index++;
                }
            }

#endif
        }

        /// <summary>
        /// Called when note collection changed, add/remove or notes' time/speed etc. changed
        /// </summary>
        internal void RefreshStageActiveNotes()
        {
            var currentTime = _game.MusicPlayer.Time;

            UpdateNotesAppearOrder();
            ReselectActiveVisibleNotes();

            UpdateNoteSoundsRelatively(currentTime >= _time, false);
            _time = currentTime;
        }

        /// <summary>
        /// Called when music time changed
        /// </summary>
        /// <param name="playSound"></param>
        internal void ShiftStageActiveNotes(bool playSound)
        {
            var currentTime = _game.MusicPlayer.Time;

            ShiftActiveVisibleNotes(currentTime >= _time);

            UpdateNoteSoundsRelatively(currentTime >= _time, playSound);
            _time = currentTime;
        }

        private readonly struct AppearTimeComparable : IComparable<IStageNoteNode>
        {
            private readonly float _time;
            private readonly NotesManager _manager;

            public AppearTimeComparable(float time, NotesManager manager)
            {
                _time = time;
                _manager = manager;
            }

            public int CompareTo(IStageNoteNode other)
                => Comparer<float>.Default.Compare(_time, _manager._game.GetStageNoteAppearTime(other));
        }
    }
}