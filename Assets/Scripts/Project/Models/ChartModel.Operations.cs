using Deenote.Edit.Operations;
using Deenote.Project.Models.Datas;
using System;
using System.Collections.Generic;

namespace Deenote.Project.Models
{
    partial class ChartModel
    {
        partial struct NoteModelList
        {
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
                    _chartModel._visibleNotes.RemoveAt(_modelInsertIndex);
                    _chartModel.Data.Notes.RemoveAt(_dataInsertIndex);
                    _onUndone?.Invoke();
                }
            }

            public sealed class RemoveNotesOperation : IUndoableOperation
            {
                private readonly int[] _removeDataIndices;
                private readonly List<int> _removeModelIndices;

                private readonly ChartModel _chartModel;

                private readonly NoteData[] _removeDatas;
                private readonly NoteModel[] _removeModels;

                private Action _onRedone;
                private Action<NoteModel[]> _onUndone;

                public RemoveNotesOperation(int[] removeDataIndices, List<int> removeModelIndices, ChartModel chartModel)
                {
                    _removeDataIndices = removeDataIndices;
                    _removeModelIndices = removeModelIndices;
                    _chartModel = chartModel;
                    _removeDatas = new NoteData[_removeDataIndices.Length];
                    for (int i = 0; i < _removeDatas.Length; i++) {
                        _removeDatas[i] = _chartModel.Data.Notes[_removeDataIndices[i]];
                    }
                    _removeModels = new NoteModel[_removeModelIndices.Count];
                    for (int i = 0; i < _removeModels.Length; i++) {
                        _removeModels[i] = _chartModel._visibleNotes[_removeModelIndices[i]];
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
                    for (int i = _removeDataIndices.Length - 1; i >= 0; i--) {
                        _chartModel.Data.Notes.RemoveAt(_removeDataIndices[i]);
                    }
                    for (int i = _removeModelIndices.Count - 1; i >= 0; i--) {
                        _chartModel._visibleNotes.RemoveAt(_removeModelIndices[i]);
                    }
                    _onRedone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    for (int i = 0; i < _removeDatas.Length; i++) {
                        _chartModel.Data.Notes.Insert(_removeDataIndices[i], _removeDatas[i]);
                    }
                    for (int i = 0; i < _removeModels.Length; i++) {
                        _chartModel._visibleNotes.Insert(_removeModelIndices[i], _removeModels[i]);
                    }
                    _onUndone?.Invoke(_removeModels);
                }
            }
        }
    }
}