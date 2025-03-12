#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Entities.Models;
using System;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace Deenote.Entities.Operations
{
    public static partial class ModelOperations
    {
        public abstract class OperationBase : IUndoableOperation
        {
            private bool _redone;

            protected abstract void Redo();
            protected abstract void Undo();

            void IUndoableOperation.Redo()
            {
                if (_redone) {
                    ThrowHelper.ThrowInvalidOperationException("Redo operation repeatly");
                }

                Redo();
                _redone = true;
            }

            void IUndoableOperation.Undo()
            {
                if (!_redone) {
                    ThrowHelper.ThrowInvalidOperationException("Undo operation repeatly");
                }
                Undo();
                _redone = false;
            }
        }

        public abstract class NotifiableOperation<TArgs> : OperationBase
        {
            private Action<TArgs>? _onRedone, _onUndone;

            public NotifiableOperation<TArgs> OnRedone(Action<TArgs> onRedone)
            {
                _onRedone = onRedone;
                return this;
            }

            public NotifiableOperation<TArgs> OnUndone(Action<TArgs> onUndone)
            {
                _onUndone = onUndone;
                return this;
            }

            protected void OnRedone(TArgs args) => _onRedone?.Invoke(args);
            protected void OnUndone(TArgs args) => _onUndone?.Invoke(args);
        }

        public abstract class EditNotesPropertyOperationBase<TProperty> : OperationBase
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

            protected EditNotesPropertyOperationBase(ChartModel chart,
                ImmutableArray<NoteModel> notes,
                Func<NoteModel, TProperty> oldValuesSelector,
                TProperty? newValue, Func<TProperty, TProperty>? newValueSelector)
            {
                Debug.Assert(newValueSelector is not null || newValue is not null, "One of newValue or newValueSelector should not be null");
                _chart = chart;
                _notes = notes;
                _oldValues = notes.Select(oldValuesSelector).ToImmutableArray();
                _newValue = newValue;
                _newValueSelector = newValueSelector;
            }

            private Action<ImmutableArray<NoteModel>>? _onDone;
            public EditNotesPropertyOperationBase<TProperty> OnDone(Action<ImmutableArray<NoteModel>> action)
            {
                _onDone = action;
                return this;
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

            protected override void Redo()
            {
                OnRedoing(_firstRedo);
                for (int i = 0; i < _notes.Length; i++) {
                    var note = _notes[i];
                    var old = _oldValues[i];
                    var newv = GetNewValue(old);
                    OnRedoingValueChanging(_firstRedo, i, newv);
                    SetValue(note, newv);
                    OnRedoingValueChanged(_firstRedo, i, newv);
                }
                OnRedone(_firstRedo);
                _onDone?.Invoke(_notes);
            }

            protected override void Undo()
            {
                for (int i = _notes.Length - 1; i >= 0; i--) {
                    var note = _notes[i];
                    var old = _oldValues[i];
                    SetValue(note, old);
                    OnUndoingValueChanged(i);
                }
                OnUndone();
                _onDone?.Invoke(_notes);
            }

            protected abstract void SetValue(NoteModel note, TProperty value);
            protected virtual void OnRedoingValueChanging(bool isFirstRedo, int index, TProperty newValue) { }
            protected virtual void OnRedoingValueChanged(bool isFirstRedo, int index, TProperty newValue) { }
            protected virtual void OnUndoingValueChanged(int index) { }

            protected virtual void OnRedoing(bool isFirstRedo) { }
            protected virtual void OnRedone(bool isFirstRedo) { }
            protected virtual void OnUndone() { }
        }
    }
}