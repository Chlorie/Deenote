#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static Deenote.Entities.Storage.DsprojLoader;

namespace Deenote.Entities.Operations
{
    partial class ModelOperations
    {
        public static AddNoteOperation AddNote(this ChartModel chart, NoteModel note)
            => new(chart, note);

        public static AddMultipleNotesOperation AddMultipleNotes(this ChartModel chart, ImmutableArray<NoteModel> notes)
        {
            return new AddMultipleNotesOperation(chart, notes);
        }

        public static RemoveNotesOperation RemoveOrderedNotes(this ChartModel chart, ImmutableArray<NoteModel> notes)
        {
            NodeTimeComparer.AssertInOrder(notes);
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
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), new(old => old with { Time = value }), true);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesTime(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<float, float> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), new(old => old with { Time = valueSelector(old.Time) }), true);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesPosition(this ChartModel chart, ReadOnlySpan<NoteModel> notes, float value)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), new(old => old with { Position = value }), false);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesPosition(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<float, float> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), new(old => old with { Position = valueSelector(old.Position) }), false);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesCoord(this ChartModel chart, ReadOnlySpan<NoteModel> notes, NoteCoord value)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), value);

        public static EditNotesPropertyOperationBase<NoteCoord> EditNotesCoord(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<NoteCoord, NoteCoord> valueSelector)
            => new EditNotesCoordPropertyOperation(chart, notes.ToImmutableArray(), valueSelector, true);

        public static EditNotesPropertyOperationBase<float> EditNotesDuration(this ChartModel chart, ReadOnlySpan<NoteModel> notes, float value)
            => new EditNotesDurationPropertyOperation(chart, notes.ToImmutableArray(), value);

        public static EditNotesPropertyOperationBase<float> EditNotesDuration(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<float, float> valueSelector)
            => new EditNotesDurationPropertyOperation(chart, notes.ToImmutableArray(), valueSelector);

        public static EditNotesPropertyOperationBase<float> EditNotesEndTime(this ChartModel chart, ReadOnlySpan<NoteModel> notes, Func<float, float> valueSelector)
            => new EditNotesEndTimePropertyOperation(chart, notes.ToImmutableArray(), valueSelector);

        public static EditNotesPropertyOperationBase<NoteModel.NoteKind> EditNotesKind(this ChartModel chart, ReadOnlySpan<NoteModel> notes, NoteModel.NoteKind value)
            => new EditNotesKindPropertyOperation(chart, notes.ToImmutableArray(), value);

        public static EditNotesPropertyOperationBase<ImmutableArray<PianoSoundValueModel>> EditNotesSounds(this ChartModel chart, ReadOnlySpan<NoteModel> notes, ReadOnlySpan<PianoSoundValueModel> value)
            => new EditNotesSoundsPropertyOperation(chart, notes.ToImmutableArray(), value.ToImmutableArray());

        public sealed class AddNoteOperation : NotifiableChartOperation<NoteModel>
        {
            private readonly NoteModel _note;

            private NoteTailNode? _tail;
            private ChartModel.CollisionResult? _collision;

            internal AddNoteOperation(ChartModel chart, NoteModel note) : base(chart)
            {
                _note = note;
            }

            protected override void Redo()
            {
                var index = _chart.NoteNodes.Add(_note);
                if (_note.IsHold) {
                    _tail ??= new NoteTailNode(_note);
                    _chart.NoteNodes.AddFrom(index, _tail);
                    _chart._holdCount++;
                }

                _collision ??= _chart.GetCollidedNotesTo(_note);
                _collision.Value.IncrementModelCollsionCounts();

                OnRedone(_note);
            }

            protected override void Undo()
            {
                if (_tail is not null) {
                    _chart.NoteNodes.Remove(_tail);
                    _chart._holdCount--;
                }
                _chart.NoteNodes.Remove(_note);

                _collision!.Value.DecrementModelCollisionCounts();

                OnUndone(_note);
            }
        }

        public sealed class AddMultipleNotesOperation : NotifiableChartOperation<ImmutableArray<NoteModel>>
        {
            private readonly ImmutableArray<NoteModel> _notes;

            private List<NoteTailNode>? _tails;
            private List<ChartModel.CollisionResult>? _collisions;

            internal AddMultipleNotesOperation(ChartModel chart, ImmutableArray<NoteModel> notes) : base(chart)
            {
                _notes = notes;
            }

            protected override void Redo()
            {
                if (_tails is null) {
                    RedoWithInit();
                }
                else {
                    _chart.NoteNodes.AddRange(_notes.AsSpan());
                    _chart.NoteNodes.AddRange(_tails.AsSpan());
                    if (_collisions is not null) {
                        foreach (var collision in _collisions) {
                            collision.IncrementModelCollsionCounts();
                        }
                    }
                    _chart._holdCount += _tails.Count;
                }

                OnRedone(_notes);
            }

            private void RedoWithInit()
            {
                _tails = new();
                foreach (var note in _notes) {
                    _chart.NoteNodes.Add(note);
                    if (note.IsHold) {
                        var tail = new NoteTailNode(note);
                        _tails.Add(tail);
                        _chart.NoteNodes.Add(tail);
                    }
                    var collision = _chart.GetCollidedNotesTo(note);
                    if (collision.CollidedNotes.Length > 0) {
                        (_collisions ??= new()).Add(collision);
                        collision.IncrementModelCollsionCounts();
                    }
                }
                _chart._holdCount += _tails.Count;
            }

            protected override void Undo()
            {
                _chart.NoteNodes.RemoveRange(_notes.AsSpan());
                _chart.NoteNodes.RemoveRange(_tails!.AsSpan());
                if (_collisions is not null) {
                    foreach (var collision in _collisions) {
                        collision.DecrementModelCollisionCounts();
                    }
                }
                _chart._holdCount -= _tails!.Count;

                OnUndone(_notes);
            }
        }

        public sealed class RemoveNotesOperation : NotifiableChartOperation<ImmutableArray<NoteModel>>
        {
            private readonly ImmutableArray<NoteModel> _notes;

            private IUndoableOperation _unlinkOperation;
            private List<NoteTailNode>? _tails;
            private List<ChartModel.CollisionResult>? _collisions;

            internal RemoveNotesOperation(ChartModel chart, ImmutableArray<NoteModel> notes) : base(chart)
            {
                _notes = notes;
                _unlinkOperation = new EditNotesKindPropertyOperation(_chart, _notes, NoteModel.NoteKind.Click);
            }

            protected override void Redo()
            {
                _unlinkOperation.Redo();

                if (_tails is null) {
                    RedoWithInit();
                }
                else {
                    if (_collisions is not null) {
                        foreach (var collision in _collisions) {
                            collision.DecrementModelCollisionCounts();
                        }
                    }
                    _chart.NoteNodes.RemoveRange(_tails.AsSpan());
                    _chart.NoteNodes.RemoveRange(_notes.AsSpan());
                    _chart._holdCount -= _tails.Count;
                }

                OnRedone(_notes);
            }

            private void RedoWithInit()
            {
                _tails = new();
                foreach (var note in _notes) {
                    var collision = _chart.GetCollidedNotesTo(note);
                    if (!collision.CollidedNotes.IsEmpty) {
                        (_collisions ??= new()).Add(collision);
                        collision.DecrementModelCollisionCounts();
                    }
                    if (note.IsHold) {
                        var tail = _chart.FindTailOf(note);
                        Debug.Assert(tail is not null);
                        _tails.Add(tail!);
                        _chart.NoteNodes.Remove(tail!);
                    }
                    _chart.NoteNodes.Remove(note);
                }
                _chart._holdCount -= _tails.Count;
            }

            protected override void Undo()
            {
                _unlinkOperation.Undo();

                if (_collisions is not null) {
                    foreach (var collision in _collisions) {
                        collision.IncrementModelCollsionCounts();
                    }
                }
                _chart.NoteNodes.AddRange(_tails!.AsSpan());
                _chart.NoteNodes.AddRange(_notes.AsSpan());
                _chart._holdCount += _tails!.Count;

                OnUndone(_notes);
            }
        }

        public sealed class EditNotesPropertyOperation<T> : EditNotesPropertyOperationBase<T>
        {
            private readonly Action<NoteModel, T> _valueSetter;

            internal EditNotesPropertyOperation(ChartModel chart,
                ImmutableArray<NoteModel> notes, ValueProvider newValue,
                Func<NoteModel, T> getter, Action<NoteModel, T> setter) :
                base(chart, notes, getter, newValue)
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
            private readonly bool IsEditTime;

            private LinkContext?[]? _linkChangeInfo;
            private List<ChartModel.CollisionResult>? _collisionsBefore;
            private List<ChartModel.CollisionResult>? _collisionsAfter;

            internal EditNotesCoordPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes,
                ValueProvider newValue, bool editTime = true) :
                base(chart, notes, n => n.PositionCoord, newValue)
            {
                IsEditTime = editTime;
            }

            protected override void SetValue(NoteModel note, NoteCoord value)
                => note.PositionCoord = value;

            protected override void OnRedoing(bool isFirstRedo)
            {
                if (IsEditTime) {
                    _linkChangeInfo = new LinkContext?[_notes.Length];
                }

                foreach (var note in _notes) {
                    var collision = _chart.GetCollidedNotesTo(note);
                    if (!collision.CollidedNotes.IsEmpty)
                        (_collisionsBefore ??= new()).Add(collision);
                }
            }

            protected override void OnRedoingValueChanged(bool isFirstRedo, int index, NoteCoord newValue)
            {
                if (!isFirstRedo) return;
                if (!IsEditTime) return;

                var note = _notes[index];
                if (note.IsSlide) {
                    Debug.Assert(_linkChangeInfo is not null);
                    _linkChangeInfo![index] = GetLinkChangeContext(note);
                }

                static LinkContext? GetLinkChangeContext(NoteModel note)
                {
                    if (note.PrevLink is not null) {
                        // Find the first note in link that has time greater than current note
                        // And move current note before target note
                        NoteModel target = note;
                        for (; target.PrevLink != null; target = target.PrevLink) {
                            if (NodeTimeUniqueComparer.Instance.Compare(target.PrevLink, note) <= 0)
                                break;
                        }
                        if (target != note)
                            return new LinkContext(InsertBefore: true, note.PrevLink, target);
                    }

                    if (note.NextLink is not null) {
                        NoteModel target = note;
                        for (; target.NextLink != null; target = target.NextLink) {
                            if (NodeTimeUniqueComparer.Instance.Compare(target.NextLink, note) >= 0)
                                break;
                        }
                        if (target != note)
                            return new LinkContext(false, note.NextLink, target);
                    }

                    return null;
                }
            }

            protected override void OnRedone(bool isFirstRedo)
            {
                if (IsEditTime) {
                    _chart.NoteNodes.ResortBubble();

                    for (int i = 0; i < _notes.Length; i++) {
                        var curNote = _notes[i];
                        if (_linkChangeInfo![i] is var (before, _, tgt)) {
                            if (before)
                                curNote.InsertAsLinkBefore(tgt);
                            else
                                curNote.InsertAsLinkAfter(tgt);
                        }
                    }
                }

                if (isFirstRedo) {
                    foreach (var note in _notes) {
                        var collision = _chart.GetCollidedNotesTo(note);
                        if (!collision.CollidedNotes.IsEmpty) {
                            (_collisionsAfter ??= new()).Add(collision);
                        }
                    }
                }
                if (_collisionsBefore is not null) {
                    foreach (var collision in _collisionsBefore) {
                        collision.DecrementModelCollisionCounts();
                    }
                }
                if (_collisionsAfter is not null) {
                    foreach (var collision in _collisionsAfter) {
                        collision.IncrementModelCollsionCounts();
                    }
                }
            }

            protected override void OnUndone()
            {
                if (IsEditTime) {
                    _chart.NoteNodes.ResortBubble();
                    for (int i = _notes.Length - 1; i >= 0; i--) {
                        var curNote = _notes[i];
                        if (_linkChangeInfo![i] is var (before, tgt, _)) {
                            // here insertBefore has opposite meaning, see comments on LinkContext
                            if (before)
                                curNote.InsertAsLinkAfter(tgt);
                            else
                                curNote.InsertAsLinkBefore(tgt);
                        }
                    }
                }

                if (_collisionsBefore is not null) {
                    foreach (var collision in _collisionsBefore) {
                        collision.IncrementModelCollsionCounts();
                    }
                }
                if (_collisionsAfter is not null) {
                    foreach (var collision in _collisionsAfter) {
                        collision.DecrementModelCollisionCounts();
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

        public abstract class EditNotesDurationPropertyOperationBase<T> : EditNotesPropertyOperationBase<T>
        {
            private readonly TailContext[] _tails;
            private int _holdCountDelta;

            protected EditNotesDurationPropertyOperationBase(ChartModel chart, ImmutableArray<NoteModel> notes,
                Func<NoteModel, T> oldValueSelector, ValueProvider newValue) :
                base(chart, notes, oldValueSelector, newValue)
            {
                _tails = new TailContext[notes.Length];
                _holdCountDelta = 0;
            }

            protected sealed override void SetValue(NoteModel note, T value)
                => note.Duration = GetDurationValue(note, value);

            private SortedList<IStageNoteNode>.TrackingScope _tmpScope;

            protected sealed override void OnRedoingValueChanging(bool isFirstRedo, int index, T newValue)
            {
                var note = _notes[index];
                var oldValue = GetDurationValue(note, _oldValues[index]);

                if (isFirstRedo) {
                    InitializeTailContexts(GetDurationValue(note, newValue));
                }

                var (tail, kind) = _tails[index];
                switch (kind) {
                    case TailModifyKind.Add: break;
                    case TailModifyKind.Remove: _chart.NoteNodes.Remove(tail); break;
                    case TailModifyKind.Move: _tmpScope = _chart.NoteNodes.EnterTrackingScope(tail); break;
                }

                void InitializeTailContexts(float newDurationValue)
                {
                    switch (oldValue, newDurationValue) {
                        case (0, not 0): {
                            var tail = new NoteTailNode(note);
                            _tails[index] = new(tail, TailModifyKind.Add);
                            _holdCountDelta++;
                            break;
                        }
                        case (not 0, 0): {
                            var tail = _chart.FindTailOf(note);
                            Debug.Assert(tail is not null);
                            _tails[index] = new(tail!, TailModifyKind.Remove);
                            _holdCountDelta--;
                            break;
                        }
                        default: {
                            if (oldValue == newDurationValue) {
                                _tails[index] = new(null!, TailModifyKind.None);
                            }
                            else {
                                var tail = _chart.FindTailOf(note);
                                Debug.Assert(tail is not null);
                                _tails[index] = new(tail!, TailModifyKind.Move);
                            }
                            break;
                        }
                    }
                }
            }

            protected sealed override void OnRedoingValueChanged(bool isFirstRedo, int index, T newValue)
            {
                var (tail, kind) = _tails[index];
                switch (kind) {
                    case TailModifyKind.Add: _chart.NoteNodes.Add(tail); break;
                    case TailModifyKind.Remove: break;
                    case TailModifyKind.Move: _tmpScope.Dispose(); break;
                }
            }

            protected sealed override void OnUndoingValueChanging(int index)
            {
                var (tail, kind) = _tails[index];
                switch (kind) {
                    case TailModifyKind.Add: _chart.NoteNodes.Remove(tail); break;
                    case TailModifyKind.Remove: break;
                    case TailModifyKind.Move: _tmpScope = _chart.NoteNodes.EnterTrackingScope(tail); break;
                }
            }

            protected sealed override void OnUndoingValueChanged(int index)
            {
                var (tail, kind) = _tails[index];
                switch (kind) {
                    case TailModifyKind.Add: break;
                    case TailModifyKind.Remove: _chart.NoteNodes.Add(tail); break;
                    case TailModifyKind.Move: _tmpScope.Dispose(); break;
                }
            }

            protected sealed override void OnRedone(bool isFirstRedo)
            {
                _chart._holdCount += _holdCountDelta;
            }

            protected sealed override void OnUndone()
            {
                _chart._holdCount -= _holdCountDelta;
            }

            protected abstract float GetDurationValue(NoteModel note, T value);

            private readonly record struct TailContext(
                NoteTailNode Tail,
                TailModifyKind ModifyKind);

            private enum TailModifyKind { None, Add, Remove, Move, }
        }

        public sealed class EditNotesDurationPropertyOperation : EditNotesDurationPropertyOperationBase<float>
        {
            internal EditNotesDurationPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes,
                ValueProvider newValue) :
                base(chart, notes, n => n.Duration, newValue)
            { }

            protected override float GetDurationValue(NoteModel note, float value) => value;
        }

        public sealed class EditNotesEndTimePropertyOperation : EditNotesDurationPropertyOperationBase<float>
        {
            internal EditNotesEndTimePropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes,
                ValueProvider newValue) :
                base(chart, notes, n => n.EndTime, newValue)
            { }

            // HACK: For consistency, here we should not use Mathf.Max(), and should call it in StageChartEditor.
            // To achive this, the implementation of ValueProvider requires changing, as we should pass the NoteModel as argument to selector delegate
            protected override float GetDurationValue(NoteModel note, float value) => Mathf.Max(0f, value - note.Time);
        }

        public sealed class EditNotesKindPropertyOperation : EditNotesPropertyOperationBase<NoteModel.NoteKind>
        {
            private readonly ImmutableArray<LinkContext> _oldLinkContexts;

            public EditNotesKindPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes, NoteModel.NoteKind newValue) :
                base(chart, notes, notes.Select(n => n.Kind).ToImmutableArray(), newValue)
            {
                var oldLinks = new LinkContext[notes.Length];
                for (int i = 0; i < notes.Length; i++) {
                    var note = notes[i];
                    oldLinks[i] = new LinkContext(note.PrevLink, note.NextLink);
                }
                _oldLinkContexts = ImmutableCollectionsMarshal.AsImmutableArray(oldLinks);
            }

            protected override void SetValue(NoteModel note, NoteModel.NoteKind value)
                => note.Kind = value;

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
                        prev._nextLink = note;
                        note._prevLink = prev;
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
                    note._nextLink = next;
                    if (next is not null)
                        next._prevLink = note;
                }
                else {
                    // Maybe here is setting link to null, but I forgot why I've wrote such a complex segment
                    if (note._prevLink != prev) {
                        note._prevLink = prev;
                        if (prev is not null)
                            prev._nextLink = note;
                    }
                    if (note._nextLink != next) {
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
            internal EditNotesSoundsPropertyOperation(ChartModel chart, ImmutableArray<NoteModel> notes,
                ImmutableArray<PianoSoundValueModel> newValue) :
                base(chart, notes, n => n.HasSounds ? n.Sounds.ToImmutableArray() : ImmutableArray<PianoSoundValueModel>.Empty, newValue)
            { }

            protected override void SetValue(NoteModel note, ImmutableArray<PianoSoundValueModel> value)
            {
                var sounds = note.Sounds;
                sounds.Clear();
                sounds.AddRange(value.AsSpan());
            }
        }
    }
}