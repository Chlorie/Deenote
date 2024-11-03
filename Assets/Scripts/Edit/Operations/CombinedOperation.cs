#nullable enable

namespace Deenote.Edit.Operations
{
    public sealed class CombinedOperation : IUndoableOperation
    {
        private readonly IUndoableOperation _first;
        private readonly IUndoableOperation _second;

        public CombinedOperation(IUndoableOperation first, IUndoableOperation second)
        {
            _first = first;
            _second = second;
        }

        public void Redo()
        {
            _first.Redo();
            _second.Redo();
        }
        public void Undo()
        {
            _second.Undo();
            _first.Undo();
        }
    }
}