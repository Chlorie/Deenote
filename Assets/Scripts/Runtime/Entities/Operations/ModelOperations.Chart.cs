#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.Numerics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Entities.Operations
{
    partial class ModelOperations
    {
        public static AddNoteOperation AddNote(this ChartModel chart, NoteModel note)
            => new(chart, note);

        public static AddMultipleNotesOperation AddMultipleNotes(this ChartModel chart, ImmutableArray<NoteModel> notes)
        {
            NoteTimeComparer.AssertInOrder(notes);
            int holdCount = 0;
            foreach (var note in notes) {
                if (note.IsHold)
                    holdCount++;
            }

            return new AddMultipleNotesOperation(chart, notes, holdCount);
        }

        public static RemoveNotesOpeartion RemoveOrderedNotes(this ChartModel chart, ImmutableArray<NoteModel> notes)
        {
            NoteTimeComparer.AssertInOrder(notes);
            return new(chart, notes);
        }

        /// <remarks>
        /// DO NOT use this method edit note Time / Position / Duration / Kind / Sounds
        /// </remarks>
        public static EditNotesPropertyOperationBase<T> EditNotes<T>(this ChartModel chart, ReadOnlySpan<NoteModel> notes, T value,
            Func<NoteModel, T> getter, Action<NoteModel, T> setter)
            => new EditNotesPropertyOperation<T>(chart, notes.ToImmutableArray(), value, getter, setter);

        /// <remarks>
        /// DO NOT use this method edit note Time / Position / Duration / Kind / Sounds
        /// </remarks>
        public static EditNotesPropertyOperationBase<T> EditNotes<T>(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<T, T> valueSelector,
            Func<NoteModel, T> getter, Action<NoteModel, T> setter)
            => new EditNotesPropertyOperation<T>(chart, notes.ToImmutableArray(), valueSelector, getter, setter);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesTime(this ChartModel chart, ReadOnlySpan<NoteModel> notes, float value)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), old => old with { Time = value }, true);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesTime(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<float, float> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), old => old with { Time = valueSelector(old.Time) }, true);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesPosition(this ChartModel chart, ReadOnlySpan<NoteModel> notes, float value)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), old => old with { Position = value }, false);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesPosition(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<float, float> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), old => old with { Position = valueSelector(old.Position) }, false);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesCoord(this ChartModel chart, ReadOnlySpan<NoteModel> notes, NoteCoord value)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), value, true);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesCoord(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<NoteCoord, NoteCoord> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), valueSelector, true);

        public static EditNotesPropertyOperationBase<float> EditNotesDuration(this ChartModel chart, ReadOnlySpan<NoteModel> notes, float value)
            => new EditNotesDurationPropertyOperation(chart, notes.ToImmutableArray(), value);

        public static EditNotesPropertyOperationBase<NoteModel.NoteKind> EditNotesKind(this ChartModel chart, ReadOnlySpan<NoteModel> notes, NoteModel.NoteKind value)
            => new EditNotesKindPropertyOperation(chart, notes.ToImmutableArray(), value);

        public static EditNotesPropertyOperationBase<ImmutableArray<PianoSoundValueModel>> EditNotesSounds(this ChartModel chart, ReadOnlySpan<NoteModel> notes, ReadOnlySpan<PianoSoundValueModel> value)
            => new EditNotesSoundsPropertyOperation(chart, notes.ToImmutableArray(), value.ToImmutableArray());

        public sealed class AddNoteOperation : IUndoableOperation
        {
            private readonly ChartModel _chart;
            private readonly int _insertIndex;
            private readonly NoteModel _note;
            private readonly int _tailInsertIndex;
            private readonly NoteTailNode? _noteTailNode;

            private ChartModel.CollisionResult? _noteCollision;

            [MemberNotNullWhen(true, nameof(_noteTailNode))]
            private bool HasTail => _tailInsertIndex >= 0;

            internal AddNoteOperation(ChartModel chart, NoteModel note)
            {
                ReadOnlySpan<IStageNoteNode> notes = chart._visibleNoteNodes.AsSpan();
                int index;
                int tailIndex;
                if (note.IsHold) {
                    tailIndex = notes.BinarySearch(new NoteTimeComparable(note.EndTime));
                    NumberUtils.FlipNegative(ref tailIndex);
                    index = notes[..tailIndex].LinearSearchFromEnd(new NoteTimeComparable(note.Time));
                    NumberUtils.FlipNegative(ref index);
                }
                else {
                    tailIndex = -1;
                    index = notes.BinarySearch(new NoteTimeComparable(note.Time));
                    NumberUtils.FlipNegative(ref index);
                }

                _insertIndex = index;
                _chart = chart;
                _note = note;
                _tailInsertIndex = tailIndex;
                if (_tailInsertIndex >= 0)
                    _noteTailNode = new NoteTailNode(note);
            }

            private Action<NoteModel>? _onRedone, _onUndone;
            public AddNoteOperation OnRedone(Action<NoteModel> action)
            {
                _onRedone = action;
                return this;
            }
            public AddNoteOperation OnUndone(Action<NoteModel> action)
            {
                _onUndone = action;
                return this;
            }

            void IUndoableOperation.Redo()
            {
                if (HasTail) {
                    _chart._visibleNoteNodes.Insert(_tailInsertIndex, _noteTailNode);
                    _chart._holdCount++;
                }
                _chart._visibleNoteNodes.Insert(_insertIndex, _note);

                _noteCollision ??= _chart.GetCollidedNotesTo(noteModelIndex: _insertIndex);
                _noteCollision.GetValueOrDefault().IncrementModelCollsionCounts();

                _onRedone?.Invoke(_note);
            }

            void IUndoableOperation.Undo()
            {
                Debug.Assert(_chart._visibleNoteNodes[_insertIndex] == _note);

                _chart._visibleNoteNodes.RemoveAt(_insertIndex);
                if (HasTail) {
                    Debug.Assert(_chart._visibleNoteNodes[_tailInsertIndex] == _noteTailNode);
                    _chart._visibleNoteNodes.RemoveAt(_tailInsertIndex);
                    _chart._holdCount--;
                }

                Debug.Assert(_noteCollision is not null);
                _noteCollision!.Value.DecrementModelCollisionCounts();

                _onUndone?.Invoke(_note);
            }
        }

        public sealed class AddMultipleNotesOperation : IUndoableOperation
        {
            private readonly ChartModel _chart;
            private readonly ImmutableArray<int> _indices;
            private readonly ImmutableArray<IStageNoteNode> _noteNodes;
            private readonly int _holdCount;

            private readonly ChartModel.CollisionResult?[] _collisions;

            internal AddMultipleNotesOperation(ChartModel chart, ImmutableArray<NoteModel> notes, int holdCount)
            {
                NoteTimeComparer.AssertInOrder(notes);
                Debug.Assert(notes.ToArray().Count(n => n.IsHold) == holdCount);

                // Optimize: 我懒。。
                IStageNoteNode[] insertNotes;
                {
                    var insertNoteList = new List<IStageNoteNode>(notes.Length + holdCount);
                    var modifier = insertNoteList.GetSortedModifier(NoteTimeComparer.Instance);
                    foreach (var note in notes) {
                        modifier.Add(note);
                        if (note.IsHold)
                            modifier.Add(new NoteTailNode(note));
                    }
                    insertNotes = insertNoteList.ToArray();
                }

                ReadOnlySpan<IStageNoteNode> visibleNotes = chart._visibleNoteNodes.AsSpan();
                var insertIndices = new int[insertNotes.Length];
                {
                    int index = visibleNotes.BinarySearch(new NoteTimeComparable(insertNotes[^1].Time));
                    NumberUtils.FlipNegative(ref index);

                    for (int i = insertNotes.Length - 1; i >= 0; i--) {
                        var note = notes[i];
                        index = visibleNotes[..index].LinearSearchFromEnd(new NoteTimeComparable(note.Time));
                        NumberUtils.FlipNegative(ref index);
                        insertIndices[i] = index;
                    }
                }

                _chart = chart;
                _indices = ImmutableCollectionsMarshal.AsImmutableArray(insertIndices);
                _noteNodes = ImmutableCollectionsMarshal.AsImmutableArray(insertNotes);
                _holdCount = holdCount;
                _collisions = new ChartModel.CollisionResult?[insertIndices.Length];
            }

            private Action<ImmutableArray<IStageNoteNode>>? _onRedone, _onUndone;
            public AddMultipleNotesOperation OnRedone(Action<ImmutableArray<IStageNoteNode>> action)
            {
                _onRedone = action;
                return this;
            }
            public AddMultipleNotesOperation OnUndone(Action<ImmutableArray<IStageNoteNode>> action)
            {
                _onUndone = action;
                return this;
            }

            void IUndoableOperation.Redo()
            {
                for (int i = 0; i < _noteNodes.Length; i++) {
                    var node = _noteNodes[i];
                    // We need to maually adjust offset when insert from start
                    var insertIndex = _indices[i] + i;
                    _chart._visibleNoteNodes.Insert(insertIndex, node);
                    if (node is NoteModel) {
                        // Check collision
                        ref var collision = ref _collisions[i];
                        collision ??= _chart.GetCollidedNotesTo(insertIndex);
                        collision.GetValueOrDefault().IncrementModelCollsionCounts();
                    }
                }
                _chart._holdCount += _holdCount;
                _onRedone?.Invoke(_noteNodes);
            }

            void IUndoableOperation.Undo()
            {
                for (int i = _noteNodes.Length - 1; i >= 0; i--) {
                    var insertIndex = _indices[i] + i;
                    Debug.Assert(_chart._visibleNoteNodes[insertIndex] == _noteNodes[i]);
                    _chart._visibleNoteNodes.RemoveAt(_indices[i] + i);
                }

                // Revert collision
                for (int i = 0; i < _collisions.Length; i++) {
                    var ncollision = _collisions[i];
                    if (ncollision is not { } collision)
                        continue;
                    if (collision.CollidedNotes.IsEmpty)
                        continue;
                    collision.DecrementModelCollisionCounts();
                }

                _chart._holdCount -= _holdCount;
                _onUndone?.Invoke(_noteNodes);
            }
        }

        public sealed class RemoveNotesOpeartion : IUndoableOperation
        {
            private readonly ChartModel _chart;
            private readonly ImmutableArray<int> _indices;
            private readonly ImmutableArray<IStageNoteNode> _noteNodes;
            private readonly int _holdCount;

            private readonly ChartModel.CollisionResult?[] _collisions;
            private readonly IUndoableOperation _unlinkOperations;

            internal RemoveNotesOpeartion(ChartModel chart, ImmutableArray<NoteModel> notes)
            {
                var removeIndices = new List<int>(notes.Length);
                ReadOnlySpan<IStageNoteNode> noteNodes = chart._visibleNoteNodes.AsSpan();
                int prevIndex = chart.Search(notes[0]);
                int holdCount = 0;

                if (prevIndex < 0) throw new InvalidOperationException("Trying to remove a note that is not in chart");

                foreach (var note in notes) {
                    int i = IndexOf(noteNodes[prevIndex..], note);
                    if (i < 0)
                        throw new InvalidOperationException("Trying to remove a note that is not in chart");
                    i += prevIndex;
                    Debug.Assert(ReferenceEquals(noteNodes[i], note));
                    removeIndices.GetSortedModifier().AddFromEnd(i);

                    if (note.IsHold) {
                        int tailIndex = IndexOfTail(noteNodes[i..], note);
                        Debug.Assert(tailIndex >= 0, "A note has a tail that doesnt exist in chart.");
                        removeIndices.GetSortedModifier().Add(tailIndex + i);
                        holdCount++;
                    }

                    prevIndex = i + 1;
                }

                var removeNodes = new IStageNoteNode[removeIndices.Count];

                for (int i = 0; i < removeNodes.Length; i++) {
                    var node = noteNodes[removeIndices[i]];
                    removeNodes[i] = node;
                }

                _chart = chart;
                _indices = removeIndices.ToImmutableArray();
                _noteNodes = ImmutableCollectionsMarshal.AsImmutableArray(removeNodes);
                _holdCount = holdCount;

                _collisions = new ChartModel.CollisionResult?[removeNodes.Length];
                _unlinkOperations = new EditNotesKindPropertyOperation(chart, notes, NoteModel.NoteKind.Click);

                int IndexOf(ReadOnlySpan<IStageNoteNode> nodes, NoteModel note)
                {
                    var near = nodes.LinearSearch(note, NoteTimeComparer.Instance);
                    if (near < 0)
                        return -1;
                    for (int i = near; ; i++) {
                        var n = nodes[i];
                        if (n.Time != note.Time)
                            return -1;
                        if (nodes[i] == note)
                            return i;
                    }
                }

                int IndexOfTail(ReadOnlySpan<IStageNoteNode> nodes, NoteModel note)
                {
                    Debug.Assert(note.IsHold);
                    var near = nodes.LinearSearch(new NoteTimeComparable(note.EndTime));
                    if (near < 0)
                        return -1;
                    for (int i = near; ; i++) {
                        var n = nodes[i];
                        if (n.Time != note.EndTime)
                            return -1;
                        if (n is NoteTailNode tail && ReferenceEquals(tail.HeadModel, note))
                            return i;
                    }
                }
            }

            private Action<ImmutableArray<IStageNoteNode>>? _onRedone, _onUndone;
            public RemoveNotesOpeartion OnRedone(Action<ImmutableArray<IStageNoteNode>> action)
            {
                _onRedone = action;
                return this;
            }
            public RemoveNotesOpeartion OnUndone(Action<ImmutableArray<IStageNoteNode>> action)
            {
                _onUndone = action;
                return this;
            }

            void IUndoableOperation.Redo()
            {
                _unlinkOperations.Redo();

                for (int i = _indices.Length - 1; i >= 0; i--) {
                    if (_chart._visibleNoteNodes[_indices[i]] is NoteModel) {
                        ref var collsion = ref _collisions[i];
                        collsion ??= _chart.GetCollidedNotesTo(_indices[i]);
                        collsion.Value.DecrementModelCollisionCounts();
                    }

                    _chart._visibleNoteNodes.RemoveAt(_indices[i]);
                }
                _chart._holdCount -= _holdCount;
                _onRedone?.Invoke(_noteNodes);
            }

            void IUndoableOperation.Undo()
            {
                _unlinkOperations.Undo();

                for (int i = 0; i < _noteNodes.Length; i++) {
                    var node = _noteNodes[i];
                    _chart._visibleNoteNodes.Insert(_indices[i], node);
                }

                // Revert Collision
                foreach (var ncollision in _collisions) {
                    if (ncollision is not { } collision)
                        // Related IStageNoteNode is NoteTailNode
                        continue;
                    if (collision.CollidedNotes.Length == 0)
                        continue;
                    collision.IncrementModelCollsionCounts();
                }

                _chart._holdCount += _holdCount;
                _onUndone?.Invoke(_noteNodes);
            }
        }

        public sealed class EditNotesPropertyOperation<T> : EditNotesPropertyOperationBase<T>
        {
            private readonly Action<NoteModel, T> _valueSetter;

            internal EditNotesPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, T newValue,
                Func<NoteModel, T> getter, Action<NoteModel, T> setter) :
                base(chart, notes, notes.Select(getter).ToImmutableArray(), newValue, null)
            {
                _valueSetter = setter;
            }

            internal EditNotesPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, Func<T, T> valueSelector,
                Func<NoteModel, T> getter, Action<NoteModel, T> setter) :
                base(chart, notes, notes.Select(getter).ToImmutableArray(), default!, valueSelector)
            {
                _valueSetter = setter;
            }

            protected override void SetValue(NoteModel note, T value)
            {
                _valueSetter.Invoke(note, value);
            }
        }

        public sealed class EditNotesCoordPropertyOperation : EditNotesPropertyOperationBase<NoteCoord>
        {
            [MemberNotNullWhen(true, nameof(_linkChangeInfos))]
            private bool IsEditTime { get; }

            private int[] _indicesBeforeSort;
            private int[] _indicesAfterSort;
            private LinkContext?[]? _linkChangeInfos;

            private ChartModel.CollisionResult[] _collisionsBeforeEdit;
            private ChartModel.CollisionResult[] _collisionsAfterEdit;

            internal EditNotesCoordPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, NoteCoord newValue, bool editTime) :
                base(chart, notes, notes.Select(n => n.PositionCoord).ToImmutableArray(), newValue, null)
            {
                IsEditTime = editTime;
                Initialize();
            }

            internal EditNotesCoordPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, Func<NoteCoord, NoteCoord> valueSelector, bool editTime) :
                base(chart, notes, notes.Select(n => n.PositionCoord).ToImmutableArray(), default, valueSelector)
            {
                IsEditTime = editTime;
                Initialize();
            }

            [MemberNotNull(nameof(_indicesBeforeSort), nameof(_indicesAfterSort), nameof(_collisionsBeforeEdit), nameof(_collisionsAfterEdit))]
            private void Initialize()
            {
                _indicesBeforeSort = new int[_notes.Length];
                InitCurrentIndicesTo(_indicesBeforeSort);

                _indicesAfterSort = IsEditTime ? new int[_notes.Length] : _indicesBeforeSort;

                if (IsEditTime) {
                    _linkChangeInfos = new LinkContext?[_notes.Length];
                }
                _collisionsBeforeEdit = new ChartModel.CollisionResult[_notes.Length];
                for (int i = 0; i < _notes.Length; i++)
                    _collisionsBeforeEdit[i] = _chart.GetCollidedNotesTo(_indicesBeforeSort[i]);
                _collisionsAfterEdit = new ChartModel.CollisionResult[_notes.Length];

                void InitCurrentIndicesTo(int[] indices)
                {
                    Debug.Assert(indices.Length == _notes.Length);
                    var prevIndex = _chart.Search(_notes[0]);
                    if (prevIndex < 0)
                        throw new InvalidOperationException("Editing a note that is not exist in chart");

                    ReadOnlySpan<IStageNoteNode> noteNodes = _chart._visibleNoteNodes.AsSpan();

                    for (int i = 0; i < _notes.Length; i++) {
                        var note = _notes[i];
                        var index = IndexOf(noteNodes[prevIndex..], note);
                        if (prevIndex < 0)
                            throw new InvalidOperationException("Editing a note that is not exist in chart");
                        index += prevIndex;
                        indices[i] = index;
                        prevIndex = index;
                    }

                    int IndexOf(ReadOnlySpan<IStageNoteNode> nodes, NoteModel note)
                    {
                        var near = nodes.LinearSearch(note, NoteTimeComparer.Instance);
                        if (near < 0)
                            return -1;
                        for (int i = near; ; i++) {
                            var n = nodes[i];
                            if (n.Time != note.Time)
                                return -1;
                            if (nodes[i] == note)
                                return i;
                        }
                    }
                }
            }

            protected override void SetValue(NoteModel note, NoteCoord value)
                => note.PositionCoord = value;

            protected override void OnRedoingValueChanged(bool isFirstRedo, int index, NoteCoord newValue)
            {
                if (!isFirstRedo)
                    return;
                if (!IsEditTime)
                    return;

                var fromIndex = _indicesBeforeSort[index];
                var toIndex = GetExpectedIndexAfterSort(fromIndex);
                _indicesAfterSort[index] = toIndex;

                var note = _notes[index];
                if (note.IsSlide) {
                    _linkChangeInfos[index] = GetLinkChangeContext(note);
                }

                int GetExpectedIndexAfterSort(int fromIndex)
                {
                    ReadOnlySpan<IStageNoteNode> noteNodes = _chart._visibleNoteNodes.AsSpan();
                    var node = noteNodes[fromIndex];
                    Debug.Assert(node is NoteModel, "Why are you editing a non-NoteModel instance?");

                    int newIndex = noteNodes[..fromIndex].LinearSearchFromEnd(new NoteTimeComparable(node.Time));
                    NumberUtils.FlipNegative(ref newIndex);
                    if (newIndex != fromIndex)
                        // The note is moved backward
                        return newIndex;

                    newIndex = noteNodes[(fromIndex + 1)..].LinearSearch(new NoteTimeComparable(node.Time));
                    NumberUtils.FlipNegative(ref newIndex);
                    newIndex += fromIndex;
                    if (newIndex != fromIndex)
                        // The note is move forward
                        return newIndex;

                    // The note's order isn't moved
                    return fromIndex;
                }

                static LinkContext? GetLinkChangeContext(NoteModel note)
                {
                    if (note.PrevLink is not null) {
                        // Find the first note in link that has time greater than current note
                        // And move current note before target note
                        NoteModel target = note;
                        for (; target.PrevLink != null; target = target.PrevLink) {
                            if (target.PrevLink.Time <= note.Time)
                                break;
                        }
                        if (!ReferenceEquals(target, note))
                            return new LinkContext(InsertBefore: true, note.PrevLink, target);
                    }

                    if (note.NextLink is not null) {
                        NoteModel target = note;
                        for (; target.NextLink != null; target = target.NextLink) {
                            if (target.NextLink.Time >= note.Time)
                                break;
                        }
                        if (!ReferenceEquals(target, note))
                            return new LinkContext(false, note.NextLink, target);
                    }

                    return null;
                }
            }

            protected override void OnRedone(bool isFirstRedo)
            {
                if (IsEditTime) {
                    SortNotes();
                }
                if (isFirstRedo) {
                    InitializeCollisions();
                }
                // UpdateCollisions
                for (int i = 0; i < _notes.Length; i++) {
                    _collisionsBeforeEdit[i].DecrementModelCollisionCounts();
                    _collisionsAfterEdit[i].IncrementModelCollsionCounts();
                }

                return;

                void SortNotes()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        // Sort time
                        _chart._visibleNoteNodes.MoveTo(_indicesBeforeSort[i], _indicesAfterSort[i]);

                        // Update link order
                        var curNote = _notes[i];
                        if (_linkChangeInfos[i] is var (insertBefore, _, target)) {
                            if (insertBefore)
                                curNote.InsertAsLinkBefore(target);
                            else
                                curNote.InsertAsLinkAfter(target);
                        }
                    }
                }

                void InitializeCollisions()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        _collisionsAfterEdit[i] = _chart.GetCollidedNotesTo(_indicesAfterSort[i]);
                    }
                }
            }

            protected override void OnUndone()
            {
                if (IsEditTime) {
                    RevertSort();
                }

                // Revert collisions
                for (int i = _notes.Length - 1; i >= 0; i--) {
                    _collisionsAfterEdit[i].DecrementModelCollisionCounts();
                    _collisionsBeforeEdit[i].IncrementModelCollsionCounts();
                }

                return;

                void RevertSort()
                {
                    for (int i = _notes.Length - 1; i >= 0; i--) {
                        _chart._visibleNoteNodes.MoveTo(_indicesAfterSort[i], _indicesBeforeSort[i]);

                        var curNote = _notes[i];
                        if (_linkChangeInfos[i] is var (insertBefore, target, _)) {
                            // here insertBefore has opposite meaning, see comments on LinkContext
                            if (insertBefore)
                                curNote.InsertAsLinkAfter(target);
                            else
                                curNote.InsertAsLinkBefore(target);
                        }
                    }
                }
            }

            /// <remarks>
            /// InsertBefore indicates whether the note should insert before or after NoteRefAfterSort.
            /// But when Undoing, the value is reversed, when InsertBefore is true, should insert after NoteRefBeforeSort
            /// </remarks>
            private record struct LinkContext(
                bool InsertBefore,
                NoteModel NoteRefBeforeSort,
                NoteModel NoteRefAfterSort);
        }

        public sealed class EditNotesDurationPropertyOperation : EditNotesPropertyOperationBase<float>
        {
            private readonly TailContext[] _tails;
            private int _holdCountDelta;

            internal EditNotesDurationPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, float newValue) :
                base(chart, notes, notes.Select(n => n.Duration).ToImmutableArray(), newValue, null)
            {
                _tails = new TailContext[_notes.Length];
            }

            protected override void SetValue(NoteModel note, float value)
                => note.Duration = value;

            protected override void OnRedoingValueChanged(bool isFirstRedo, int index, float newValue)
            {
                var note = _notes[index];
                var oldValue = _oldValues[index];

                if (isFirstRedo) {
                    InitializeTailContexts();
                }

                var (tail, from, to) = _tails[index];
                switch (from, to) {
                    case (-1, > 0): _chart._visibleNoteNodes.Insert(to, tail); break;
                    case ( > 0, -1): _chart._visibleNoteNodes.RemoveAt(from); break;
                    case (-1, -1): break;
                    default: _chart._visibleNoteNodes.MoveTo(from, to); break;
                }

                void InitializeTailContexts()
                {
                    switch (oldValue, newValue) {
                        case (0, not 0): {
                            var tail = new NoteTailNode(note);
                            int insertIndex = _chart._visibleNoteNodes.BinarySearch(tail, NoteTimeComparer.Instance);
                            _tails[index] = new(tail, -1, insertIndex);
                            _holdCountDelta++;
                            break;
                        }
                        case (not 0, 0): {
                            int tailIndex = _chart.SearchTailOf(note);
                            _tails[index] = new((NoteTailNode)_chart._visibleNoteNodes[tailIndex], tailIndex, -1);
                            _holdCountDelta--;
                            break;
                        }
                        default: {
                            if (oldValue == newValue) {
                                _tails[index] = new(null!, -1, -1);
                                break;
                            }

                            ReadOnlySpan<IStageNoteNode> noteNodes = _chart._visibleNoteNodes.AsSpan();
                            int tailIndex = _chart.SearchTailOf(note);
                            var tail = (NoteTailNode)noteNodes[tailIndex];
                            int toIndex;
                            if (oldValue < newValue) {
                                toIndex = noteNodes[tailIndex..].LinearSearch(tail, NoteTimeComparer.Instance);
                                NumberUtils.FlipNegative(ref toIndex);
                                toIndex += tailIndex;
                            }
                            else {
                                toIndex = noteNodes[..tailIndex].LinearSearchFromEnd(tail, NoteTimeComparer.Instance);
                            }
                            _tails[index] = new(tail, tailIndex, toIndex);
                            break;
                        }
                    }
                }
            }

            protected override void OnUndoingValueChanged(int index)
            {
                var (tail, from, to) = _tails[index];
                switch (from, to) {
                    case (-1, > 0): _chart._visibleNoteNodes.RemoveAt(to); break;
                    case ( > 0, -1): _chart._visibleNoteNodes.Insert(from, tail); break;
                    case (-1, -1): break;
                    default: _chart._visibleNoteNodes.MoveTo(to, from); break;
                }
            }

            protected override void OnRedone(bool isFirstRedo) => _chart._holdCount += _holdCountDelta;

            protected override void OnUndone() => _chart._holdCount -= _holdCountDelta;

            private readonly record struct TailContext(
                NoteTailNode Tail,
                int FromIndex,
                int ToIndex);
        }

        public sealed class EditNotesKindPropertyOperation : EditNotesPropertyOperationBase<NoteModel.NoteKind>
        {
            private readonly LinkContext[] _oldLinkContexts;

            public EditNotesKindPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, NoteModel.NoteKind newValue) :
                base(chart, notes, notes.Select(n => n.Kind).ToImmutableArray(), newValue, null)
            {
                var oldLinks = new LinkContext[notes.Length];
                for (int i = 0; i < notes.Length; i++) {
                    var note = notes[i];
                    oldLinks[i] = new LinkContext(note.PrevLink, note.NextLink);
                }
                _oldLinkContexts = oldLinks;
            }

            protected override void SetValue(NoteModel note, NoteModel.NoteKind value) => note.Kind = value;

            protected override void OnRedoingValueChanging(bool isFirstRedo, int index, NoteModel.NoteKind newValue)
            {
                var note = _notes[index];
                if (note.IsSlide) {
                    note.UnlinkWithoutCutChain();
                }
            }

            protected override void OnRedoingValueChanged(bool isFirstRedo, int index, NoteModel.NoteKind newValue)
            {
                if (newValue is NoteModel.NoteKind.Slide) {
                    if (index > 0) {
                        var note = _notes[index];
                        var prev = _notes[index - 1];
                        note._prevLink = prev;
                        prev._nextLink = note;
                    }
                }
            }

            protected override void OnUndoingValueChanged(int index)
            {
                var note = _notes[index];
                var (prev, next) = _oldLinkContexts[index];
                if (note.IsSlide) {
                    note._prevLink = prev;
                    if (prev is not null)
                        prev._nextLink = note;
                    note._nextLink = note;
                    if (next is not null)
                        next._prevLink = note;
                }
                else {
                    if (note.PrevLink != prev) {
                        note._prevLink = prev;
                        if (prev is not null)
                            prev._nextLink = note;
                    }
                    if (note.NextLink != next) {
                        note._nextLink = next;
                        if (next is not null)
                            next._prevLink = note;
                    }
                }
            }

            private readonly record struct LinkContext(
                 NoteModel? PrevLink,
                 NoteModel? NextLink);
        }

        public sealed class EditNotesSoundsPropertyOperation : EditNotesPropertyOperationBase<ImmutableArray<PianoSoundValueModel>>
        {
            internal EditNotesSoundsPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, ImmutableArray<PianoSoundValueModel> newValue) :
                base(chart, notes,
                    notes.Select(n => n.HasSounds ? n.Sounds.ToImmutableArray() : ImmutableArray<PianoSoundValueModel>.Empty).ToImmutableArray(),
                    newValue, null)
            { }

            protected override void SetValue(NoteModel note, ImmutableArray<PianoSoundValueModel> value)
            {
                var sounds = note.Sounds;
                sounds.Clear();
                foreach (var s in value) {
                    sounds.Add(s);
                }
            }
        }
    }
}