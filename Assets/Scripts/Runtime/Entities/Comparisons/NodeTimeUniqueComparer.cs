#nullable enable

using Deenote.Entities.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.Entities.Comparisons
{
    public sealed class NodeTimeUniqueComparer : IComparer<IStageNoteNode>
    {
        public static NodeTimeUniqueComparer Instance { get; } = new();

        public int Compare(IStageNoteNode x, IStageNoteNode y)
        {
            var cmp = NodeTimeComparer.Instance.Compare(x, y);
            if (cmp != 0) return cmp;
            cmp = Comparer<float>.Default.Compare(x.Speed, y.Speed);
            if (cmp != 0) return cmp;
            cmp = Comparer<uint>.Default.Compare(x.Uid, y.Uid);
            Debug.Assert(cmp != 0);
            return cmp;
        }
    }
}