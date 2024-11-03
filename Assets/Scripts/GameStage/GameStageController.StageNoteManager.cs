#nullable enable

using Deenote.GameStage.Elements;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        // 3个index依次为NextDisappearNote，NextHitNote，NextAppearNote
        // |        |                |
        // D        H                A
        // 管理两个List<StageNoteController>
        // - _onStageNotes: NoteHead在范围[D,A)中
        // - _trackingNotes: Note已经出界(-,D]，但是NoteTail依然处于范围(D,A)中
        // _trackingNotes将全部都是Hold
        // _onStageNotes的顺序将和(D,A).OfType<NoteModel>()一致
        [Serializable]
        private struct StageNoteManager
        {
            public StageNoteManager(ObjectPool<StageNoteController> pool)
            {
                _pool = pool;
                _onStageNotes = new();
                _trackingNotes = new();
                NextDisappearNoteIndex = 0;
                NextHitNoteIndex = 0;
                CurrentCombo = 0;
                NextAppearNoteIndex = 0;
            }

            private readonly ObjectPool<StageNoteController> _pool;
            private readonly List<StageNoteController> _onStageNotes;
            /// <summary>
            /// Notes whose start time is earlier than NoteDisappearTime,
            /// but end time later than that.
            /// </summary>
            /// <remarks>
            /// Normally, this list will contains only 1 or 2 notes, so
            /// actually we treat this as a IDictionary<NoteModel, StageNoteController>
            /// </remarks>
            [SerializeField] private List<StageNoteController> _trackingNotes;

            /// <summary>
            /// Index of the note that will disappear next when
            /// player is playing forward
            /// </summary>
            public int NextDisappearNoteIndex;
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
            public int NextAppearNoteIndex;

            public readonly ReadOnlySpan<StageNoteController> OnStageNotes => _onStageNotes.AsSpan();

            public readonly ReadOnlySpan<StageNoteController> TrackingNotes => _trackingNotes.AsSpan();

            public void ResetIndices()
            {
                NextDisappearNoteIndex = 0;
                NextHitNoteIndex = 0;
                CurrentCombo = 0;
                NextAppearNoteIndex = 0;
            }

            public readonly void ClearAll()
            {
                foreach (var item in _trackingNotes) {
                    _pool.Release(item);
                }
                foreach (var item in _onStageNotes) {
                    _pool.Release(item);
                }
                _trackingNotes.Clear();
                _onStageNotes.Clear();
            }

            /// <summary>
            /// Move NoteController to _trackingNotes,
            /// without remove item in _onStageNote
            /// </summary>
            /// <param name="indexOfOnStageNotes">Index in _onStageNote</param>
            public readonly void MoveNoteToTracking_NonRemove(int indexOfOnStageNotes)
            {
                _trackingNotes.Add(_onStageNotes[indexOfOnStageNotes]);
            }

            public readonly void RemoveOnStageNotes(Range range)
            {
                foreach (var note in _onStageNotes.AsSpan()[range]) {
                    _pool.Release(note);
                }
                _onStageNotes.RemoveRange(range);
            }

            /// <summary>
            /// Remove range in _onStageNotes, if note is held by _trackingNotes,
            /// the note will not be released.
            /// </summary>
            public readonly void RemoveOnStageNotesWithTrackingCheck(Range range)
            {
                foreach (var note in _onStageNotes.AsSpan()[range]) {
                    if (IndexOfInTrackingNotes(note.Model) == -1)
                        _pool.Release(note);
                }
                _onStageNotes.RemoveRange(range);
            }

            public readonly void RemoveTrackingNote(NoteModel noteModel)
            {
                int index = IndexOfInTrackingNotes(noteModel);
                Debug.Assert(index >= 0, "Cannot find note in _trackingNotes");

                _pool.Release(_trackingNotes[index]);
                _trackingNotes.RemoveAt(index);
                return;
            }

            public readonly void RemoveAllTrackingNotes(Predicate<StageNoteController> predicate)
            {
                var pool_captured = _pool;
                _trackingNotes.RemoveAll(note =>
                {
                    if (predicate(note)) {
                        pool_captured.Release(note);
                        return true;
                    }
                    else
                        return false;
                });
            }

            public readonly void AddOnStageNote(NoteModel note)
            {
                var item = _pool.Get();
                item.Initialize(note);
                _onStageNotes.Add(item);
            }

            public readonly void AddTrackingNote(NoteModel note)
            {
                // We try to predict whether _trackingNotes contains this
                // in GameStageController.SearchForNotesOnStage(),
                // to reduce some cost on repeat-check
                Debug.Assert(IndexOfInTrackingNotes(note) == -1);

                var item = _pool.Get();
                item.Initialize(note);
                _trackingNotes.Add(item);
            }

            public readonly int IndexOfInTrackingNotes(NoteModel note)
            {
                for (int i = 0; i < _trackingNotes.Count; i++) {
                    if (_trackingNotes[i].Model == note)
                        return i;
                }
                return -1;
            }

            public readonly void PrependOnStageNotes(ReadOnlySpan<NoteModel> notes)
            {
                var oldCount = _onStageNotes.Count;
                for (int i = 0; i < notes.Length; i++)
                    _onStageNotes.Add(null);

                var span = _onStageNotes.AsSpan();
                span[..oldCount].CopyTo(span[notes.Length..]);
                for (int i = 0; i < notes.Length; i++) {
                    // To keep _onStageNotes in order
                    NoteModel note = notes[i];
                    ref StageNoteController destination = ref span[notes.Length - i - 1];
                    int indexInTrackingNotes = IndexOfInTrackingNotes(note);
                    if (indexInTrackingNotes >= 0) {
                        // If the note is in _trackingNotes, move it into _onStageNotes
                        destination = _trackingNotes[indexInTrackingNotes];
                        _trackingNotes.RemoveAt(indexInTrackingNotes);
                    }
                    else {
                        // Else create new
                        var item = _pool.Get();
                        item.Initialize(note);
                        destination = item;
                    }
                }
            }

            [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
            public readonly void AssertOnStageNotesInOrder(string? additionalMessage = null)
            {
                NoteTimeComparer.AssertInOrder(_onStageNotes.Select(n => n.Model), additionalMessage);
            }
        }
    }
}