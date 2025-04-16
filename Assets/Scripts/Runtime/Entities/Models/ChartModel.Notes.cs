#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Entities.Comparisons;
using Deenote.Library.Collections;
using Deenote.Library.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote.Entities.Models
{
    partial class ChartModel
    {
        // `_visibleNotes` stores all visible `NoteModel`s and their `NoteTailNode`s if is hold
        // and `_holdCount` is ensured to be equal to count of `NoteTailNode` in `_visibleNotes`
        internal int _holdCount;

        /// <summary>
        /// Notes that will be displayed on stage.
        /// <br/>
        /// For hold notes, there will be 2 objects represent the
        /// hold head and hold tail, use <see cref="EnumerateNoteModels"/> get distinct notes
        /// </summary>
        public SortedList<IStageNoteNode> NoteNodes { get; internal set; }
        public SortedList<SoundNoteModel> BackgroundSoundNotes { get; internal set; }
        public SortedList<SpeedChangeWarningModel> SpeedChangeWarnings { get; internal set; }
        public SortedList<SpeedLineValueModel> SpeedLines { get; internal set; }

        public int NoteCount => NoteNodes.Count - _holdCount;

        public SpanUtils.OfTypeIterator<IStageNoteNode, NoteModel> EnumerateNoteModels()
            => NoteNodes.AsSpan().OfType<IStageNoteNode, NoteModel>();

        /// <summary>
        /// Generate SpeedLines according to notes in chart, call it before save to json
        /// </summary>
        /// <remarks>
        /// Currently Deenote does not support any way to edit speed lines directly
        /// (because it looks like a duplicated property)
        /// </remarks>
        private void GenerateSpeedLines()
        {
            var speedDiffNotes = NoteNodes
                .OfType<NoteModel>()
                .Adjacent()
                .Where(tpl => tpl.Item1.Speed != tpl.Item2.Speed);

            SpeedLines.Clear();
            foreach (var (prev,next) in speedDiffNotes) {
                SpeedLines.AddFromEnd(new SpeedLineValueModel(next.Speed, next.Time));
            }
        }

        internal CollisionResult GetCollidedNotesTo(NoteModel note)
        {
            var index = NoteNodes.BinarySearch(note);
            if (index < 0)
                ThrowHelper.ThrowInvalidOperationException("The chart doesn't contain the given note");

            return GetCollidedNotesToByIndex(index);
        }

        private CollisionResult GetCollidedNotesToByIndex(int index)
        {
            Debug.Assert(NoteNodes[index] is NoteModel);

            var note = (NoteModel)NoteNodes[index];
            var collidedNotes = new List<NoteModel>();

            foreach (var node in NoteNodes.AsSpan()[(index + 1)..]) {
                if (node is not NoteModel n)
                    continue;
                if (!EntityArgs.IsTimeCollided(n, note))
                    break;
                if (EntityArgs.IsPositionCollided(n, note))
                    collidedNotes.Add(n);
            }
            foreach (var node in NoteNodes.AsSpan()[..index].AsReversed()) {
                if (node is not NoteModel n)
                    continue;
                if (!EntityArgs.IsTimeCollided(n, note))
                    break;
                if (EntityArgs.IsPositionCollided(n, note))
                    collidedNotes.Add(n);
            }

            return new CollisionResult(note, collidedNotes);
        }

        internal NoteTailNode? FindTailOf(NoteModel note)
        {
            if (!note.IsHold)
                return null;

            // The comparer of StageNoteNodes will compare node.Time first, so this should work
            var index = NoteNodes.AsSpan().FindLowerBoundIndex(new NodeTimeComparable(note.EndTime));
            for (; index < NoteNodes.Count; index++) {
                var node = NoteNodes[index];
                if (node is NoteTailNode tail && tail.HeadModel == note)
                    return tail;
            }

            Debug.Assert(false, "Tail node of a hold note is not found");
            return null;
        }

        internal int IndexOfTailOf(NoteModel note)
        {
            if (!note.IsHold)
                return -1;

            // The comparer of StageNoteNodes will compare node.Time first, so this should work
            var index = NoteNodes.AsSpan().FindLowerBoundIndex(new NodeTimeComparable(note.EndTime));
            for (; index < NoteNodes.Count; index++) {
                var node = NoteNodes[index];
                if (node is NoteTailNode tail && tail.HeadModel == note)
                    return index;
            }

            Debug.Assert(false, "Tail node of a hold note is not found");
            return -1;
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