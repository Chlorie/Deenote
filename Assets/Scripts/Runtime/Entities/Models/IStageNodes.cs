#nullable enable

namespace Deenote.Entities.Models
{
    public interface IStageTimeNode
    {
        float Time { get; }
    }

    public interface IStageNoteNode : IStageTimeNode
    {
        private static uint s_uid;

        float Speed { get; }
        float Position { get; }

        bool IsComboNode { get; }

        internal uint Uid { get; }

        internal static void InitUid(ref uint uid) => uid = s_uid++;
    }
}