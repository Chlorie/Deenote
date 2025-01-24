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
        float Position { get; }
    }

    public static class StageNoteNodeExt
    {
        /// <summary>
        /// Check whether the <paramref name="note"/> reached judgeline, the combo should increment or not
        /// </summary>
        public static bool IsComboNode(this IStageNoteNode note)
            => note is NoteTailNode or NoteModel { IsHold: false };
    }
}
