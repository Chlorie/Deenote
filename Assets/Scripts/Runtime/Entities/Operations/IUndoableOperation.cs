#nullable enable

namespace Deenote.Entities.Operations
{
    public interface IUndoableOperation
    {
        void Redo();
        void Undo();
    }
}