using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote.Project.Comparers
{
    public sealed class NoteTimeComparer : IComparer<IStageNoteModel>, IComparer<NoteData>
    {
        public static readonly NoteTimeComparer Instance = new();

        public int Compare(IStageNoteModel x, IStageNoteModel y) => Comparer<float>.Default.Compare(x.Time, y.Time);
        public int Compare(NoteData x, NoteData y) => Comparer<float>.Default.Compare(x.Time, y.Time);

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertInOrder(IEnumerable<IStageNoteModel> notes, string? additionMessage = null)
            => AssertInOrder(notes.Select(n => n.Time), additionMessage);

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void AssertInOrder(IEnumerable<NoteData> notes, string? additionMessage = null)
            => AssertInOrder(notes.Select(n => n.Time), additionMessage);

        public static void AssertInOrder<T>(ReadOnlySpan<T> notes, string? additionMessage = null) where T : IStageNoteModel
            => AssertInOrder(notes, n => n.Time, additionMessage);

        public static void AssertInOrder(ReadOnlySpan<NoteData> notes, string? additionMessage = null)
            => AssertInOrder(notes, n => n.Time, additionMessage);

        private static void AssertInOrder(IEnumerable<float> times, string? additionMessage)
        {
            using var enumerator = times.GetEnumerator();
            if (!enumerator.MoveNext())
                return;
            var prev = enumerator.Current;
            int iPrev = 0;

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                int iCurr = iPrev + 1;
                if (curr < prev) {
                    Debug.Assert(false, $"Notes (#{iPrev}, #{iCurr}) not in order: {additionMessage}");
                }
                prev = curr;
                iPrev = iCurr;
            }
        }

        private static void AssertInOrder<T>(ReadOnlySpan<T> values, Func<T, float> timeGetter, string? additionMessage)
        {
            var enumerator = values.GetEnumerator();
            if (!enumerator.MoveNext())
                return;
            var prev = enumerator.Current;

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                if (timeGetter(curr) < timeGetter(prev)) {
                    Debug.Assert(false, $"Notes not in order: {additionMessage}");
                }
                prev = curr;
            }
        }
    }
}