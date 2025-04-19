#nullable enable

using Deenote.Entities.Models;
using Deenote.Library.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Core.GameStage
{
    partial class NotesManager
    {
        private ObjectPool<GameStageNoteController> _pool = default!;
        private readonly SortedList<GameStageNoteController> _trackingNotesInTimeOrder;
        private readonly SortedList<GameStageNoteController> _trackingNotesAppearTimeOrder;

        internal ReadOnlySpan<GameStageNoteController> OnStageNotes => _trackingNotesInTimeOrder.AsSpan();


        #region Collection Modification

        private void AddTrackNote(NoteModel noteModel)
        {
            var item = _pool.Get();
            Debug.Assert(!_trackingNotesInTimeOrder.ToList().Contains(item));
            item.Initialize(noteModel);
            _trackingNotesInTimeOrder.Add(item);
            _trackingNotesAppearTimeOrder.Add(item);
        }

        private void ClearTrackNotes()
        {
            foreach (var item in _trackingNotesInTimeOrder) {
                _pool.Release(item);
            }
            _trackingNotesInTimeOrder.Clear();
            _trackingNotesAppearTimeOrder.Clear();
        }

        private void RemoveTrackNote(GameStageNoteController noteController)
        {
            _pool.Release(noteController);
            _trackingNotesAppearTimeOrder.Remove(noteController);
            _trackingNotesInTimeOrder.Remove(noteController);
        }
        /*
        /// <summary>
        /// Move a note in active list to pending list, NOTE that this
        /// method does not remove the item in active list
        /// </summary>
        private void MoveToPending_NonRemove(int indexInActiveNotes)
        {
            _pendingNotes.Add(_activeNotes[indexInActiveNotes]);
        }

        private void RemoveActiveNotes(Range range, bool checkPending)
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
        private void RemoveActiveNotesWithPendingCheck(Range range)
        {
            foreach (var note in _activeNotes.AsSpan()[range]) {
                if (IndexOfPendingNote(note.NoteModel) < 0)
                    _pool.Release(note);
            }
            _activeNotes.RemoveRange(range);
        }

        private void RemovePendingNote(NoteModel note)
        {
            int index = IndexOfPendingNote(note);
            Debug.Assert(index >= 0, "Cannot find note in pending list");

            _pool.Release(_pendingNotes[index]);
            _pendingNotes.RemoveAt(index);
        }

        private void RemoveAllPendingNotes(Predicate<GameStageNoteController> predicate)
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

        private GameStageNoteController AddActiveNote(NoteModel note, float appearAheadTime)
        {
            var item = _pool.Get();
            item.Initialize(note);
            _activeNotes.Add(item);
            return item;
        }

        private GameStageNoteController AddPendingNote(NoteModel note)
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

        private int IndexOfPendingNote(NoteModel note)
        {
            for (int i = 0; i < _pendingNotes.Count; i++) {
                if (_pendingNotes[i].NoteModel == note)
                    return i;
            }
            return -1;
        }

        private void PrependOnStageNotes(ReadOnlySpan<NoteModel> notes)
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

            static void PreserveSpaceFromStart(List<GameStageNoteController> list, int count)
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
        private void AssertOnStageNotesInOrder(string? additionalMessage = null)
        {
            NoteTimeComparer.AssertInOrder(_activeNotes.Select(n => n.NoteModel), additionalMessage);
        }
        */
        #endregion
    }
}