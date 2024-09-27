using Deenote.Edit.Operations;
using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote.Project.Models
{
    partial class ChartModel
    {
        partial struct NoteModelListProxy
        {
            public AddNoteOperation AddNote(NoteCoord coord, NoteData notePrototype)
            {
                NoteData data = notePrototype.Clone();
                data.Position = coord.Position;
                data.Time = coord.Time;

                var noteModels = _chartModel._visibleNotes;
                int iModel = noteModels.Count - 1;
                int iTailModel;
                if (data.IsHold) {
                    iTailModel = iModel;
                    IterateUntil(ref iTailModel, coord.Time + data.Duration);
                    iModel = iTailModel - 1;
                }
                else {
                    iTailModel = -1;
                }

                IterateUntil(ref iModel, coord.Time);

                return new AddNoteOperation(iModel, _chartModel, data, iTailModel);

                void IterateUntil(ref int index, float compareTime)
                {
                    for (; index >= 0; index--) {
                        if (noteModels[index].Time <= compareTime) {
                            break;
                        }
                    }
                    index++;
                }
            }

            public AddMultipleNotesOperation AddMultipleNotes(NoteCoord baseCoord,
                ReadOnlySpan<NoteData> notePrototypes)
            {
                NoteTimeComparer.AssertInOrder(notePrototypes);

                List<IStageNoteModel> insertModels = new(notePrototypes.Length);
                for (int i = 0; i < notePrototypes.Length; i++) {
                    var insertNote = notePrototypes[i].Clone();
                    var coord = NoteCoord.ClampPosition(baseCoord + insertNote.PositionCoord);
                    insertNote.PositionCoord = coord;

                    var noteModel = new NoteModel(insertNote);
                    insertModels.Add(noteModel);
                    if (noteModel.Data.IsHold)
                        insertModels.Add(new NoteTailModel(noteModel));
                }

                Utils.InsertionSortAsc(insertModels.AsSpan(), NoteTimeComparer.Instance);

                int[] insertIndices = new int[notePrototypes.Length];

                int iModel = _chartModel.Notes.Count - 1;
                for (int i = notePrototypes.Length - 1; i >= 0; i--) {
                    var note = insertModels[i];

                    while (iModel >= 0 && _chartModel._visibleNotes[iModel].Time > note.Time)
                        iModel--;
                    insertIndices[i] = iModel + 1;
                }

                return new AddMultipleNotesOperation(insertIndices, _chartModel, insertModels.AsMemory());
            }

            public RemoveNotesOperation RemoveNotes(ReadOnlySpan<NoteModel> notesInTimeOrder)
            {
                NoteTimeComparer.AssertInOrder(notesInTimeOrder);
                var notemodels = _chartModel._visibleNotes;

                // Record all visible notes, mark their indices
                var removeIndices = new List<int>(notesInTimeOrder.Length);
                int iModel = 0;
                foreach (var note in notesInTimeOrder) {
                    int i = notemodels.IndexOf(note, iModel);
                    Debug.Assert(i >= 0, "Remove a note that doesn't on stage");
                    removeIndices.Add(i);

                    if (note.Data.IsHold) {
                        int iTail = _chartModel.Notes.SearchTailOf(i);
                        Debug.Assert(iTail >= 0, "Remove a note tail that doesn't on stage");
                        removeIndices.Add(iTail);
                    }

                    iModel = i + 1;
                }

                Utils.InsertionSortAsc(removeIndices.AsSpan());

                return new RemoveNotesOperation(removeIndices.AsMemory(), _chartModel);
            }

            public LinkNotesOperation LinkNotes(ReadOnlySpan<NoteModel> notes)
            {
                var editNotes = new NoteModel[notes.Length];
                for (int i = 0; i < notes.Length; i++) {
                    editNotes[i] = notes[i];
                }

                return new LinkNotesOperation(editNotes);
            }

            public UnlinkNotesOperation UnlinkNotes(ReadOnlySpan<NoteModel> notes)
            {
                var editNotes = new NoteModel[notes.Length];
                for (int i = 0; i < notes.Length; i++) {
                    editNotes[i] = notes[i];
                }

                return new UnlinkNotesOperation(editNotes);
            }

            public EditNotesPropertyOperation<T> EditNotes<T>(ReadOnlySpan<NoteModel> notes, T value,
                Func<NoteData, T> valueGetter, Action<NoteData, T> valueSetter)
            {
                var contexts = new (NoteModel Note, T OldValue)[notes.Length];
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, valueGetter(note.Data));
                }
                return new EditNotesPropertyOperation<T>(_chartModel, contexts, value, valueSetter);
            }

            public EditNotesPropertyOperation<T> EditNotes<T>(ReadOnlySpan<NoteModel> notes,
                Func<T, T> valueSelector, Func<NoteData, T> valueGetter, Action<NoteData, T> valueSetter)
            {
                var contexts = new (NoteModel Note, T OldValue)[notes.Length];
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, valueGetter(notes[i].Data));
                }
                return new EditNotesPropertyOperation<T>(_chartModel, contexts, valueSelector, valueSetter);
            }

            public EditNotesCoordPropertyOperation EditNotesCoord(ReadOnlySpan<NoteModel> notes,
                NoteCoord newCoord, bool editingTime)
            {
                var contexts = new (NoteModel Note, NoteCoord OldValue)[notes.Length];
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, note.Data.PositionCoord);
                }
                return new EditNotesCoordPropertyOperation(_chartModel, contexts, newCoord, editingTime);
            }

            public EditNotesCoordPropertyOperation EditNotesCoord(ReadOnlySpan<NoteModel> notes,
                Func<NoteCoord, NoteCoord> valueSelector, bool editingTime)
            {
                var contexts = new (NoteModel Note, NoteCoord OldValue)[notes.Length];
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, note.Data.PositionCoord);
                }
                return new EditNotesCoordPropertyOperation(_chartModel, contexts, valueSelector, editingTime);
            }

            public EditNotesDurationPropertyOperation EditNotesDuration(ReadOnlySpan<NoteModel> notes, float newDuration)
            {
                var contexts = new (NoteModel Note, float OldValue)[notes.Length];
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, note.Data.Duration);
                }
                return new EditNotesDurationPropertyOperation(_chartModel, contexts, newDuration);
            }

            public EditNotesSoundsOperation EditNotesSounds(ReadOnlySpan<NoteModel> notes,
                ReadOnlySpan<PianoSoundValueData> valueData)
            {
                var contexts = new (NoteModel, PianoSoundValueData[])[notes.Length];
                for (int i = 0; i < notes.Length; i++) {
                    var note = notes[i];
                    var sounds = note.Data.Sounds;
                    var datas = Utils.Array<PianoSoundValueData>(sounds.Count);
                    for (int j = 0; j < sounds.Count; i++) {
                        datas[j] = sounds[i].GetValues();
                    }
                    contexts[i] = (note, datas);
                }

                return new EditNotesSoundsOperation(_chartModel, contexts, valueData);
            }

            #region Operation implementation

            public sealed class AddNoteOperation : IUndoableOperation
            {
                private readonly int _modelInsertIndex;
                private readonly ChartModel _chartModel;
                private readonly NoteModel _note;
                private readonly int _holdTailInsertIndex;
                private readonly NoteTailModel? _noteTailModel;

                private Action? _onRedone;
                private Action? _onUndone;

                private ReadOnlyMemory<NoteModel>? _collidedNotes;

                private bool RequiresInsertTail => _holdTailInsertIndex >= 0;

                // Unity 什么时候支持 C#12.jpg
                public AddNoteOperation(int modelInsertIndex, ChartModel chartModel, NoteData note, int holdTailInsertIndex)
                {
                    Debug.Assert((holdTailInsertIndex >= 0) == (note.IsHold));

                    _modelInsertIndex = modelInsertIndex;
                    _chartModel = chartModel;
                    _note = new NoteModel(note);
                    _holdTailInsertIndex = holdTailInsertIndex;
                    if (_holdTailInsertIndex >= 0)
                        _noteTailModel = new NoteTailModel(_note);
                }

                public AddNoteOperation WithRedoneAction(Action action)
                {
                    _onRedone = action;
                    return this;
                }

                public AddNoteOperation WithUndoneAction(Action action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    if (RequiresInsertTail) {
                        _chartModel._visibleNotes.Insert(_holdTailInsertIndex, _noteTailModel);
                        _chartModel._holdCount++;
                    }
                    _chartModel._visibleNotes.Insert(_modelInsertIndex, _note);

                    CheckCollision();
                    _onRedone?.Invoke();

                    void CheckCollision()
                    {
                        _collidedNotes ??= _chartModel.Notes.GetCollidedNotesTo(_modelInsertIndex);
                        var collidedNotes = _collidedNotes.Value.Span;

                        _note.CollisionCount += collidedNotes.Length;
                        foreach (var n in collidedNotes) {
                            n.CollisionCount++;
                        }
                    }
                }

                void IUndoableOperation.Undo()
                {
                    Debug.Assert(_chartModel._visibleNotes[_modelInsertIndex] == _note);
                    _chartModel._visibleNotes.RemoveAt(_modelInsertIndex);
                    if (RequiresInsertTail) {
                        Debug.Assert(_chartModel._visibleNotes[_holdTailInsertIndex] == _noteTailModel);
                        _chartModel._visibleNotes.RemoveAt(_holdTailInsertIndex);
                        _chartModel._holdCount--;
                    }

                    RevertCollision();
                    _onUndone?.Invoke();

                    void RevertCollision()
                    {
                        Debug.Assert(_collidedNotes is not null);

                        var collidedNotes = _collidedNotes.Value.Span;
                        _note.CollisionCount -= collidedNotes.Length;
                        foreach (var n in collidedNotes) {
                            n.CollisionCount--;
                        }
                    }
                }
            }

            public sealed class AddMultipleNotesOperation : IUndoableOperation
            {
                private readonly int[] _insertIndices;
                private readonly ChartModel _chartModel;
                private readonly ReadOnlyMemory<IStageNoteModel> _notes;

                private readonly int _holdCount;

                private ReadOnlySpanAction<IStageNoteModel>? _onRedone;
                private ReadOnlySpanAction<IStageNoteModel>? _onUndone;

                /// <summary>
                /// Notes that collides to _notes[index], the value is default
                /// when _notes[index] is not <see cref="NoteModel"/>
                /// </summary>
                private readonly ReadOnlyMemory<NoteModel>?[] _collidedNotes;

                public AddMultipleNotesOperation(int[] insertIndices, ChartModel chartModel, ReadOnlyMemory<IStageNoteModel> noteModels)
                {
                    Debug.Assert(insertIndices.Length == noteModels.Length);
                    _insertIndices = insertIndices;
                    _chartModel = chartModel;
                    _notes = noteModels;
                    _collidedNotes = new ReadOnlyMemory<NoteModel>?[_insertIndices.Length];

                    foreach (var note in _notes.Span) {
                        if (note is NoteTailModel)
                            _holdCount++;
                    }
                }

                public AddMultipleNotesOperation WithRedoneAction(ReadOnlySpanAction<IStageNoteModel> action)
                {
                    _onRedone = action;
                    return this;
                }

                public AddMultipleNotesOperation WithUndoneAction(ReadOnlySpanAction<IStageNoteModel> action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    var notes = _notes.Span;
                    for (int i = 0; i < notes.Length; i++) {
                        IStageNoteModel note = notes[i];
                        // We need to maually adjust offset when insert from start
                        _chartModel._visibleNotes.Insert(_insertIndices[i] + i, note);
                        if (note is NoteModel noteModel)
                            CheckCollision(i, noteModel);
                    }
                    _chartModel._holdCount += _holdCount;
                    _onRedone?.Invoke(notes);

                    void CheckCollision(int index, NoteModel noteModel)
                    {
                        Debug.Assert(_notes.Span[index] == noteModel);

                        ref var collidedNotes = ref _collidedNotes[index];
                        collidedNotes ??= _chartModel.Notes.GetCollidedNotesTo(_insertIndices[index] + index);
                        var collidedNotesSpan = collidedNotes.Value.Span;

                        noteModel.CollisionCount += collidedNotesSpan.Length;
                        foreach (var note in collidedNotesSpan) {
                            note.CollisionCount++;
                        }
                    }
                }

                void IUndoableOperation.Undo()
                {
                    var notes = _notes.Span;
                    for (int i = _notes.Length - 1; i >= 0; i--) {
                        Debug.Assert(_chartModel._visibleNotes[_insertIndices[i] + i] == notes[i]);
                        _chartModel._visibleNotes.RemoveAt(_insertIndices[i] + i);
                    }
                    RevertCollision();
                    _chartModel._holdCount -= _holdCount;
                    _onUndone?.Invoke(notes);

                    void RevertCollision()
                    {
                        for (int i = 0; i < _collidedNotes.Length; i++) {
                            Debug.Assert(_collidedNotes[i].HasValue);
                            var collidedNotes = _collidedNotes[i].Value.Span;
                            if (collidedNotes.IsEmpty) {
                                // If empty, means this note doesnt collided to any other note
                                // or this note is a NoteTailModel
                                continue;
                            }

                            ((NoteModel)_notes.Span[i]).CollisionCount -= collidedNotes.Length;
                            foreach (var note in collidedNotes) {
                                note.CollisionCount--;
                            }
                        }
                    }
                }
            }

            public sealed class RemoveNotesOperation : IUndoableOperation
            {
                private readonly ChartModel _chartModel;

                private readonly Memory<int> _removeIndices;
                private readonly IStageNoteModel[] _removeModels;

                private readonly int _holdCount;
                private readonly UnlinkNotesOperation _unlinkOperation;

                private Action? _onRedone;
                private ReadOnlySpanAction<IStageNoteModel>? _onUndone;

                /// <summary>
                /// Notes that collides to _notes[index], the value is default
                /// when _notes[index] is not <see cref="NoteModel"/>
                /// </summary>
                private readonly ReadOnlyMemory<NoteModel>?[] _collidedNotes;

                public RemoveNotesOperation(Memory<int> removeIndices, ChartModel chartModel)
                {
                    _removeIndices = removeIndices;
                    _chartModel = chartModel;
                    _removeModels = new IStageNoteModel[removeIndices.Length];
                    var removeIndicesSpan = removeIndices.Span;
                    for (int i = 0; i < _removeModels.Length; i++) {
                        var model = _chartModel._visibleNotes[removeIndicesSpan[i]];
                        _removeModels[i] = model;
                        if (model is NoteTailModel)
                            _holdCount++;
                    }
                    _collidedNotes = new ReadOnlyMemory<NoteModel>?[_removeIndices.Length];

                    _unlinkOperation = new UnlinkNotesOperation(_removeModels.OfType<NoteModel>().ToArray());
                }

                public RemoveNotesOperation WithRedoneAction(Action action)
                {
                    _onRedone = action;
                    return this;
                }

                public RemoveNotesOperation WithUndoneAction(ReadOnlySpanAction<IStageNoteModel> action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    ((IUndoableOperation)_unlinkOperation).Redo();

                    var removeIndices = _removeIndices.Span;

                    for (int i = _removeIndices.Length - 1; i >= 0; i--) {
                        if (_chartModel._visibleNotes[removeIndices[i]] is NoteModel)
                            UpdateCollision(i);
                        _chartModel._visibleNotes.RemoveAt(removeIndices[i]);
                    }
                    _chartModel._holdCount -= _holdCount;
                    _onRedone?.Invoke();

                    void UpdateCollision(int index)
                    {
                        var removeIndices = _removeIndices.Span;
                        Debug.Assert(_chartModel._visibleNotes[removeIndices[index]] is NoteModel);

                        ref var collidedNotes = ref _collidedNotes[index];
                        collidedNotes ??= _chartModel.Notes.GetCollidedNotesTo(removeIndices[index]);

                        foreach (var note in collidedNotes.Value.Span) {
                            note.CollisionCount--;
                        }
                    }
                }

                void IUndoableOperation.Undo()
                {
                    ((IUndoableOperation)_unlinkOperation).Undo();
                    var removeIndices = _removeIndices.Span;

                    for (int i = 0; i < _removeModels.Length; i++) {
                        var model = _removeModels[i];
                        _chartModel._visibleNotes.Insert(removeIndices[i], model);
                    }
                    RevertCollision();
                    _chartModel._holdCount += _holdCount;
                    _onUndone?.Invoke(_removeModels);

                    void RevertCollision()
                    {
                        foreach (var collidedNotes in _collidedNotes) {
                            if (collidedNotes is null) {
                                // Means the related IStageNoteModel is NoteTailModel
                                continue;
                            }
                            if (collidedNotes.Value.IsEmpty)
                                continue;
                            foreach (var note in collidedNotes.Value.Span) {
                                note.CollisionCount++;
                            }
                        }
                    }
                }
            }

            public sealed class LinkNotesOperation : IUndoableOperation
            {
                private readonly NoteModel[] _notes;
                private readonly (bool IsSlide, NoteData? Prev, NoteData? Next)[] _oldValues;

                private Action? _onRedone;
                private Action? _onUndone;

                public LinkNotesOperation(NoteModel[] notes)
                {
                    _notes = notes;
                    _oldValues = Utils.Array<(bool IsSlide, NoteData? Prev, NoteData? Next)>(_notes.Length);

                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        _oldValues[i] = (data.IsSlide, data.PrevLink, data.NextLink);
                    }
                }

                public LinkNotesOperation WithRedoneAction(Action action)
                {
                    _onRedone = action;
                    return this;
                }

                public LinkNotesOperation WithUndoneAction(Action action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        if (data.IsSlide) {
                            data.UnlinkWithoutCutLinkChain();
                        }
                        data.IsSlide = true;

                        if (i > 0) {
                            var prev = _notes[i - 1].Data;
                            data.PrevLink = prev;
                            prev.NextLink = data;
                        }
                    }
                    _onRedone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        var (isSlide, prev, next) = _oldValues[i];
                        Debug.Assert(data.IsSlide);

                        data.IsSlide = isSlide;
                        if (data.IsSlide) {
                            // value.Prev may be not in context, so manually set .NextLink
                            data.PrevLink = prev;
                            if (prev != null && prev.NextLink != data)
                                prev.NextLink = data;
                            data.NextLink = next;
                            if (next != null)
                                next.PrevLink = data;
                        }
                    }
                    _onUndone?.Invoke();
                }
            }

            public sealed class UnlinkNotesOperation : IUndoableOperation
            {
                private readonly NoteModel[] _notes;
                private readonly (bool IsSlide, NoteData? Prev, NoteData? Next)[] _oldValues;

                private Action? _onRedone;
                private Action? _onUndone;

                public UnlinkNotesOperation(NoteModel[] contexts)
                {
                    _notes = contexts;
                    _oldValues = Utils.Array<(bool IsSlide, NoteData? Prev, NoteData? Next)>(_notes.Length);

                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        _oldValues[i] = (data.IsSlide, data.PrevLink, data.NextLink);
                    }
                }

                public UnlinkNotesOperation WithRedoneAction(Action action)
                {
                    _onRedone = action;
                    return this;
                }

                public UnlinkNotesOperation WithUndoneAction(Action action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        if (data.IsSlide) {
                            data.UnlinkWithoutCutLinkChain();
                        }
                    }
                    _onRedone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        var (isSlide, prev, next) = _oldValues[i];

                        data.IsSlide = isSlide;
                        if (data.PrevLink != prev) {
                            data.PrevLink = prev;
                            if (data.PrevLink != null)
                                data.PrevLink.NextLink = data;
                        }
                        data.NextLink = next;
                        if (data.NextLink != null)
                            data.NextLink.PrevLink = data;
                    }
                    _onUndone?.Invoke();
                }
            }

            public abstract class EditNotesPropertyOperationBase<T> : IUndoableOperation
            {
                protected readonly ChartModel _chartModel;

                protected readonly (NoteModel Note, T OldValue)[] _contexts;
                protected readonly T _newValue;
                protected readonly Func<T, T>? _newValueSelector;

                private bool _firstRedo;
                private Action _onDone;

                protected EditNotesPropertyOperationBase(ChartModel chartModel,
                    (NoteModel Note, T OldValue)[] contexts, T? newValue, Func<T, T>? newValueSelector)
                {
                    _chartModel = chartModel;
                    _contexts = contexts;
                    _newValue = newValue!;
                    _newValueSelector = newValueSelector!;
                    _firstRedo = true;
                }

                public EditNotesPropertyOperationBase<T> WithDoneAction(Action onDone)
                {
                    _onDone = onDone;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    for (int i = 0; i < _contexts.Length; i++) {
                        var (note, oldValue) = _contexts[i];
                        var newValue = GetNewValue(oldValue);
                        SetValue(note, newValue);
                        OnRedoingValueSetted(_firstRedo, i, newValue);
                    }
                    OnRedone(_firstRedo);

                    _onDone?.Invoke();
                    _firstRedo = false;
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = _contexts.Length - 1; i >= 0; i--) {
                        var (note, oldValue) = _contexts[i];
                        SetValue(note, oldValue);
                        OnUndoingValueSetted(i);
                    }
                    OnUndone();

                    _onDone?.Invoke();
                }

                private T GetNewValue(T oldValue)
                    => _newValueSelector is null ? _newValue : _newValueSelector(oldValue);

                protected abstract void SetValue(NoteModel note, T value);

                protected virtual void OnRedoingValueSetted(bool isFirstRedo, int contextIndex, T newValue) { }
                protected virtual void OnUndoingValueSetted(int contextIndex) { }

                protected virtual void OnRedone(bool isFirstRedo) { }
                protected virtual void OnUndone() { }
            }

            /*
            public sealed class EditNotesPropertyOperation<T> : IUndoableOperation
            {
                private readonly (NoteModel Note, T OldValue)[] _contexts;
                private readonly T _newValue;
                private readonly Func<T, T> _newValueSelector;
                private readonly Action<NoteData, T> _valueSetter;
                private readonly ChartModel _chartModel;

                private Action? _onDone;

                private SpecialProperties _specialProperties;
                private bool RequiresSort => _specialProperties.HasFlag(SpecialProperties.Time);
                private bool RequiresUpdateCollision => (_specialProperties & (SpecialProperties.Time | SpecialProperties.Position)) != SpecialProperties.None;
                private bool RequiresEditHoldTail => _specialProperties.HasFlag(SpecialProperties.Duration);

                //private bool _isRequireSortNotes;
                //private bool _isRequireUpdateCollision;
                //private int[]? _beforeNoteIndices;
                //private int[]? _afterNoteIndices;
                //private ListReadOnlyView<NoteModel>[]? _beforeCollidedNotes;
                //private ListReadOnlyView<NoteModel>[]? _afterCollidedNotes;
                //private (bool? IsInsertBefore, NoteData BeforeRefNote, NoteData AfterRefNote)[] _linkChangeDatas;

                private int[] _noteIndicesBeforeSort;
                private int[] _noteIndicesAfterSort;
                private (bool? InsertBefore, NoteData NoteRefBeforeSort, NoteData NoteRefAfterSort)[] _linkChangeInfosOnSorting;

                private ReadOnlyMemory<NoteModel>[] _collidedNotesBeforeUpdate = default!; // Lazy init in EnsureExtraHandlingInitialized
                private ReadOnlyMemory<NoteModel>[] _collidedNotesAfterUpdate = default!;  // Lazy init in EnsureExtraHandlingInitialized

                public EditNotesPropertyOperation((NoteModel, T OldValue)[] contexts, T newValue,
                    Action<NoteData, T> edit, ChartModel chartModel)
                {
                    _contexts = contexts;
                    _newValue = newValue;
                    _valueSetter = edit;
                    _chartModel = chartModel;
                    Debug.Assert(_contexts.Length > 0);
                    NoteTimeComparer.AssertInOrder(_contexts.Select(v => v.Note));
                }

                public EditNotesPropertyOperation((NoteModel, T OldValue)[] contexts, Func<T, T> newValueSelector,
                    Action<NoteData, T> valueSetter, ChartModel chartModel)
                {
                    _contexts = contexts;
                    _newValueSelector = newValueSelector;
                    _valueSetter = valueSetter;
                    _chartModel = chartModel;
                    Debug.Assert(_contexts.Length > 0);
                    NoteTimeComparer.AssertInOrder(_contexts.Select(v => v.Note));
                }


                public EditNotesPropertyOperation<T> WithExtraHandlings(bool editTime = false, bool editPosition = false, bool editDuration = false)
                {
                    if (editTime)
                        _specialProperties |= SpecialProperties.Time;
                    if (editPosition)
                        _specialProperties |= SpecialProperties.Position;
                    if (editDuration)
                        _specialProperties |= SpecialProperties.Duration;
                    return this;
                }

                public EditNotesPropertyOperation<T> WithDoneAction(Action action)
                {
                    _onDone = action;
                    return this;
                }

                // - Initialize datas
                //   - Index in chart
                //   - collision datas
                // - Edit
                // - Sort notes & relink
                // - check collisions
                void IUndoableOperation.Redo()
                {
                    //EnsureBeforeAdjustInited(out var requiresLaterInit);
                    EnsureExtraHandlingInitialized();

                    for (int i = 0; i < _contexts.Length; i++) {
                        var (note, oldValue) = _contexts[i];
                        _valueSetter(note.Data,
                            _newValueSelector is null ? _newValue : _newValueSelector.Invoke(oldValue));
                        InitializeSortingContextsAfterValueSetting(i);
                        //if (requiresLaterInit)
                        //    InitAfterSortNotesDatas(i);
                    }
                    OnRedoneSortNotes();

                    InitializeCollisionUpdatingContext();
                    //if (requiresLaterInit)
                    //    InitAfterCollisionDatas();
                    OnRedoneUpdateCollisions();

                    _onDone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    foreach (var (note, oldVal) in _contexts) {
                        _valueSetter(note.Data, oldVal);
                    }
                    OnUndone();

                    _onDone?.Invoke();
                }


                private void EnsureExtraHandlingInitialized()
                {
                    // Init note indices before sort
                    switch (RequiresSort, RequiresUpdateCollision) {
                        case (true, true): {
                            InitNoteIndices(fillCurrentInidicesIntoAfter: false);
                            _linkChangeInfosOnSorting ??= new (bool?, NoteData, NoteData)[_contexts.Length];
                            InitCollidedNotesContext();
                            break;
                        }
                        case (true, false): {
                            InitNoteIndices(fillCurrentInidicesIntoAfter: false);
                            _linkChangeInfosOnSorting ??= new (bool?, NoteData, NoteData)[_contexts.Length];
                            break;
                        }
                        case (false, true): {
                            InitNoteIndices(true);
                            InitCollidedNotesContext();
                            break;
                        }
                        case (false, false):
                            break;
                    }

                    void InitNoteIndices(bool fillCurrentInidicesIntoAfter)
                    {
                        if (_noteIndicesBeforeSort is not null)
                            return;
                        _noteIndicesBeforeSort = new int[_contexts.Length];
                        InitCurrentIndices(_noteIndicesBeforeSort);

                        if (fillCurrentInidicesIntoAfter)
                            _noteIndicesAfterSort = _noteIndicesBeforeSort;
                        else
                            _noteIndicesAfterSort ??= new int[_contexts.Length];

                        void InitCurrentIndices(int[] indices)
                        {
                            int iContext = 0;
                            NoteModel currentNote = _contexts[iContext].Note;

                            int iModel = _chartModel.Notes.Search(currentNote);
                            Debug.Assert(iModel >= 0);

                            for (; iModel < _chartModel.Notes.Count; iModel++) {
                                var note = _chartModel.Notes[iModel];
                                if (!ReferenceEquals(note, currentNote))
                                    continue;

                                indices[iContext] = iModel;
                                iContext++;
                                if (iContext >= _contexts.Length)
                                    break;
                                currentNote = _contexts[iContext].Note;
                            }
                        }
                    }

                    void InitCollidedNotesContext()
                    {
                        if (_collidedNotesBeforeUpdate is null) {
                            _collidedNotesBeforeUpdate = new ReadOnlyMemory<NoteModel>[_contexts.Length];
                            for (int i = 0; i < _contexts.Length; i++) {
                                _collidedNotesBeforeUpdate[i] = _chartModel.Notes.GetCollidedNotesTo(_noteIndicesBeforeSort[i]);
                            }
                        }
                        _collidedNotesAfterUpdate ??= new ReadOnlyMemory<NoteModel>[_contexts.Length];
                    }
                }

                private void InitializeSortingContextsAfterValueSetting(int contextIndex)
                {
                    if (!RequiresSort)
                        return;

                    Debug.Assert(_noteIndicesBeforeSort is not null);
                    Debug.Assert(_noteIndicesAfterSort is not null);
                    Debug.Assert(_linkChangeInfosOnSorting is not null);

                    var fromIndex = _noteIndicesBeforeSort[contextIndex];
                    var toIndex = GetExpectedIndexAfterSort(fromIndex);
                    _noteIndicesAfterSort[contextIndex] = toIndex;

                    var note = _contexts[contextIndex].Note;
                    if (note.Data.IsSlide) {
                        _linkChangeInfosOnSorting[contextIndex] = GetLinkChangInfos(note);
                    }

                    int GetExpectedIndexAfterSort(int fromIndex)
                    {
                        var note = _chartModel.Notes[fromIndex];

                        int newIndex = fromIndex - 1;

                        // Search backward
                        for (; newIndex >= 0; newIndex--) {
                            var prevNote = _chartModel.Notes[newIndex];
                            if (prevNote.Time <= note.Time)
                                break;
                        }
                        newIndex++;
                        if (newIndex != fromIndex)
                            // The note is moved backward
                            return newIndex;

                        newIndex = fromIndex + 1;
                        for (; newIndex < _chartModel.Notes.Count; newIndex++) {
                            var nextNote = _chartModel.Notes[newIndex];
                            if (nextNote.Time >= note.Time)
                                break;
                        }
                        newIndex--;
                        if (newIndex != fromIndex)
                            // The note is moved forward
                            return newIndex;

                        // The note doesn't need move
                        return fromIndex;
                    }

                    static (bool? InsertBefore, NoteData NoteRefBeforeSort, NoteData NoteRefAfterSort) GetLinkChangInfos(NoteModel note)
                    {
                        if (note.Data.PrevLink is not null) {
                            // Find the first note in link that has time greater than current note,
                            // And move current note before target note
                            NoteData target = note.Data;
                            for (; target.PrevLink != null; target = target.PrevLink) {
                                if (target.PrevLink.Time <= note.Data.Time)
                                    break;
                            }
                            if (target != note.Data) {
                                return (true, note.Data.NextLink, target);
                            }
                        }

                        if (note.Data.NextLink is not null) {
                            NoteData target = note.Data;
                            for (; target.NextLink != null; target = target.NextLink) {
                                if (target.NextLink.Time >= note.Data.Time)
                                    break;
                            }
                            if (target != note.Data)
                                return (false, note.Data.PrevLink, target);
                        }

                        return (null, null!, null!);
                    }
                }

                private void OnRedoneSortNotes()
                {
                    if (!RequiresSort)
                        return;

                    Debug.Assert(_noteIndicesBeforeSort is not null);
                    Debug.Assert(_noteIndicesAfterSort is not null);
                    Debug.Assert(_linkChangeInfosOnSorting is not null);

                    for (int i = 0; i < _contexts.Length; i++) {
                        // Sort time
                        _chartModel._visibleNotes.MoveTo(_noteIndicesBeforeSort[i], _noteIndicesAfterSort[i]);

                        // Update link order
                        var curNote = _contexts[i].Note.Data;
                        var (insertBefore, _, target) = _linkChangeInfosOnSorting[i];
                        switch (insertBefore) {
                            case true:
                                curNote.InsertAsLinkBefore(target);
                                break;
                            case false:
                                curNote.InsertAsLinkAfter(target);
                                break;
                            case null:
                                break;
                        }
                    }
                }

                private void InitializeCollisionUpdatingContext()
                {
                    if (!RequiresUpdateCollision)
                        return;

                    Debug.Assert(_noteIndicesAfterSort is not null);
                    Debug.Assert(_collidedNotesAfterUpdate is not null);

                    for (int i = 0; i < _contexts.Length; i++) {
                        _collidedNotesAfterUpdate[i] = _chartModel.Notes.GetCollidedNotesTo(_noteIndicesAfterSort[i]);
                    }
                }

                private void OnRedoneUpdateCollisions()
                {
                    if (!RequiresUpdateCollision)
                        return;

                    Debug.Assert(_collidedNotesBeforeUpdate is not null);
                    Debug.Assert(_collidedNotesAfterUpdate is not null);

                    for (int i = 0; i < _contexts.Length; i++) {
                        var curNote = _contexts[i].Note;
                        curNote.CollisionCount += _collidedNotesAfterUpdate[i].Length - _collidedNotesBeforeUpdate[i].Length;
                        foreach (var note in _collidedNotesBeforeUpdate[i].Span)
                            note.CollisionCount--;
                        foreach (var note in _collidedNotesAfterUpdate[i].Span)
                            note.CollisionCount++;
                    }
                }

                private void OnUndone()
                {
                    if (RequiresSort) {
                        for (int i = _contexts.Length - 1; i >= 0; i--) {
                            _chartModel._visibleNotes.MoveTo(_noteIndicesAfterSort[i], _noteIndicesBeforeSort[i]);

                            var curNote = _contexts[i].Note.Data;
                            var (insertBefore, target, _) = _linkChangeInfosOnSorting[i];
                            switch (insertBefore) {
                                case true: curNote.InsertAsLinkBefore(target); break;
                                case false: curNote.InsertAsLinkAfter(target); break;
                                case null: break;
                            }
                        }
                    }

                    if (RequiresUpdateCollision) {
                        for (int i = _contexts.Length - 1; i >= 0; i--) {
                            var curNote = _contexts[i].Note;
                            curNote.CollisionCount += _collidedNotesBeforeUpdate[i].Length - _collidedNotesAfterUpdate[i].Length;
                            foreach (var note in _collidedNotesAfterUpdate[i].Span)
                                note.CollisionCount--;
                            foreach (var note in _collidedNotesBeforeUpdate[i].Span)
                                note.CollisionCount++;
                        }
                    }
                }
                /*
                private void EnsureBeforeAdjustInited(out bool requiresLaterInit)
                {
                    requiresLaterInit = false;

                    if (_beforeNoteIndices is null && (_isRequireUpdateCollision || _isRequireSortNotes)) {
                        requiresLaterInit = true;

                        _beforeNoteIndices = new int[_contexts.Length];
                        int iContext = 0;
                        NoteModel currentNote = _contexts[iContext].Note;

                        int iModel = _chartModel.Notes.Search(currentNote);
                        Debug.Assert(iModel >= 0);
                        for (; iModel < _chartModel.Notes.Count; iModel++) {
                            var note = _chartModel.Notes[iModel];
                            if (!ReferenceEquals(note, currentNote))
                                continue;

                            _beforeNoteIndices[iContext] = iModel;
                            iContext++;
                            if (iContext >= _contexts.Length)
                                break;
                            currentNote = _contexts[iContext].Note;
                        }
                    }

                    if (_afterNoteIndices is null) {
                        if (_isRequireSortNotes)
                            _afterNoteIndices = new int[_contexts.Length];
                        else if (_isRequireUpdateCollision)
                            _afterNoteIndices = _beforeNoteIndices;
                    }

                    if (_beforeCollidedNotes is null && _isRequireUpdateCollision) {
                        _beforeCollidedNotes = new ListReadOnlyView<NoteModel>[_contexts.Length];
                        for (int i = 0; i < _contexts.Length; i++) {
                            _beforeCollidedNotes[i] = _chartModel.Notes.GetCollidedNotesTo(_beforeNoteIndices[i]);
                        }

                        _afterCollidedNotes = new ListReadOnlyView<NoteModel>[_contexts.Length];
                    }

                    if (_linkChangeDatas is null && _isRequireSortNotes) {
                        _linkChangeDatas =
                            new (bool? IsInsertBefore, NoteData BeforeRefNote, NoteData AfterRefNote)[_contexts.Length];
                    }
                }

                private void InitAfterSortNotesDatas(int contextIndex)
                {
                    if (_isRequireSortNotes) {
                        var fromIndex = _beforeNoteIndices[contextIndex];
                        var toIndex = GetIndexAfterSort(fromIndex);
                        _afterNoteIndices[contextIndex] = toIndex;

                        var note = _contexts[contextIndex].Note;
                        if (note.Data.IsSlide) {
                            _linkChangeDatas[contextIndex] = GetLinkChangeDatas(note);
                        }
                    }

                    int GetIndexAfterSort(int index)
                    {
                        var note = _chartModel.Notes[index];

                        int newIndex = index - 1;
                        for (; newIndex >= 0; newIndex--) {
                            var prevNote = _chartModel._visibleNotes[newIndex];
                            if (prevNote.Data.Time <= note.Data.Time) {
                                break;
                            }
                        }
                        newIndex++;
                        if (newIndex != index) {
                            return newIndex;
                        }

                        newIndex = index + 1;
                        for (; newIndex < _chartModel._visibleNotes.Count; newIndex++) {
                            var nextNote = _chartModel._visibleNotes[newIndex];
                            if (nextNote.Data.Time >= note.Data.Time) {
                                break;
                            }
                        }
                        newIndex--;
                        if (newIndex != index) {
                            return newIndex;
                        }

                        return index;
                    }

                    (bool? InsertBefore, NoteData Before, NoteData After) GetLinkChangeDatas(NoteModel note)
                    {
                        if (note.Data.PrevLink is not null) {
                            NoteData next = note.Data;
                            for (; next.PrevLink != null; next = next.PrevLink) {
                                if (next.PrevLink.Time <= note.Data.Time) {
                                    break;
                                }
                            }
                            if (next != note.Data) {
                                return (true, note.Data.NextLink, next);
                            }
                        }

                        if (note.Data.NextLink is not null) {
                            NoteData prev = note.Data;
                            for (; prev.NextLink != null; prev = prev.NextLink) {
                                if (prev.NextLink.Time >= note.Data.Time) {
                                    break;
                                }
                            }
                            if (prev != note.Data) {
                                return (false, note.Data.PrevLink, prev);
                            }
                        }

                        return (null, null, null);
                    }
                }

                private void InitAfterCollisionDatas()
                {
                    if (_isRequireUpdateCollision) {
                        for (int i = 0; i < _contexts.Length; i++) {
                            _afterCollidedNotes[i] = _chartModel.Notes.GetCollidedNotesTo(_afterNoteIndices[i]);
                        }
                    }
                }

                private void OnRedoneSortNotes_()
                {
                    if (!_isRequireSortNotes)
                        return;

                    for (int i = 0; i < _contexts.Length; i++) {
                        // Sort time order
                        _chartModel._visibleNotes.MoveTo(_beforeNoteIndices[i], _afterNoteIndices[i]);

                        // Update link order
                        var curNote = _contexts[i].Note;
                        var (isBefore, _, linkTarget) = _linkChangeDatas[i];
                        switch (isBefore) {
                            case true:
                                curNote.Data.UnlinkWithoutCutLinkChain();
                                curNote.Data.InsertAsLinkBefore(linkTarget);
                                break;
                            case false:
                                curNote.Data.UnlinkWithoutCutLinkChain();
                                curNote.Data.InsertAsLinkAfter(linkTarget);
                                break;
                            case null:
                                break;
                        }
                    }
                }

                private void OnRedoneUpdateCollisions_()
                {
                    if (!_isRequireUpdateCollision)
                        return;

                    for (int i = 0; i < _contexts.Length; i++) {
                        // Update collision
                        var curNote = _contexts[i].Note;
                        curNote.CollisionCount += _afterCollidedNotes[i].Count - _beforeCollidedNotes[i].Count;
                        foreach (var note in _beforeCollidedNotes[i]) {
                            note.CollisionCount--;
                        }
                        foreach (var note in _afterCollidedNotes[i]) {
                            note.CollisionCount++;
                        }
                    }
                }

                private void OnUndone_()
                {
                    if (_isRequireSortNotes) {
                        for (int i = _contexts.Length - 1; i >= 0; i--) {
                            _chartModel._visibleNotes.MoveTo(_afterNoteIndices[i], _beforeNoteIndices[i]);

                            var curNote = _contexts[i].Note;
                            var (isBefore, linkTarget, _) = _linkChangeDatas[i];
                            switch (isBefore) {
                                case null:
                                    break;
                                case true:
                                    curNote.Data.UnlinkWithoutCutLinkChain();
                                    curNote.Data.InsertAsLinkBefore(linkTarget);
                                    break;
                                case false:
                                    curNote.Data.UnlinkWithoutCutLinkChain();
                                    curNote.Data.InsertAsLinkAfter(linkTarget);
                                    break;
                            }
                        }
                    }

                    if (_isRequireUpdateCollision) {
                        for (int i = _contexts.Length - 1; i >= 0; i--) {
                            var curNote = _contexts[i].Note;
                            curNote.CollisionCount += _beforeCollidedNotes[i].Count - _afterCollidedNotes[i].Count;
                            foreach (var note in _afterCollidedNotes[i]) {
                                note.CollisionCount--;
                            }
                            foreach (var note in _beforeCollidedNotes[i]) {
                                note.CollisionCount++;
                            }
                        }
                    }
                }
                
                [Flags]
                public enum SpecialProperties
                {
                    None = 0,
                    Time = 1,
                    Position = 1 << 1,
                    Duration = 1 << 2,
                }
            }
            */

            public sealed class EditNotesPropertyOperation<T> : EditNotesPropertyOperationBase<T>
            {
                private readonly Action<NoteData, T> _valueSetter;

                public EditNotesPropertyOperation(ChartModel chartModel,
                    (NoteModel Note, T OldValue)[] contexts, T newValue, Action<NoteData, T> valueSetter)
                    : base(chartModel, contexts, newValue, null)
                { _valueSetter = valueSetter; }

                public EditNotesPropertyOperation(ChartModel chartModel,
                    (NoteModel Note, T OldValue)[] contexts, Func<T, T> newValueSelector, Action<NoteData, T> valueSetter)
                    : base(chartModel, contexts, default, newValueSelector)
                { _valueSetter = valueSetter; }

                protected override void SetValue(NoteModel note, T value) => _valueSetter(note.Data, value);
            }

            public sealed class EditNotesCoordPropertyOperation : EditNotesPropertyOperationBase<NoteCoord>
            {
                public EditNotesCoordPropertyOperation(ChartModel chartModel,
                    (NoteModel Note, NoteCoord OldValue)[] contexts, NoteCoord newValue,
                    bool timeEditing)
                    : base(chartModel, contexts, newValue, null)
                {
                    _timeEditing = timeEditing;
                    Initialize();
                }

                public EditNotesCoordPropertyOperation(ChartModel chartModel,
                    (NoteModel Note, NoteCoord OldValue)[] contexts, Func<NoteCoord, NoteCoord> newValueSelector,
                    bool timeEditing)
                    : base(chartModel, contexts, default, newValueSelector)
                {
                    _timeEditing = timeEditing;
                    Initialize();
                }

                private readonly bool _timeEditing;

                private int[] _noteIndicesBeforeSort;
                private int[] _noteIndicesAfterSort;
                private (bool? InsertBefore, NoteData NoteRefBeforeSort, NoteData NoteRefAfterSort)[] _linkChangeInfosOnSorting;

                private ReadOnlyMemory<NoteModel>[] _collidedNotesBeforeUpdate;
                private ReadOnlyMemory<NoteModel>[] _collidedNotesAfterUpdate;

                private void Initialize()
                {
                    _noteIndicesBeforeSort = new int[_contexts.Length];
                    InitCurrentIndices(_noteIndicesBeforeSort);

                    _noteIndicesAfterSort = _timeEditing
                        ? new int[_contexts.Length]
                        : _noteIndicesBeforeSort;

                    if (_timeEditing)
                        _linkChangeInfosOnSorting = new (bool?, NoteData, NoteData)[_contexts.Length];

                    _collidedNotesBeforeUpdate = new ReadOnlyMemory<NoteModel>[_contexts.Length];
                    for (int i = 0; i < _contexts.Length; i++)
                        _collidedNotesBeforeUpdate[i] = _chartModel.Notes.GetCollidedNotesTo(_noteIndicesBeforeSort[i]);

                    _collidedNotesAfterUpdate = new ReadOnlyMemory<NoteModel>[_contexts.Length];

                    void InitCurrentIndices(int[] indices)
                    {
                        int iContext = 0;
                        NoteModel currentNote = _contexts[iContext].Note;

                        int iModel = _chartModel.Notes.Search(currentNote);
                        Debug.Assert(iModel >= 0);

                        for (; iModel < _chartModel.Notes.Count; iModel++) {
                            var note = _chartModel.Notes[iModel];
                            if (note != currentNote)
                                continue;

                            indices[iContext] = iModel;
                            iContext++;
                            if (iContext >= _contexts.Length)
                                break;
                            currentNote = _contexts[iContext].Note;
                        }
                    }
                }

                protected override void SetValue(NoteModel note, NoteCoord value)
                    => note.Data.PositionCoord = new NoteCoord(
                        MainSystem.Args.ClampNoteTime(value.Time),
                        MainSystem.Args.ClampNotePosition(value.Position));

                protected override void OnRedoingValueSetted(bool isFirstRedo, int contextIndex, NoteCoord newValue)
                {
                    if (!isFirstRedo)
                        return;
                    if (!_timeEditing)
                        return;

                    Debug.Assert(_noteIndicesBeforeSort is not null);
                    Debug.Assert(_noteIndicesAfterSort is not null);
                    Debug.Assert(_linkChangeInfosOnSorting is not null);

                    var fromIndex = _noteIndicesBeforeSort[contextIndex];
                    var toIndex = GetExpectedIndexAfterSort(fromIndex);
                    _noteIndicesAfterSort[contextIndex] = toIndex;

                    var note = _contexts[contextIndex].Note;
                    if (note.Data.IsSlide)
                        _linkChangeInfosOnSorting[contextIndex] = GetLinkChangInfos(note);

                    int GetExpectedIndexAfterSort(int fromIndex)
                    {
                        var note = _chartModel.Notes[fromIndex];

                        int newIndex = fromIndex - 1;

                        // Search backward
                        for (; newIndex >= 0; newIndex--) {
                            var prevNote = _chartModel.Notes[newIndex];
                            if (prevNote.Time <= note.Time)
                                break;
                        }
                        newIndex++;
                        if (newIndex != fromIndex)
                            // The note is moved backward
                            return newIndex;

                        newIndex = fromIndex + 1;
                        for (; newIndex < _chartModel.Notes.Count; newIndex++) {
                            var nextNote = _chartModel.Notes[newIndex];
                            if (nextNote.Time >= note.Time)
                                break;
                        }
                        newIndex--;
                        if (newIndex != fromIndex)
                            // The note is moved forward
                            return newIndex;

                        // The note doesn't need move
                        return fromIndex;
                    }

                    static (bool? InsertBefore, NoteData NoteRefBeforeSort, NoteData NoteRefAfterSort) GetLinkChangInfos(NoteModel note)
                    {
                        if (note.Data.PrevLink is not null) {
                            // Find the first note in link that has time greater than current note,
                            // And move current note before target note
                            NoteData target = note.Data;
                            for (; target.PrevLink != null; target = target.PrevLink) {
                                if (target.PrevLink.Time <= note.Data.Time)
                                    break;
                            }
                            if (target != note.Data) {
                                return (true, note.Data.NextLink, target);
                            }
                        }

                        if (note.Data.NextLink is not null) {
                            NoteData target = note.Data;
                            for (; target.NextLink != null; target = target.NextLink) {
                                if (target.NextLink.Time >= note.Data.Time)
                                    break;
                            }
                            if (target != note.Data)
                                return (false, note.Data.PrevLink, target);
                        }

                        return (null, null!, null!);
                    }
                }

                protected override void OnRedone(bool isFirstRedo)
                {
                    if (_timeEditing)
                        SortNotes();

                    if (isFirstRedo)
                        InitializeCollisionUpdatingContext();

                    UpdateCollisions();

                    return;

                    void SortNotes()
                    {
                        Debug.Assert(_noteIndicesBeforeSort is not null);
                        Debug.Assert(_noteIndicesAfterSort is not null);
                        Debug.Assert(_linkChangeInfosOnSorting is not null);

                        for (int i = 0; i < _contexts.Length; i++) {
                            // Sort time
                            _chartModel._visibleNotes.MoveTo(_noteIndicesBeforeSort[i], _noteIndicesAfterSort[i]);

                            // Update link order
                            var curNote = _contexts[i].Note.Data;
                            var (insertBefore, _, target) = _linkChangeInfosOnSorting[i];
                            switch (insertBefore) {
                                case true: curNote.InsertAsLinkBefore(target); break;
                                case false: curNote.InsertAsLinkAfter(target); break;
                                case null: break;
                            }
                        }
                    }

                    void InitializeCollisionUpdatingContext()
                    {
                        Debug.Assert(_noteIndicesAfterSort is not null);
                        Debug.Assert(_collidedNotesAfterUpdate is not null);

                        for (int i = 0; i < _contexts.Length; i++) {
                            _collidedNotesAfterUpdate[i] = _chartModel.Notes.GetCollidedNotesTo(_noteIndicesAfterSort[i]);
                        }
                    }

                    void UpdateCollisions()
                    {
                        Debug.Assert(_collidedNotesBeforeUpdate is not null);
                        Debug.Assert(_collidedNotesAfterUpdate is not null);

                        for (int i = 0; i < _contexts.Length; i++) {
                            var curNote = _contexts[i].Note;
                            curNote.CollisionCount += _collidedNotesAfterUpdate[i].Length - _collidedNotesBeforeUpdate[i].Length;
                            foreach (var note in _collidedNotesBeforeUpdate[i].Span)
                                note.CollisionCount--;
                            foreach (var note in _collidedNotesAfterUpdate[i].Span)
                                note.CollisionCount++;
                        }
                    }
                }

                protected override void OnUndone()
                {
                    if (_timeEditing)
                        RevertSort();

                    RevertCollision();

                    return;

                    void RevertSort()
                    {
                        for (int i = _contexts.Length - 1; i >= 0; i--) {
                            _chartModel._visibleNotes.MoveTo(_noteIndicesAfterSort[i], _noteIndicesBeforeSort[i]);

                            var curNote = _contexts[i].Note.Data;
                            var (insertBefore, target, _) = _linkChangeInfosOnSorting[i];
                            switch (insertBefore) {
                                case true: curNote.InsertAsLinkBefore(target); break;
                                case false: curNote.InsertAsLinkAfter(target); break;
                                case null: break;
                            }
                        }
                    }

                    void RevertCollision()
                    {
                        for (int i = _contexts.Length - 1; i >= 0; i--) {
                            var curNote = _contexts[i].Note;
                            curNote.CollisionCount += _collidedNotesBeforeUpdate[i].Length - _collidedNotesAfterUpdate[i].Length;
                            foreach (var note in _collidedNotesAfterUpdate[i].Span)
                                note.CollisionCount--;
                            foreach (var note in _collidedNotesBeforeUpdate[i].Span)
                                note.CollisionCount++;
                        }
                    }
                }
            }

            public sealed class EditNotesDurationPropertyOperation : EditNotesPropertyOperationBase<float>
            {
                private int _holdCountDelta;

                public EditNotesDurationPropertyOperation(ChartModel chartModel,
                    (NoteModel Note, float OldValue)[] contexts, float newValue)
                    : base(chartModel, contexts, newValue, null)
                {
                    _tails = new (NoteTailModel Tail, int FromIndex, int ToIndex)[contexts.Length];
                }

                // index -1, means the note is not hold.
                // if both index are -1, means the value not changed.
                private readonly (NoteTailModel Tail, int FromIndex, int ToIndex)[] _tails;

                protected override void SetValue(NoteModel note, float value)
                {
                    note.Data.Duration = value;
                }

                protected override void OnRedoingValueSetted(bool isFirstRedo, int contextIndex, float newValue)
                {
                    var (note, oldValue) = _contexts[contextIndex];

                    if (isFirstRedo)
                        InitializeTailsArray();

                    var (tail, from, to) = _tails[contextIndex];
                    switch (from, to) {
                        case (-1, > 0): _chartModel._visibleNotes.Insert(to, tail); break;
                        case ( > 0, -1): _chartModel._visibleNotes.RemoveAt(from); break;
                        case (-1, -1): break;
                        default: _chartModel._visibleNotes.MoveTo(from, to); break;
                    }

                    void InitializeTailsArray()
                    {
                        switch (oldValue, newValue) {
                            case (0, not 0): {
                                var tail = new NoteTailModel(note);
                                int insertIndex = _chartModel._visibleNotes.AsReadOnlySpan()
                                    .FindLowerBoundIndex(tail, NoteTimeComparer.Instance);
                                _tails[contextIndex] = (tail, -1, insertIndex);
                                _holdCountDelta++;
                                break;
                            }
                            case (not 0, 0): {
                                int tailIndex = _chartModel.Notes.SearchTailOf(note);
                                _tails[contextIndex] = ((NoteTailModel)_chartModel._visibleNotes[tailIndex], tailIndex, -1);
                                _holdCountDelta--;
                                break;
                            }
                            default: {
                                if (oldValue == newValue) {
                                    _tails[contextIndex] = (null, -1, -1);
                                    break;
                                }

                                int tailIndex = _chartModel.Notes.SearchTailOf(note);
                                var tail = (NoteTailModel)_chartModel._visibleNotes[tailIndex];
                                int toIndex;
                                if (oldValue < newValue) {
                                    toIndex = tailIndex + _chartModel._visibleNotes
                                        .AsReadOnlySpan()[tailIndex..]
                                        .FindLowerBoundIndex(tail, NoteTimeComparer.Instance);
                                }
                                else {
                                    toIndex = _chartModel._visibleNotes
                                        .AsReadOnlySpan()[..tailIndex]
                                        .FindUpperBoundIndex(tail, NoteTimeComparer.Instance);
                                }
                                _tails[contextIndex] = (tail, tailIndex, toIndex);
                                break;
                            }
                        }
                    }
                }

                protected override void OnUndoingValueSetted(int contextIndex)
                {
                    var (tailModel, from, to) = _tails[contextIndex];
                    switch (from, to) {
                        case (-1, > 0): _chartModel._visibleNotes.RemoveAt(to); break;
                        case ( > 0, -1): _chartModel._visibleNotes.Insert(from, tailModel); break;
                        case (-1, -1): break;
                        default: _chartModel._visibleNotes.MoveTo(to, from); break;
                    }
                }

                protected override void OnRedone(bool isFirstRedo)
                {
                    _chartModel._holdCount += _holdCountDelta;
                }

                protected override void OnUndone()
                {
                    _chartModel._holdCount -= _holdCountDelta;
                }
            }

            public sealed class EditNotesSoundsOperation : EditNotesPropertyOperationBase<PianoSoundValueData[]>
            {
                public EditNotesSoundsOperation(ChartModel chartModel, (NoteModel Note, PianoSoundValueData[] OldValues)[] contexts,
                    ReadOnlySpan<PianoSoundValueData> newValue)
                    : base(chartModel, contexts, newValue.ToArray(), null)
                { }

                protected override void SetValue(NoteModel note, PianoSoundValueData[] value)
                {
                    var sounds = note.Data.Sounds;
                    if (value.Length < sounds.Count) {
                        int i = 0;
                        for (; i < value.Length; i++) {
                            sounds[i].SetValues(value[i]);
                        }
                        sounds.RemoveRange(value.Length..);
                    }
                    else {
                        int i = 0;
                        for (; i < sounds.Count; i++) {
                            sounds[i].SetValues(value[i]);
                        }
                        for (; i < value.Length; i++) {
                            // Optimizable: GC?
                            sounds.Add(new PianoSoundData(value[i]));
                        }
                    }
                }
            }

            #endregion
        }
    }
}