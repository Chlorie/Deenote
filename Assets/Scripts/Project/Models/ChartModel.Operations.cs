using Deenote.Edit.Operations;
using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
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
                int iModel;
                for (iModel = noteModels.Count - 1; iModel >= 0; iModel--) {
                    if (noteModels[iModel].Data.Time <= coord.Time) {
                        break;
                    }
                }
                iModel++;

                NoteModel model = new(data);
                return new AddNoteOperation(iModel, _chartModel, model);
            }

            public AddMultipleNotesOperation AddMultipleNotes(NoteCoord baseCoord,
                ListReadOnlyView<NoteData> notePrototypes)
            {
                NoteTimeComparer.AssertInOrder(notePrototypes);

                var insertIndices = new int[notePrototypes.Count];

                var noteModels = _chartModel._visibleNotes;
                int iModel = _chartModel.Notes.Count - 1;
                for (int i = notePrototypes.Count - 1; i >= 0; i--) {
                    var note = notePrototypes[i];
                    var realTime = note.Time + baseCoord.Time;

                    while (iModel >= 0 && noteModels[iModel].Data.Time > realTime)
                        iModel--;
                    insertIndices[i] = iModel + 1;
                }

                NoteModel[] insertModels = new NoteModel[notePrototypes.Count];
                for (int i = 0; i < notePrototypes.Count; i++) {
                    var insertNote = notePrototypes[i].Clone();
                    var coord = NoteCoord.ClampPosition(new(insertNote.Time + baseCoord.Time,
                        insertNote.Position + baseCoord.Position));
                    insertNote.PositionCoord = coord;
                    insertModels[i] = new(insertNote);
                }
                return new AddMultipleNotesOperation(insertIndices, _chartModel, insertModels);
            }

            public RemoveNotesOperation RemoveNotes(ListReadOnlyView<NoteModel> notesInTimeOrder)
            {
                NoteTimeComparer.AssertInOrder(notesInTimeOrder);

                // Record all visible notes, mark their indices
                var removeIndices = new int[notesInTimeOrder.Count];

                var notemodels = _chartModel._visibleNotes;
                int iModel = 0;

                for (int index = 0; index < notesInTimeOrder.Count; index++) {
                    var note = notesInTimeOrder[index];
                    Debug.Assert(note.Data.IsVisible);

                    int i = notemodels.IndexOf(note, iModel);
                    Debug.Assert(i >= 0);
                    removeIndices[index] = i;
                    iModel = i + 1;
                }

                Debug.Assert(IsInOrder(removeIndices));

                return new RemoveNotesOperation(removeIndices, _chartModel);

                static bool IsInOrder(IEnumerable<int> indices)
                {
                    if (!indices.Any())
                        return true;
                    var prev = indices.First();

                    foreach (var item in indices.Skip(1)) {
                        if (item < prev)
                            return false;
                        prev = item;
                    }
                    return true;
                }
            }

            public LinkNotesOperation LinkNotes(ListReadOnlyView<NoteModel> notes)
            {
                var editNotes = new NoteModel[notes.Count];
                for (int i = 0; i < notes.Count; i++) {
                    editNotes[i] = notes[i];
                }

                return new LinkNotesOperation(editNotes);
            }

            public UnlinkNotesOperation UnlinkNotes(ListReadOnlyView<NoteModel> notes)
            {
                var editNotes = new NoteModel[notes.Count];
                for (int i = 0; i < notes.Count; i++) {
                    editNotes[i] = notes[i];
                }

                return new UnlinkNotesOperation(editNotes);
            }

            public EditNotesPropertyOperation<T> EditNotes<T>(ListReadOnlyView<NoteModel> notes, T value,
                Func<NoteData, T> valueGetter, Action<NoteData, T> valueSetter)
            {
                var contexts = Utils.Array<(NoteModel Note, T OldValue)>(notes.Count);
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, valueGetter(note.Data));
                }
                return new EditNotesPropertyOperation<T>(contexts, value, valueSetter, _chartModel);
            }

            public EditNotesPropertyOperation<T> EditNotes<T>(ListReadOnlyView<NoteModel> notes,
                Func<T, T> valueSelector, Func<NoteData, T> valueGetter, Action<NoteData, T> valueSetter)
            {
                var contexts = Utils.Array<(NoteModel Note, T OldValue)>(notes.Count);
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, valueGetter(notes[i].Data));
                }
                return new EditNotesPropertyOperation<T>(contexts, valueSelector, valueSetter, _chartModel);
            }

            public EditNotesSoundsOperation EditNotesSounds(ListReadOnlyView<NoteModel> notes,
                PianoSoundValueData[] valueData)
            {
                var contexts = Utils.Array<(NoteModel, PianoSoundValueData[])>(notes.Count);
                for (int i = 0; i < notes.Count; i++) {
                    var note = notes[i];
                    var sounds = note.Data.Sounds;
                    var datas = Utils.Array<PianoSoundValueData>(sounds.Count);
                    for (int j = 0; j < sounds.Count; i++) {
                        datas[j] = sounds[i].GetValues();
                    }
                    contexts[i] = (note, datas);
                }

                return new EditNotesSoundsOperation(contexts, valueData);
            }

            #region Helpers

            private ListReadOnlyView<NoteModel> GetCollidedNotesTo(int noteIndex)
            {
                var collidedNotes = new List<NoteModel>();

                var editNote = _chartModel.Notes[noteIndex];
                for (int i = noteIndex - 1; i >= 0; i--) {
                    var note = _chartModel.Notes[i];
                    if (!MainSystem.Args.IsTimeCollided(note.Data, editNote.Data))
                        break;
                    if (MainSystem.Args.IsPositionCollided(note.Data, editNote.Data)) {
                        collidedNotes.Add(note);
                    }
                }
                for (int i = noteIndex + 1; i < _chartModel.Notes.Count; i++) {
                    var note = _chartModel.Notes[i];
                    if (!MainSystem.Args.IsTimeCollided(editNote.Data, note.Data))
                        break;
                    if (MainSystem.Args.IsPositionCollided(editNote.Data, note.Data)) {
                        collidedNotes.Add(note);
                    }
                }

                return collidedNotes.Count == 0 ? ListReadOnlyView<NoteModel>.Empty : collidedNotes;
            }

            #endregion

            #region Operation implementation

            public sealed class AddNoteOperation : IUndoableOperation
            {
                private readonly int _modelInsertIndex;
                private readonly ChartModel _chartModel;
                private readonly NoteModel _note;

                private Action? _onRedone;
                private Action? _onUndone;

                private ListReadOnlyView<NoteModel> _collidedNotes;

                // Unity 什么时候支持 C#12.jpg
                public AddNoteOperation(int modelInsertIndex, ChartModel chartModel, NoteModel noteModel)
                {
                    _modelInsertIndex = modelInsertIndex;
                    _chartModel = chartModel;
                    _note = noteModel;
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
                    _chartModel._visibleNotes.Insert(_modelInsertIndex, _note);
                    CheckCollision();
                    _onRedone?.Invoke();

                    void CheckCollision()
                    {
                        if (_collidedNotes.IsNull) {
                            _collidedNotes = _chartModel.Notes.GetCollidedNotesTo(_modelInsertIndex);
                        }

                        _note.CollisionCount += _collidedNotes.Count;
                        foreach (var n in _collidedNotes) {
                            n.CollisionCount++;
                        }
                    }
                }

                void IUndoableOperation.Undo()
                {
                    Debug.Assert(_chartModel._visibleNotes[_modelInsertIndex] == _note);
                    _chartModel._visibleNotes.RemoveAt(_modelInsertIndex);
                    RevertCollision();
                    _onUndone?.Invoke();

                    void RevertCollision()
                    {
                        _note.CollisionCount -= _collidedNotes.Count;
                        foreach (var n in _collidedNotes) {
                            n.CollisionCount--;
                        }
                    }
                }
            }

            public sealed class AddMultipleNotesOperation : IUndoableOperation
            {
                private readonly int[] _insertIndices;
                private readonly ChartModel _chartModel;
                private readonly NoteModel[] _notes;

                private Action<NoteModel[]>? _onRedone;
                private Action<NoteModel[]>? _onUndone;

                private readonly ListReadOnlyView<NoteModel>[] _collidedNotes;

                public AddMultipleNotesOperation(int[] insertIndices, ChartModel chartModel, NoteModel[] noteModels)
                {
                    Debug.Assert(insertIndices.Length == noteModels.Length);
                    _insertIndices = insertIndices;
                    _chartModel = chartModel;
                    _notes = noteModels;
                    _collidedNotes = new ListReadOnlyView<NoteModel>[_insertIndices.Length];
                }

                public AddMultipleNotesOperation WithRedoneAction(Action<NoteModel[]> action)
                {
                    _onRedone = action;
                    return this;
                }

                public AddMultipleNotesOperation WithUndoneAction(Action<NoteModel[]> action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        // We need to maually adjust offset when insert from start
                        _chartModel._visibleNotes.Insert(_insertIndices[i] + i, _notes[i]);
                        CheckCollision(i);
                    }
                    _onRedone?.Invoke(_notes);

                    void CheckCollision(int index)
                    {
                        ref var collidedNotes = ref _collidedNotes[index];
                        if (collidedNotes.IsNull) {
                            collidedNotes =
                                _chartModel.Notes.GetCollidedNotesTo(_insertIndices[index] + index);
                        }
                        _notes[index].CollisionCount += collidedNotes.Count;
                        foreach (var note in collidedNotes) {
                            note.CollisionCount++;
                        }
                    }
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = _notes.Length - 1; i >= 0; i--) {
                        Debug.Assert(_chartModel._visibleNotes[_insertIndices[i] + i] == _notes[i]);
                        _chartModel._visibleNotes.RemoveAt(_insertIndices[i] + i);
                    }
                    RevertCollision();
                    _onUndone?.Invoke(_notes);

                    void RevertCollision()
                    {
                        for (int i = 0; i < _collidedNotes.Length; i++) {
                            var collidedNotes = _collidedNotes[i];
                            _notes[i].CollisionCount -= collidedNotes.Count;
                            foreach (var note in collidedNotes) {
                                note.CollisionCount--;
                            }
                        }
                    }
                }
            }

            public sealed class RemoveNotesOperation : IUndoableOperation
            {
                private readonly int[] _removeIndices;
                private readonly NoteModel[] _removeModels;

                private readonly ChartModel _chartModel;

                private readonly UnlinkNotesOperation _unlinkOperation;

                private Action? _onRedone;
                private Action<NoteModel[]>? _onUndone;
                private readonly ListReadOnlyView<NoteModel>[] _collidedNotes;

                public RemoveNotesOperation(int[] removeIndices, ChartModel chartModel)
                {
                    _removeIndices = removeIndices;
                    _chartModel = chartModel;
                    _removeModels = new NoteModel[removeIndices.Length];
                    for (int i = 0; i < _removeModels.Length; i++) {
                        _removeModels[i] = _chartModel._visibleNotes[removeIndices[i]];
                    }
                    _collidedNotes = new ListReadOnlyView<NoteModel>[_removeIndices.Length];

                    _unlinkOperation = new UnlinkNotesOperation(_removeModels);
                }

                public RemoveNotesOperation WithRedoneAction(Action action)
                {
                    _onRedone = action;
                    return this;
                }

                public RemoveNotesOperation WithUndoneAction(Action<NoteModel[]> action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    ((IUndoableOperation)_unlinkOperation).Redo();

                    for (int i = _removeIndices.Length - 1; i >= 0; i--) {
                        UpdateCollision(i);
                        _chartModel._visibleNotes.RemoveAt(_removeIndices[i]);
                    }
                    _onRedone?.Invoke();

                    void UpdateCollision(int index)
                    {
                        ref var collidedNotes = ref _collidedNotes[index];
                        if (collidedNotes.IsNull) {
                            collidedNotes = _chartModel.Notes.GetCollidedNotesTo(_removeIndices[index]);
                        }
                        foreach (var note in collidedNotes) {
                            note.CollisionCount--;
                        }
                    }
                }

                void IUndoableOperation.Undo()
                {
                    ((IUndoableOperation)_unlinkOperation).Undo();

                    for (int i = 0; i < _removeModels.Length; i++) {
                        var model = _removeModels[i];
                        _chartModel._visibleNotes.Insert(_removeIndices[i], model);
                    }
                    RevertCollision();
                    _onUndone?.Invoke(_removeModels);

                    void RevertCollision()
                    {
                        foreach (var collidedNotes in _collidedNotes) {
                            foreach (var note in collidedNotes) {
                                note.CollisionCount--;
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

            public sealed class EditNotesPropertyOperation<T> : IUndoableOperation
            {
                private readonly (NoteModel Note, T OldValue)[] _contexts;
                private readonly T _newValue;
                private readonly Func<T, T> _newValueSelector;
                private readonly Action<NoteData, T> _valueSetter;
                private readonly ChartModel _chartModel;

                private Action? _onDone;

                private bool _isRequireSortNotes;
                private bool _isRequireUpdateCollision;
                private int[]? _beforeNoteIndices;
                private int[]? _afterNoteIndices;
                private ListReadOnlyView<NoteModel>[]? _beforeCollidedNotes;
                private ListReadOnlyView<NoteModel>[]? _afterCollidedNotes;
                private (bool? IsInsertBefore, NoteData BeforeRefNote, NoteData AfterRefNote)[] _linkChangeDatas;

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

                public EditNotesPropertyOperation<T> WithOptions(bool sortNotes = false, bool updateCollision = false)
                {
                    _isRequireSortNotes = sortNotes;
                    _isRequireUpdateCollision = sortNotes || updateCollision;
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
                    EnsureBeforeAdjustInited(out var requiresLaterInit);

                    for (int i = 0; i < _contexts.Length; i++) {
                        var (note, oldValue) = _contexts[i];
                        _valueSetter(note.Data,
                            _newValueSelector is null ? _newValue : _newValueSelector.Invoke(oldValue));
                        if (requiresLaterInit)
                            InitAfterSortNotesDatas(i);
                    }
                    OnRedoneSortNotes();

                    if (requiresLaterInit)
                        InitAfterCollisionDatas();
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

                private void OnRedoneSortNotes()
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

                private void OnRedoneUpdateCollisions()
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

                private void OnUndone()
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
            }

            public sealed class EditNotesSoundsOperation : IUndoableOperation
            {
                private readonly (NoteModel Note, PianoSoundValueData[] OldValues)[] _contexts;
                private readonly PianoSoundValueData[] _newValue;

                private Action? _onDone;

                public EditNotesSoundsOperation((NoteModel Note, PianoSoundValueData[] OldValues)[] contexts,
                    PianoSoundValueData[] newValue)
                {
                    _contexts = contexts;
                    _newValue = newValue;
                }

                public EditNotesSoundsOperation WithDoneAction(Action action)
                {
                    _onDone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    foreach (var (note, _) in _contexts) {
                        SetValue(note, _newValue);
                    }
                    _onDone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    foreach (var (note, data) in _contexts) {
                        SetValue(note, data);
                    }
                    _onDone?.Invoke();
                }

                private void SetValue(NoteModel note, PianoSoundValueData[] value)
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