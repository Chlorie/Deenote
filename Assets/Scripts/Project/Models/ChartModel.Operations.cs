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

                // Optimizable full iteration?
                var notedatas = _chartModel.Data.Notes;
                int iData;
                for (iData = notedatas.Count - 1; iData >= 0; iData--) {
                    if (notedatas[iData].Time <= coord.Time) {
                        break;
                    }
                }
                iData++;

                NoteModel model = new(data);
                return new AddNoteOperation(iModel, iData, _chartModel, model);
            }

            public AddMultipleNotesOperation AddMultipleNotes(NoteCoord baseCoord, ListReadOnlyView<NoteData> notePrototypes)
            {
                NoteTimeComparer.AssertInOrder(notePrototypes);

                var insertIndices = Utils.Array<(int ModelIndex, int DataIndex)>(notePrototypes.Count);

                var noteModels = _chartModel._visibleNotes;
                int iModel = _chartModel.Notes.Count - 1;
                var noteDatas = _chartModel.Data.Notes;
                int iData = noteDatas.Count - 1;
                for (int i = notePrototypes.Count - 1; i >= 0; i--) {
                    var note = notePrototypes[i];
                    var realTime = note.Time + baseCoord.Time;

                    while (noteModels[iModel].Data.Time > realTime)
                        iModel--;
                    insertIndices[i].ModelIndex = iModel + 1;

                    while (noteDatas[iData].Time > realTime)
                        iData--;
                    insertIndices[i].DataIndex = iData + 1;
                }

                NoteModel[] insertModels = Utils.Array<NoteModel>(notePrototypes.Count);
                for (int i = 0; i < notePrototypes.Count; i++) {
                    var insertNote = notePrototypes[i].Clone();
                    var coord = NoteCoord.ClampPosition(new(insertNote.Position + baseCoord.Position, insertNote.Time + baseCoord.Time));
                    insertNote.PositionCoord = coord;
                    insertModels[i] = new(insertNote);
                }
                return new AddMultipleNotesOperation(insertIndices, _chartModel, insertModels);
            }

            public RemoveNotesOperation RemoveNotes(ListReadOnlyView<NoteModel> noteInTimeOrder)
            {
                NoteTimeComparer.AssertInOrder(noteInTimeOrder);

                // Record all visible notes, mark their indices
                var removeIndices = Utils.Array<(int ModelIndex, int DataIndex)>(noteInTimeOrder.Count);
                int index = 0;

                var notemodels = _chartModel._visibleNotes;
                int iModel = 0;
                var notedatas = _chartModel.Data.Notes;
                int iData = 0;

                foreach (var note in noteInTimeOrder) {
                    Debug.Assert(note.Data.IsVisible);

                    int i = notemodels.IndexOf(note, iModel);
                    Debug.Assert(i >= 0);
                    removeIndices[index].ModelIndex = i;
                    iModel = i + 1;

                    i = notedatas.IndexOf(note.Data, iData);
                    Debug.Assert(i >= 0);
                    removeIndices[index].DataIndex = i;
                    iData = i + 1;
                }

                Debug.Assert(IsInOrder(removeIndices));

                return new RemoveNotesOperation(removeIndices, _chartModel);

                static bool IsInOrder(IEnumerable<(int, int)> indices)
                {
                    if (!indices.Any())
                        return true;
                    var prev = indices.First();

                    foreach (var item in indices.Skip(1)) {
                        if (item.Item1 < prev.Item1 || item.Item2 < prev.Item2)
                            return false;
                        prev = item;
                    }
                    return true;
                }
            }

            public LinkNotesOperation LinkNotes(ListReadOnlyView<NoteModel> notes)
            {
                var editNotes = Utils.Array<NoteModel>(notes.Count);
                for (int i = 0; i < notes.Count; i++) {
                    editNotes[i] = notes[i];
                }

                return new LinkNotesOperation(editNotes);
            }

            public UnlinkNotesOperation UnlinkNotes(ListReadOnlyView<NoteModel> notes)
            {
                var editNotes = Utils.Array<NoteModel>(notes.Count);

                for (int i = 0; i < notes.Count; i++) {
                    editNotes[i] = notes[i];
                }

                return new UnlinkNotesOperation(editNotes);
            }

            public EditNotesPropertyOperation<T> EditNotes<T>(ListReadOnlyView<NoteModel> notes, T value, Func<NoteData, T> valueGetter, Action<NoteData, T> valueSetter)
            {
                var contexts = Utils.Array<(NoteModel Note, T OldValue)>(notes.Count);
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, valueGetter(note.Data));
                }
                return new EditNotesPropertyOperation<T>(contexts, value, valueSetter);
            }

            public EditNotesPropertyOperation<T> EditNotes<T>(ListReadOnlyView<NoteModel> notes, Func<T, T> valueSelector, Func<NoteData, T> valueGetter, Action<NoteData, T> valueSetter)
            {
                var contexts = Utils.Array<(NoteModel Note, T OldValue)>(notes.Count);
                for (int i = 0; i < contexts.Length; i++) {
                    var note = notes[i];
                    contexts[i] = (note, valueGetter(note.Data));
                }
                return new EditNotesPropertyOperation<T>(contexts, valueSelector, valueSetter);
            }

            public EditNotesSoundsOperation EditNotesSounds(ListReadOnlyView<NoteModel> notes, PianoSoundValueData[] valueData)
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

            #region Operation implementation

            public sealed class AddNoteOperation : IUndoableOperation
            {
                private readonly int _modelInsertIndex;
                private readonly int _dataInsertIndex;
                private readonly ChartModel _chartModel;
                private readonly NoteModel _note;

                private Action _onRedone;
                private Action _onUndone;

                // Unity 什么时候支持 C#12.jpg
                public AddNoteOperation(int modelInsertIndex, int dataInsertIndex, ChartModel chartModel, NoteModel noteModel)
                {
                    _modelInsertIndex = modelInsertIndex;
                    _dataInsertIndex = dataInsertIndex;
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
                    _chartModel.Data.Notes.Insert(_dataInsertIndex, _note.Data);
                    _onRedone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    Debug.Assert(_chartModel._visibleNotes[_modelInsertIndex] == _note);
                    Debug.Assert(_chartModel.Data.Notes[_dataInsertIndex] == _note.Data);
                    _chartModel._visibleNotes.RemoveAt(_modelInsertIndex);
                    _chartModel.Data.Notes.RemoveAt(_dataInsertIndex);
                    _onUndone?.Invoke();
                }
            }

            public sealed class AddMultipleNotesOperation : IUndoableOperation
            {
                private readonly (int ModelIndex, int DataIndex)[] _insertIndices;
                private readonly ChartModel _chartModel;
                private readonly NoteModel[] _notes;

                private Action<NoteModel[]> _onRedone;
                private Action _onUndone;

                public AddMultipleNotesOperation((int ModelIndex, int DataIndex)[] insertIndices, ChartModel chartModel, NoteModel[] noteModels)
                {
                    _insertIndices = insertIndices;
                    _chartModel = chartModel;
                    _notes = noteModels;
                }

                public AddMultipleNotesOperation WithRedoneAction(Action<NoteModel[]> action)
                {
                    _onRedone = action;
                    return this;
                }

                public AddMultipleNotesOperation WithUndoneAction(Action action)
                {
                    _onUndone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        // We need to maually adjust offset when insert from start
                        _chartModel._visibleNotes.Insert(_insertIndices[i].ModelIndex + i, _notes[i]);
                        _chartModel.Data.Notes.Insert(_insertIndices[i].DataIndex + i, _notes[i].Data);
                    }
                    _onRedone?.Invoke(_notes);
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = _notes.Length - 1; i >= 0; i--) {
                        Debug.Assert(_chartModel._visibleNotes[_insertIndices[i].ModelIndex + i] == _notes[i]);
                        Debug.Assert(_chartModel.Data.Notes[_insertIndices[i].DataIndex + i] == _notes[i].Data);
                        _chartModel._visibleNotes.RemoveAt(_insertIndices[i].ModelIndex + i);
                        _chartModel.Data.Notes.RemoveAt(_insertIndices[i].DataIndex + i);
                    }
                    _onUndone?.Invoke();
                }
            }

            public sealed class RemoveNotesOperation : IUndoableOperation
            {
                private readonly (int ModelIndex, int DataIndex)[] _removeIndices;
                private readonly NoteModel[] _removeModels;

                private readonly ChartModel _chartModel;

                private Action _onRedone;
                private Action<NoteModel[]> _onUndone;

                public RemoveNotesOperation((int ModelIndex, int DataIndex)[] removeIndices, ChartModel chartModel)
                {
                    _removeIndices = removeIndices;
                    _chartModel = chartModel;
                    _removeModels = Utils.Array<NoteModel>(removeIndices.Length);
                    for (int i = 0; i < _removeModels.Length; i++) {
                        _removeModels[i] = _chartModel._visibleNotes[removeIndices[i].ModelIndex];
                        Debug.Assert(_removeModels[i].Data == _chartModel.Data.Notes[removeIndices[i].DataIndex]);
                    }
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
                    for (int i = _removeIndices.Length - 1; i >= 0; i--) {
                        var (modelIndex, dataIndex) = _removeIndices[i];
                        _chartModel._visibleNotes.RemoveAt(modelIndex);
                        _chartModel.Data.Notes.RemoveAt(dataIndex);
                    }
                    _onRedone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = 0; i < _removeModels.Length; i++) {
                        var model = _removeModels[i];
                        var (modelIndex, dataIndex) = _removeIndices[i];
                        _chartModel._visibleNotes.Insert(modelIndex, model);
                        _chartModel.Data.Notes.Insert(dataIndex, model.Data);
                    }
                    _onUndone?.Invoke(_removeModels);
                }
            }

            public sealed class LinkNotesOperation : IUndoableOperation
            {
                //private readonly (NoteModel Note, (bool IsSlide, NoteData Prev, NoteData Next) OldValue, (NoteData Prev, NoteData Next) NewValue)[] _contexts;

                private readonly NoteModel[] _notes;
                private readonly (bool IsSlide, NoteData Prev, NoteData Next)[] _oldValues;

                private Action _onRedone;
                private Action _onUndone;

                public LinkNotesOperation(NoteModel[] notes)
                {
                    _notes = notes;
                    _oldValues = Utils.Array<(bool IsSlide, NoteData Prev, NoteData Next)>(_notes.Length);

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
                            if (data.PrevLink != null)
                                data.PrevLink.NextLink = data.NextLink;
                            if (data.NextLink != null)
                                data.NextLink.PrevLink = data.PrevLink;
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

                        if (data.PrevLink != null)
                            data.PrevLink.NextLink = data.NextLink;
                        if (data.NextLink != null)
                            data.NextLink.PrevLink = data.PrevLink;

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
                private readonly (bool IsSlide, NoteData Prev, NoteData Next)[] _oldValues;

                private Action _onRedone;
                private Action _onUndone;

                public UnlinkNotesOperation(NoteModel[] contexts)
                {
                    _notes = contexts;
                    _oldValues = Utils.Array<(bool IsSlide, NoteData Prev, NoteData Next)>(_notes.Length);

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
                            if (data.PrevLink != null)
                                data.PrevLink.NextLink = data.NextLink;
                            if (data.NextLink != null)
                                data.NextLink.PrevLink = data.PrevLink;
                        }
                        data.IsSlide = false;
                    }
                    _onRedone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = 0; i < _notes.Length; i++) {
                        var data = _notes[i].Data;
                        var (isSlide, prev, next) = _oldValues[i];
                        Debug.Assert(!data.IsSlide);

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

                private Action _onDone;

                public EditNotesPropertyOperation((NoteModel Note, T OldValue)[] contexts, T newValue, Action<NoteData, T> edit)
                {
                    _contexts = contexts;
                    _newValue = newValue;
                    _valueSetter = edit;
                }

                public EditNotesPropertyOperation((NoteModel Note, T OldValue)[] contexts, Func<T, T> newValueSelector, Action<NoteData, T> valueSetter)
                {
                    _contexts = contexts;
                    _newValueSelector = newValueSelector;
                    _valueSetter = valueSetter;
                }

                public EditNotesPropertyOperation<T> WithDoneAction(Action action)
                {
                    _onDone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    if (_newValueSelector is null) {
                        foreach (var (note, _) in _contexts) {
                            _valueSetter(note.Data, _newValue);
                        }
                    }
                    else {
                        foreach (var (note, oldValue) in _contexts) {
                            _valueSetter(note.Data, _newValueSelector.Invoke(oldValue));
                        }
                    }
                    _onDone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    foreach (var (note, oldVal) in _contexts) {
                        _valueSetter(note.Data, oldVal);
                    }
                    _onDone?.Invoke();
                }
            }

            public sealed class EditNotesSoundsOperation : IUndoableOperation
            {
                private readonly (NoteModel Note, PianoSoundValueData[] OldValues)[] _contexts;
                private readonly PianoSoundValueData[] _newValue;

                private Action _onDone;

                public EditNotesSoundsOperation((NoteModel Note, PianoSoundValueData[] OldValues)[] contexts, PianoSoundValueData[] newValue)
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