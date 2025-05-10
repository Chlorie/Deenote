#nullable enable

using Deenote.Entities.Models;

namespace Deenote.Entities.Operations
{
    public interface IUndoableOperation
    {
        void Redo();
        void Undo();
    }

    public interface IUndoableChartOperation
    {
        ChartModel Chart { get; }
    }
}