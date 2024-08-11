using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote.Project.Comparers
{
    public sealed class NoteTimeComparer : IComparer<NoteModel>, IComparer<NoteData>
    {
        public static readonly NoteTimeComparer Instance = new();

        public int Compare(NoteModel x, NoteModel y) => Compare(x.Data, y.Data);
        public int Compare(NoteData x, NoteData y) => Comparer<float>.Default.Compare(x.Time, y.Time);

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertInOrder(IEnumerable<NoteModel> notes, string? additionMessage = null)
            => AssertInOrder(notes.Select(n => n.Data), additionMessage);

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertInOrder(IEnumerable<NoteData> notes, string? additionMessage = null)
        {
            using var enumerator = notes.GetEnumerator();
            if (!enumerator.MoveNext())
                return;
            var prev = enumerator.Current;

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                if (curr!.Time < prev!.Time) {
                    Debug.Assert(false, $"Notes not in order: {additionMessage}");
                }
                prev = curr;
            }
        }
    }
}