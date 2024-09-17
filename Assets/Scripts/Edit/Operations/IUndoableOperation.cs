namespace Deenote.Edit.Operations
{
    public interface IUndoableOperation
    {
        void Redo();
        void Undo();
    }
}