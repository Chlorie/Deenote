#nullable enable

using Deenote.Entities.Operations;
using Deenote.Library.Collections.Generic;

namespace Deenote.Core.Editing
{
    public sealed class OperationMemento
    {
        private const int MaxOperationUndoCount = 100;

        private readonly Memento<IUndoableOperation> _memento;
        private int _saveOffset;

        public bool CanRedo => _memento.ActiveCount >= _memento.Count;

        public bool CanUndo => _memento.ActiveCount > 0;

        public bool HasUnsavedChange => _saveOffset != 0;

        public OperationMemento()
        {
            _memento = new(MaxOperationUndoCount);
        }

        public void Do(IUndoableOperation? operation)
        {
            if (operation is null)
                return;

            _memento.Add(operation);
            operation.Redo();
            _saveOffset++;
        }

        public void Redo()
        {
            if (_memento.TryReapply(out var operation)) {
                operation.Redo();
                _saveOffset++;
            }
        }

        public void Undo()
        {
            if (_memento.TryRollback(out var operation)) {
                operation.Undo();
                _saveOffset--;
            }
        }

        public void Reset()
        {
            _memento.Clear();
            _saveOffset = 0;
        }

        public void SaveAtCurrent()
        {
            _saveOffset = 0;
        }
    }
}