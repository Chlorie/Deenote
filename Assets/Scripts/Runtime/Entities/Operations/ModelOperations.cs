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

        public abstract class NotifiableChartOperation<TArgs> : NotifiableOperation<TArgs>, IUndoableChartOperation
        {
            protected readonly ChartModel _chart;

            protected NotifiableChartOperation(ChartModel chart)
            {
                _chart = chart;
            }

            ChartModel IUndoableChartOperation.Chart => _chart;
        }

        public abstract class EditNotesPropertyOperationBase<TProperty> : OperationBase, IUndoableChartOperation
        {
            protected readonly ChartModel _chart;
            protected readonly ImmutableArray<NoteModel> _notes;
            protected readonly ImmutableArray<TProperty> _oldValues;

            private readonly ValueProvider _newValueProvider;

            private bool _firstRedo = true;

            protected EditNotesPropertyOperationBase(ChartModel chart,
                ImmutableArray<NoteModel> notes,
                ImmutableArray<TProperty> oldValues,
                ValueProvider valueProvider)
            {
                Debug.Assert(notes.Length == oldValues.Length, "Note count should match value count");
                _chart = chart;
                _notes = notes;
                _oldValues = oldValues;
                _newValueProvider = valueProvider;
            }

            protected EditNotesPropertyOperationBase(ChartModel chart,
                ImmutableArray<NoteModel> notes,
                Func<NoteModel, TProperty> oldValuesSelector,
                ValueProvider valueProvider)
            {
                _chart = chart;
                _notes = notes;
                _oldValues = notes.Select(oldValuesSelector).ToImmutableArray();
                _newValueProvider = valueProvider;
            }

            private Action<ImmutableArray<NoteModel>>? _onDone;

            ChartModel IUndoableChartOperation.Chart => _chart;

            public EditNotesPropertyOperationBase<TProperty> OnDone(Action<ImmutableArray<NoteModel>> action)
            {
                _onDone = action;
                return this;
            }

            protected override void Redo()
            {
                OnRedoing(_firstRedo);
                for (int i = 0; i < _notes.Length; i++) {
                    var note = _notes[i];
                    var old = _oldValues[i];
                    var newv = _newValueProvider.GetNewValue(old);
                    OnRedoingValueChanging(_firstRedo, i, newv);
                    SetValue(note, newv);
                    OnRedoingValueChanged(_firstRedo, i, newv);
                }
                OnRedone(_firstRedo);
                _onDone?.Invoke(_notes);

                _firstRedo = false;
            }

            protected override void Undo()
            {
                for (int i = _notes.Length - 1; i >= 0; i--) {
                    var note = _notes[i];
                    var old = _oldValues[i];
                    OnUndoingValueChanging(i);
                    SetValue(note, old);
                    OnUndoingValueChanged(i);
                }
                OnUndone();
                _onDone?.Invoke(_notes);
            }

            protected abstract void SetValue(NoteModel note, TProperty value);
            protected virtual void OnRedoingValueChanging(bool isFirstRedo, int index, TProperty newValue) { }
            protected virtual void OnRedoingValueChanged(bool isFirstRedo, int index, TProperty newValue) { }
            protected virtual void OnUndoingValueChanging(int index) { }
            protected virtual void OnUndoingValueChanged(int index) { }

            protected virtual void OnRedoing(bool isFirstRedo) { }
            protected virtual void OnRedone(bool isFirstRedo) { }
            protected virtual void OnUndone() { }

            protected internal readonly struct ValueProvider
            {
                private readonly TProperty? _value;
                private readonly Func<TProperty, TProperty>? _valueSelector;

                public ValueProvider(TProperty value)
                {
                    _value = value;
                    _valueSelector = null;
                }

                public ValueProvider(Func<TProperty, TProperty> valueSelector)
                {
                    _value = default;
                    _valueSelector = valueSelector;
                }

                public TProperty GetNewValue(TProperty oldValue)
                {
                    if (_valueSelector is null) {
                        Debug.Assert(_value is not null);
                        return _value!;
                    }
                    else {
                        return _valueSelector.Invoke(oldValue);
                    }
                }

                public static implicit operator ValueProvider(TProperty value) => new(value);
                public static implicit operator ValueProvider(Func<TProperty, TProperty> valueSelector) => new(valueSelector);
            }
        }
    }
}