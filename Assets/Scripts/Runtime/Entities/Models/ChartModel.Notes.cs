#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Entities.Models
{
    partial class ChartModel
    {
        // `_visibleNotes` stores all visible `NoteModel`s and their `NoteTailNode`s if is hold
        // and `_holdCount` is ensured to be equal to count of `NoteTailNode` in `_visibleNotes`
        internal int _holdCount;
        internal List<IStageNoteNode> _visibleNoteNodes;
        internal List<SoundNoteModel> _backgroundNotes;
        internal List<SpeedLineValueModel> _speedLines;

        /// <summary>
        /// Notes that will be displayed on stage.
        /// <br/>
        /// For hold notes, there will be 2 objects represent the
        /// hold head and hold tail, use <see cref="EnumerateNoteModels"/> get distinct notes
        /// </summary>
        public ReadOnlySpan<IStageNoteNode> NoteNodes => _visibleNoteNodes.AsSpan();

        public ReadOnlySpan<SoundNoteModel> BackgroundNotes => _backgroundNotes.AsSpan();

        public ReadOnlySpan<SpeedLineValueModel> SpeedLines => _speedLines.AsSpan();

        public int NoteCount => _visibleNoteNodes.Count - _holdCount;

        public CollectionUtils.SpanOfTypeIterator<IStageNoteNode, NoteModel> EnumerateNoteModels()
            => NoteNodes.OfType<IStageNoteNode, NoteModel>();

        internal CollisionResult GetCollidedNotesTo(int noteModelIndex)
        {
            NoteTimeComparer.AssertInOrder(_visibleNoteNodes);
            Debug.Assert(_visibleNoteNodes[noteModelIndex] is NoteModel);

            var collidedNotes = new List<NoteModel>();
            var editNote = (NoteModel)_visibleNoteNodes[noteModelIndex];

            for (int i = noteModelIndex - 1; i >= 0; i--) {
                if (_visibleNoteNodes[i] is not NoteModel note)
                    continue;

                if (!EntityArgs.IsTimeCollided(editNote, note))
                    break;
                if (EntityArgs.IsPositionCollided(editNote, note))
                    collidedNotes.Add(editNote);
            }

            for (int i = noteModelIndex + 1; i < _visibleNoteNodes.Count; i++) {
                if (_visibleNoteNodes[i] is not NoteModel note)
                    continue;

                if (!EntityArgs.IsTimeCollided(editNote, note))
                    break;
                if (EntityArgs.IsPositionCollided(editNote, note))
                    collidedNotes.Add(editNote);
            }

            return new CollisionResult(editNote, collidedNotes);
        }

        public int Search(NoteModel note)
        {
            var index = _visibleNoteNodes.BinarySearch(note, NoteTimeComparer.Instance);
            if (index < 0)
                return index;

            var find = _visibleNoteNodes[index];
            if (find == note)
                return index;

            for (int i = index - 1; i >= 0; i--) {
                var near = _visibleNoteNodes[i];
                if (near.Time != note.Time)
                    break;
                if (near == note)
                    return i;
            }

            for (int i = index + 1; i < _visibleNoteNodes.Count; i++) {
                var near = _visibleNoteNodes[i];
                if (near.Time != note.Time)
                    break;
                if (near == note)
                    return i;
            }

            return ~index;
        }

        internal int IndexOfTailOf(int noteModelIndex)
        {
            NoteTimeComparer.AssertInOrder(_visibleNoteNodes);
            Debug.Assert(_visibleNoteNodes[noteModelIndex] is NoteModel);

            var head = (NoteModel)_visibleNoteNodes[noteModelIndex];

            for (int i = noteModelIndex + 1; i < _visibleNoteNodes.Count; i++) {
                var note = _visibleNoteNodes[i];
                if (note is NoteTailNode tail && tail.HeadModel == head)
                    return i;
            }

            Debug.Assert(false, "Chart contains a hold note but its tail doesnt exist");
            return -1;
        }

        internal int SearchTailOf(NoteModel note)
        {
            var index = _visibleNoteNodes.AsSpan().BinarySearch(new NoteTimeComparable(note.EndTime));
            if (index < 0)
                return index;

            var find = _visibleNoteNodes[index];
            if (find is NoteTailNode tail && tail.HeadModel == note)
                return index;

            for (int i = index - 1; i >= 0; i--) {
                var near = _visibleNoteNodes[i];
                if (near.Time != note.Time)
                    break;
                if (near is NoteTailNode nearAsTail && nearAsTail.HeadModel == note)
                    return i;
            }

            for (int i = index + 1; i < _visibleNoteNodes.Count; i++) {
                var near = _visibleNoteNodes[i];
                if (near.Time != note.Time)
                    break;
                if (near is NoteTailNode nearAsTail && nearAsTail.HeadModel == note)
                    return i;
            }

            return ~index;
        }

        internal readonly struct CollisionResult
        {
            private readonly NoteModel _note;
            private readonly List<NoteModel> _collidedNotes;

            public NoteModel Note => _note;
            public ReadOnlySpan<NoteModel> CollidedNotes => _collidedNotes.AsSpan();

            internal CollisionResult(NoteModel note, List<NoteModel> collidedNotes)
            {
                _note = note;
                _collidedNotes = collidedNotes;
            }

            public void IncrementModelCollsionCounts()
            {
                _note.CollisionCount += _collidedNotes.Count;
                foreach (var note in _collidedNotes) {
                    note.CollisionCount++;
                }
            }

            public void DecrementModelCollisionCounts()
            {
                _note.CollisionCount -= _collidedNotes.Count;
                foreach (var note in _collidedNotes) {
                    note.CollisionCount--;
                }
            }
        }
    }
}