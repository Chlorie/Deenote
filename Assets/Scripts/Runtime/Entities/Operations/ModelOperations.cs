#nullable enable

using Deenote.Entities.Models;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Entities.Operations
{
    public static partial class ModelOperations
    {
        public abstract class EditNotesPropertyOperationBase<TProperty> : IUndoableOperation
        {
            protected readonly ChartModel _chart;
            protected readonly ImmutableArray<NoteModel> _notes;
            protected readonly ImmutableArray<TProperty> _oldValues;
            private readonly TProperty? _newValue;
            private readonly Func<TProperty, TProperty>? _newValueSelector;

            private bool _firstRedo = true;

            protected EditNotesPropertyOperationBase(ChartModel chart,
                ImmutableArray<NoteModel> notes,
                ImmutableArray<TProperty> oldValues,
                TProperty? newValue, Func<TProperty, TProperty>? newValueSelector)
            {
                Debug.Assert(notes.Length == oldValues.Length, "Note count should match value count");
                Debug.Assert(newValueSelector is not null || newValue is not null, "One of newValue or newValueSelector should not be null");
                _chart = chart;
                _notes = notes;
                _oldValues = oldValues;
                _newValue = newValue;
                _newValueSelector = newValueSelector;
            }

            private Action<ImmutableArray<NoteModel>>? _onDone;
            public EditNotesPropertyOperationBase<TProperty> OnDone(Action<ImmutableArray<NoteModel>> action)
            {
                _onDone = action;
                return this;
            }

            void IUndoableOperation.Redo()
            {
                for (int i = 0; i < _notes.Length; i++) {
                    var note = _notes[i];
                    var oldValue = _oldValues[i];
                    var newValue = GetNewValue(oldValue);
                    OnRedoingValueChanging(_firstRedo, i, newValue);
                    SetValue(note, newValue);
                    OnRedoingValueChanged(_firstRedo, i, newValue);
                }
                OnRedone(_firstRedo);

                _onDone?.Invoke(_notes);
                _firstRedo = false;
            }

            void IUndoableOperation.Undo()
            {
                for (int i = _notes.Length - 1; i >= 0; i--) {
                    var note = _notes[i];
                    var oldValue = _oldValues[i];
                    SetValue(note, oldValue);
                    OnUndoingValueChanged(i);
                }
                OnUndone();
                _onDone?.Invoke(_notes);
            }

            private TProperty GetNewValue(TProperty oldValue)
            {
                if (_newValueSelector is null) {
                    Debug.Assert(_newValue is not null);
                    return _newValue!;
                }
                else {
                    return _newValueSelector.Invoke(oldValue);
                }
            }

            protected abstract void SetValue(NoteModel note, TProperty value);
            protected virtual void OnRedoingValueChanging(bool isFirstRedo, int index, TProperty newValue) { }
            protected virtual void OnRedoingValueChanged(bool isFirstRedo, int index, TProperty newValue) { }
            protected virtual void OnUndoingValueChanged(int index) { }

            protected virtual void OnRedone(bool isFirstRedo) { }
            protected virtual void OnUndone() { }
        }
    }
}