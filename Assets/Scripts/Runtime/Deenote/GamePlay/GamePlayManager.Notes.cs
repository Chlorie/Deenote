#nullable enable

using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.GamePlay.Stage;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GamePlay
{
    partial class GamePlayManager
    {
        private NoteManager _noteManager;

        internal int NextHitNoteIndex => _noteManager.NextHitNoteIndex;

        public int CurrentCombo => _noteManager.CurrentCombo;

        private void OnStageLoaded_Notes(GameStageSceneLoader loader)
        {
            var stage = loader.StageController;
            // TODO: 切换stage时，需要重新创建NoteManager，还有Noteindicator
            _noteManager = new NoteManager(
                UnityUtils.CreateObjectPool(stage.Args.GamePlayNotePrefab, stage.NotePanelTransform, item => item.OnInstantiate(this)));
        }

        /// <returns>
        /// ComboNode that just reached judge line,
        /// <see langword="null"/> if current combo is 0
        /// <br/>
        /// See <see cref="StageNoteNodeExt.IsComboNode(IStageNoteNode)"/> for what is a ComboNote
        /// </returns>
        public IStageNoteNode? GetPreviouseHitComboNode()
        {
            ValidateChart();

            for (int i = _noteManager.NextHitNoteIndex - 1; i >= 0; i--) {
                var note = CurrentChart.NoteNodes[i];
                if (note.IsComboNode())
                    return note;
            }
            return null;
        }

        public void ForceUpdateNotes(bool notesOrderChanged, bool noteVisualDataChanged)
        {
            if (notesOrderChanged)
                UpdateActiveNotes();

            RefreshNotesVisual();
        }

        private void RefreshNotesVisual()
        {
            if (Stage is null)
                return;

            foreach (var note in _noteManager.ActiveNotes) {
                note.RefreshVisual();
            }
            foreach (var note in _noteManager.PendingNotes) {
                note.RefreshVisual();
            }
        }

        private void RefreshNotesTimeState()
        {
            if (Stage is null)
                return;

            foreach (var note in _noteManager.ActiveNotes) {
                note.RefreshTimeState();
            }
            foreach (var note in _noteManager.PendingNotes) {
                note.RefreshTimeState();
            }
        }

        /// <remarks>
        /// This is optimized version of <see cref="UpdateActiveNotes(bool)"/>,
        /// If we know the time of previous update, we can iterate from
        /// cached indices
        /// </remarks>
        /// <param name="forward">
        /// <see langword="true"/> if current time is greater than time on previous update
        /// </param>
        private void UpdateActiveNotesRelatively(bool forward)
        {
            UpdateActiveNotes();
            return;

            if (CurrentChart is null)
                return;
            if (forward) OnPlayForward();
            else OnPlayBackward();

            RefreshNotesTimeState();
            NotifyFlag(NotificationFlag.ActiveNoteUpdated);

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

        private void UpdateActiveNotes()
        {
            AssertStageLoaded();
            AssertChartLoaded();

            _noteManager.ClearAll();

            var activateTime = MusicPlayer.Time + Stage.NoteActiveAheadTime;
            var deactivateTime = MusicPlayer.Time - Stage.Args.HitEffectSpritePrefabs.HitEffectTime;

            int index = 0, combo = 0;

            // Iterate nodes before current stage
            for (; index < CurrentChart.NoteNodes.Length; index++) {
                var node = CurrentChart.NoteNodes[index];
                if (node.Time > deactivateTime)
                    break;
                AdjustCombo(node);
            }
            _noteManager.NextInactiveNoteIndex = index;

            // Iterate nodes in hiteffect time
            for (; index < CurrentChart.NoteNodes.Length; index++) {
                var node = CurrentChart.NoteNodes[index];
                if (node.Time > MusicPlayer.Time)
                    break;
                AdjustCombo(node);
                TrackNote(node);
            }
            _noteManager.NextHitNoteIndex = index;
            _noteManager.CurrentCombo = combo;

            // iterate nodes in falling time
            for (; index < CurrentChart.NoteNodes.Length; index++) {
                var node = CurrentChart.NoteNodes[index];
                if (node.Time > activateTime)
                    break;
                TrackNote(node);
            }
            _noteManager.NextActiveNoteIndex = index;

            RefreshNotesTimeState();
            NotifyFlag(NotificationFlag.ActiveNoteUpdated);

            void AdjustCombo(IStageNoteNode note)
            {
                if (note.IsComboNode())
                    combo++;
            }

            void TrackNote(IStageNoteNode note)
            {
                if (note is NoteTailNode tail) {
                    var head = tail.HeadModel;
                    if (head.Time <= deactivateTime) {
                        _noteManager.AddPendingNote(head);
                    }
                    // Handled in another branch
                    else {
                        return;
                    }
                }
                else {
                    Debug.Assert(note is NoteModel);
                    _noteManager.AddActiveNote((NoteModel)note);
                }
            }
        }

        // 3个index依次为NextDisappearNote，NextHitNote，NextAppearNote
        // |        |                |
        // D        H                A
        // 管理两个List<StageNoteController>
        // - _onStageNotes: NoteHead在范围[D,A)中
        // - _trackingNotes: Note已经出界(-,D]，但是NoteTail依然处于范围(D,A)中
        // _trackingNotes将全部都是Hold
        // _onStageNotes的顺序将和(D,A).OfType<NoteModel>()一致
        private struct NoteManager
        {
            private readonly ObjectPool<GamePlayNoteController> _pool;
            private readonly List<GamePlayNoteController> _activeNotes;
            /// <summary>
            /// Notes whose start time is earlier than NoteDisappearTime,
            /// but end time later than that,
            /// </summary>
            /// <remarks>
            /// Normally, this list contains very few notes, so actually we
            /// just using a List&lt;> as Dictionary&lt;,> 
            /// </remarks>
            private readonly List<GamePlayNoteController> _pendingNotes;

            /// <summary>
            /// Index of the note that will disappear next when
            /// player is playing forward
            /// </summary>
            public int NextInactiveNoteIndex;
            /// <summary>
            /// Index of the note that will next touch the 
            /// judgeline when player is playing forward.
            /// </summary>
            public int NextHitNoteIndex;
            /// <summary>
            /// Current combo
            /// </summary>
            /// <remarks>
            /// If note is hold, combo++ when hold tail reach judgeline,
            /// else combo++ when note reach judgeline
            /// </remarks>
            public int CurrentCombo;
            /// <summary>
            /// Index of the note that will next appear when
            /// player is playing forward.
            /// </summary>
            public int NextActiveNoteIndex;

            public readonly ReadOnlySpan<GamePlayNoteController> ActiveNotes => _activeNotes.AsSpan();
            public readonly ReadOnlySpan<GamePlayNoteController> PendingNotes => _pendingNotes.AsSpan();

            public NoteManager(ObjectPool<GamePlayNoteController> pool)
            {
                _pool = pool;
                _activeNotes = new();
                _pendingNotes = new();
                NextInactiveNoteIndex = 0;
                NextHitNoteIndex = 0;
                CurrentCombo = 0;
                NextActiveNoteIndex = 0;
            }

            public void ResetIndices()
            {
                NextInactiveNoteIndex = 0;
                NextHitNoteIndex = 0;
                CurrentCombo = 0;
                NextActiveNoteIndex = 0;
            }

            public readonly void ClearAll()
            {
                foreach (var item in _activeNotes) {
                    _pool.Release(item);
                }
                foreach (var item in _pendingNotes) {
                    _pool.Release(item);
                }
                _activeNotes.Clear();
                _pendingNotes.Clear();
            }

            /// <summary>
            /// Move a note in active list to pending list, NOTE that this
            /// method does not remove the item in active list
            /// </summary>
            public readonly void MoveToPending_NonRemove(int indexInActiveNotes)
            {
                _pendingNotes.Add(_activeNotes[indexInActiveNotes]);
            }


            public readonly void RemoveActiveNotes(Range range, bool checkPending)
            {
                foreach (var note in _activeNotes.AsSpan()[range]) {
                    _pool.Release(note);
                }
                _activeNotes.RemoveRange(range);
            }

            /// <summary>
            /// Remove range in active list, if note is in pending list,
            /// the note will not be released.
            /// </summary>
            public readonly void RemoveActiveNotesWithPendingCheck(Range range)
            {
                foreach (var note in _activeNotes.AsSpan()[range]) {
                    if (IndexOfPendingNote(note.NoteModel) < 0)
                        _pool.Release(note);
                }
                _activeNotes.RemoveRange(range);
            }

            public readonly void RemovePendingNote(NoteModel note)
            {
                int index = IndexOfPendingNote(note);
                Debug.Assert(index >= 0, "Cannot find note in pending list");

                _pool.Release(_pendingNotes[index]);
                _pendingNotes.RemoveAt(index);
            }

            public readonly void RemoveAllPendingNotes(Predicate<GamePlayNoteController> predicate)
            {
                var pool = _pool;
                _pendingNotes.RemoveAll(note =>
                {
                    if (predicate(note)) {
                        pool.Release(note);
                        return true;
                    }
                    else {
                        return false;
                    }
                });
            }

            public readonly GamePlayNoteController AddActiveNote(NoteModel note)
            {
                var item = _pool.Get();
                item.Initialize(note);
                _activeNotes.Add(item);
                return item;
            }

            public readonly GamePlayNoteController AddPendingNote(NoteModel note)
            {
                // We try to predict whether _trackingNotes contains this
                // in GameStageController.SearchForNotesOnStage(),
                // to reduce some cost on repeat-check
                Debug.Assert(IndexOfPendingNote(note) == -1);

                var gameNote = _pool.Get();
                gameNote.Initialize(note);
                _pendingNotes.Add(gameNote);
                return gameNote;
            }

            public readonly int IndexOfPendingNote(NoteModel note)
            {
                for (int i = 0; i < _pendingNotes.Count; i++) {
                    if (_pendingNotes[i].NoteModel == note)
                        return i;
                }
                return -1;
            }

            public readonly void PrependOnStageNotes(ReadOnlySpan<NoteModel> notes)
            {
                PreserveSpaceFromStart(_activeNotes, notes.Length);

                var span = _activeNotes.AsSpan();
                for (int i = 0; i < notes.Length; i++) {
                    // To keep active notes in order
                    NoteModel note = notes[i];
                    ref var gameNote = ref span[notes.Length - i - 1];
                    int indexInPending = IndexOfPendingNote(note);
                    if (indexInPending >= 0) {
                        // If the note is in pending list, move from pending list
                        gameNote = _pendingNotes[indexInPending];
                        _pendingNotes.RemoveAt(indexInPending);
                    }
                    else {
                        // Create new
                        var item = _pool.Get();
                        item.Initialize(note);
                        gameNote = item;
                    }
                }

                static void PreserveSpaceFromStart(List<GamePlayNoteController> list, int count)
                {
                    var oldCount = list.Count;
                    for (int i = 0; i < count; i++) {
                        list.Add(default!);
                    }
                    var span = list.AsSpan();
                    span[..oldCount].CopyTo(span[count..]);
                }
            }

            [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
            public readonly void AssertOnStageNotesInOrder(string? additionalMessage = null)
            {
                NoteTimeComparer.AssertInOrder(_activeNotes.Select(n => n.NoteModel), additionalMessage);
            }
        }
    }
}