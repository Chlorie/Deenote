#nullable enable

using Deenote.Entities.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.Entities.Comparisons
{
    public sealed class NodeUniqueComparer : IComparer<IStageNoteNode>
    {
        public static NodeUniqueComparer Instance { get; } = new();

        public int Compare(IStageNoteNode x, IStageNoteNode y)
        {
            return Comparer<uint>.Default.Compare(x.Uid, y.Uid);
        }
    }
}