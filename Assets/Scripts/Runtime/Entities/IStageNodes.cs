#nullable enable

using Deenote.Entities.Models;

namespace Deenote.Entities
{
    public interface IStageTimeNode
    {
        float Time { get; }
    }

    public interface IStageNoteNode : IStageTimeNode
    {
        float Speed { get; }
        float Position { get; }

        bool IsComboNode { get; }
    }
}
