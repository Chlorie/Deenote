namespace Deenote.Edit.Operations
{
    public interface IUndoableOperation
    {
        void Redo();
        void Undo();
    }

    public static class UndoableOperation
    {
        public static readonly IUndoableOperation NoOperation = new NoOperationImpl();

        private sealed class NoOperationImpl : IUndoableOperation
        {
            void IUndoableOperation.Redo() { }
            void IUndoableOperation.Undo() { }
        }
    }
}