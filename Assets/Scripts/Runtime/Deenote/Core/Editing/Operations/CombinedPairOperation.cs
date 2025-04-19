#nullable enable

using Deenote.Entities.Operations;

namespace Deenote.Core.Editing.Operations
{
    public sealed class CombinedPairOperation : IUndoableOperation
    {
        private readonly IUndoableOperation _first;
        private readonly IUndoableOperation _second;

        public CombinedPairOperation(IUndoableOperation first, IUndoableOperation second)
        {
            _first = first;
            _second = second;
        }

        void IUndoableOperation.Redo()
        {
            _first.Redo();
            _second.Redo();
        }

        void IUndoableOperation.Undo()
        {
            _second.Undo();
            _first.Undo();
        }
    }
}