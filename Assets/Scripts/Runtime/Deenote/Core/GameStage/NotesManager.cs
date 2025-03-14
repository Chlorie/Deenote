#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.Collections.Generic;
using Deenote.Library.Numerics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
                (l, r) => NodeTimeUniqueComparer.Instance.Compare(l.NoteModel, r.NoteModel)));
            _trackingNotesAppearTimeOrder = new(Comparer<GameStageNoteController>.Create(
                (l, r) =>
                {
                    _game.AssertStageLoaded();
                    var ltime = _game.GetStageNoteActiveTime(l.NoteModel);
                    var rtime = _game.GetStageNoteActiveTime(r.NoteModel);
                    var cmp = Comparer<float>.Default.Compare(ltime, rtime);
                    if (cmp != 0) return cmp;
                    cmp = NodeUniqueComparer.Instance.Compare(l.NoteModel, r.NoteModel);
                    Debug.Assert(cmp != 0);
                    return cmp;
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
                    var ltime = _game.GetStageNoteActiveTime(l);
                    var rtime = _game.GetStageNoteActiveTime(r);
                    var cmp = Comparer<float>.Default.Compare(ltime, rtime);
                    if (cmp != 0) return cmp;
                    cmp = NodeUniqueComparer.Instance.Compare(l, r);
                    Debug.Assert(cmp != 0);
                    return cmp;
                }));
            }
            else {
                _stageNoteNodesInAppearOrder.Clear();
            }

            _game.AssertChartLoaded();

            foreach (var node in _game.CurrentChart.NoteNodes) {
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

            var nodes = _game.CurrentChart.NoteNodes.AsSpan();
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
                ReselectActiveVisibleNotesInternal(_game.CurrentChart.NoteNodes.AsSpan());
            }
        }

        private void ReselectActiveVisibleNotesInternal(ReadOnlySpan<IStageNoteNode> noteNodesInAppearOrder)
        {
            _game.AssertStageLoaded();
            _game.AssertChartLoaded();

            ClearTrackNotes();
            var stage = _game.Stage;
            var chart = _game.CurrentChart;
            var noteNodes = chart.NoteNodes.AsSpan();

            var currentTime = _game.MusicPlayer.Time;
            var deactiveDeltaTime = stage.Args.HitEffectSpritePrefabs.HitEffectTime;
            var deactiveNoteTime = currentTime - deactiveDeltaTime;
            var activateDeltaTime = _game.StageNoteActiveAheadTime;
            var activateNoteTime = currentTime + activateDeltaTime;

            int index = 0, combo = 0;

            // Iterate nodes before current stage
            // Shoul track if note tail is on stage
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node.Time > deactiveNoteTime)
                    break;
                IncrementCombo(node);
                TrackNote(node);
            }
            _nextInactiveNoteIndex = index;

            // Iterate nodes in hiteffect time
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node.Time > currentTime)
                    break;
                IncrementCombo(node);
                TrackNote(node);
            }
            _nextHitNoteIndex = index;
            CurrentCombo = combo;

            // Iterate nodes in falling time to find _nextActiveNoteIndex
            // Notes may have different speed, we do not track note here
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node is NoteModel && _game.GetNotePseudoTime(node.Time, node.Speed) > activateNoteTime)
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
        }

        private void ResetIndices()
        {
            _nextInactiveNoteIndex = 0;
            _nextHitNoteIndex = 0;
            CurrentCombo = 0;
            _nextActiveNoteIndex = 0;
        }

        private void ShiftActiveVisibleNotes(bool forward)
        {
            ReselectActiveVisibleNotes();
            return;
            _game.AssertChartLoaded();

            var noteNodesInAppearOrder = _game.IsApplySpeedDifference
                ? _stageNoteNodesInAppearOrder.AsSpan()
                : _game.CurrentChart.NoteNodes.AsSpan();

            if (forward)
                ShiftActiveVisibleNotesForwardInternal(noteNodesInAppearOrder);
            else
                ShiftActiveVisibleNotesBackwardInternal(noteNodesInAppearOrder);
        }

        private void ShiftActiveVisibleNotesForwardInternal(ReadOnlySpan<IStageNoteNode> noteNodesInAppearOrder)
        {
            _game.AssertChartLoaded();
            _game.AssertStageLoaded();

            var stage = _game.Stage;
            var chart = _game.CurrentChart;
            var noteNodes = chart.NoteNodes.AsSpan();

            var currentTime = _game.MusicPlayer.Time;
            var deactiveDeltaTime = stage.Args.HitEffectSpritePrefabs.HitEffectTime;
            var deactiveNoteTime = currentTime - deactiveDeltaTime;
            var activateDeltaTime = _game.StageNoteActiveAheadTime;
            var activateNoteTime = currentTime + activateDeltaTime;

            var index = _nextInactiveNoteIndex;
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node.Time > deactiveNoteTime)
                    break;
            }
            for (int i = 0; i < _trackingNotesInTimeOrder.Count; i++) {
                var noteCtrl = _trackingNotesInTimeOrder[i];
                var note = noteCtrl.NoteModel;
                if (note.Time > deactiveNoteTime)
                    break;
                if (note.EndTime <= deactiveNoteTime) {
                    RemoveTrackNote(noteCtrl);
                }
            }
            _nextInactiveNoteIndex = index;

            index = _nextHitNoteIndex;
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node.Time > currentTime)
                    break;
                if (node.IsComboNode)
                    CurrentCombo++;
            }
            _nextHitNoteIndex = index;

            // Iterate nodes in falling time to find _nextActiveNoteIndex
            // Notes may have different speed, we do not track note here
            index = _nextActiveNoteIndex;
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node is NoteModel && _game.GetNotePseudoTime(node.Time, node.Speed) > activateNoteTime)
                    break;
            }
            _nextActiveNoteIndex = index;

            var newNextActiveNoteIndexInAppearOrder = noteNodesInAppearOrder[_nextActiveNoteIndexInAppearOrder..]
                .LinearSearch(new AppearTimeComparable(currentTime, this));
            NumberUtils.FlipNegative(ref newNextActiveNoteIndexInAppearOrder);
            newNextActiveNoteIndexInAppearOrder += _nextActiveNoteIndexInAppearOrder;
            Debug.Log($"{_nextActiveNoteIndexInAppearOrder}, {newNextActiveNoteIndexInAppearOrder}");
            foreach (var node in noteNodesInAppearOrder[_nextActiveNoteIndexInAppearOrder..newNextActiveNoteIndexInAppearOrder]) {
                if (node is NoteModel note) {
                    AddTrackNote(note);
                }
            }
            _nextActiveNoteIndexInAppearOrder = newNextActiveNoteIndexInAppearOrder;

            // Chain stage notes
            GameStageNoteController? prevStageNote = null;
            foreach (var note in _trackingNotesInTimeOrder) {
                note.PostInitialize(prevStageNote);
                prevStageNote = note;
            }

            Debug.Assert(_trackingNotesInTimeOrder.Distinct().Count() == _trackingNotesInTimeOrder.Count());
        }

        private void ShiftActiveVisibleNotesBackwardInternal(ReadOnlySpan<IStageNoteNode> noteNodesInAppearOrder)
        {
            _game.AssertChartLoaded();
            _game.AssertStageLoaded();

            var stage = _game.Stage;
            var chart = _game.CurrentChart;
            var noteNodes = chart.NoteNodes.AsSpan();

            var currentTime = _game.MusicPlayer.Time;
            var deactiveDeltaTime = stage.Args.HitEffectSpritePrefabs.HitEffectTime;
            var deactiveNoteTime = currentTime - deactiveDeltaTime;
            var activateDeltaTime = _game.StageNoteActiveAheadTime;
            var activateNoteTime = currentTime + activateDeltaTime;

            var index = _nextInactiveNoteIndex - 1;
            for (; index >= 0; index--) {
                var node = noteNodes[index];
                if (node.Time <= deactiveNoteTime)
                    break;
                if (node is NoteTailNode tail) {
                    AddTrackNote(tail.HeadModel);
                }
                else if (node is NoteModel { IsHold: false } note) {
                    AddTrackNote(note);
                }
                else {
                    Debug.Assert(node is NoteModel, "Unknown IStageNoteNode Type");
                }
            }
            _nextInactiveNoteIndex = index + 1;

            index = _nextHitNoteIndex - 1;
            for (; index >= 0; index--) {
                var node = noteNodes[index];
                if (node.Time <= currentTime)
                    break;
                if (node.IsComboNode)
                    CurrentCombo--;
            }
            _nextHitNoteIndex = index + 1;

            // Iterate nodes in falling time to find _nextActiveNoteIndex
            // Notes may have different speed, we do not track note here
            index = _nextHitNoteIndex;
            for (; index < noteNodes.Length; index++) {
                var node = noteNodes[index];
                if (node is NoteModel && _game.GetNotePseudoTime(node.Time, node.Speed) > activateNoteTime)
                    break;
            }
            _nextActiveNoteIndex = index;

            var newNextActiveNoteIndexInAppearOrder = noteNodesInAppearOrder[.._nextActiveNoteIndexInAppearOrder]
                .FindLowerBoundIndex(new AppearTimeComparable(currentTime, this));
            for (int i = _trackingNotesAppearTimeOrder.Count - 1; i >= 0; i--) {
                var noteCtrl = _trackingNotesAppearTimeOrder[i];
                var note = noteCtrl.NoteModel;
                if (note.Time <= activateNoteTime)
                    break;
                if (note.Time > activateNoteTime)
                    RemoveTrackNote(noteCtrl);
            }
            _nextActiveNoteIndexInAppearOrder = newNextActiveNoteIndexInAppearOrder;

            // Chain stage notes
            GameStageNoteController? prevStageNote = null;
            foreach (var note in _trackingNotesInTimeOrder) {
                note.PostInitialize(prevStageNote);
                prevStageNote = note;
            }
        }

        /// <summary>
        /// Called when note collection changed, add/remove or notes' time/speed etc. changed
        /// </summary>
        internal void RefreshStageActiveNotes()
        {
            var currentTime = _game.MusicPlayer.Time;

            UpdateNotesAppearOrder();
            ReselectActiveVisibleNotes();

            RefreshNoteSoundsIndices();
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
            foreach (var note in _trackingNotesInTimeOrder) {
                note.RefreshStageDeltaTime();
            }

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
                => Comparer<float>.Default.Compare(_time, _manager._game.GetStageNoteActiveTime(other));
        }
    }
}