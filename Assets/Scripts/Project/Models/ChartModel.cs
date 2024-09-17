using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.Project.Models
{
    public sealed partial class ChartModel
    {
        public string Name { get; set; } = "";

        public Difficulty Difficulty { get; set; }

        public string Level { get; set; } = "";

        /// <remarks>
        /// Data.Notes will not update once be loaded from file, use <see cref="Notes"/> to
        /// get note infos of the chart
        /// </remarks>
        public ChartData Data { get; private set; }

        // NonSerialize
        private List<NoteModel> _visibleNotes = null!;
        private List<NoteData> _backgroundNotes = null!;
        public NoteModelListProxy Notes => new(this);

        public ChartModel(ChartData data)
        {
            InitializationHelper.SetData(this, data);
        }

        public ChartModel CloneForSave()
        {
            var chart = new ChartModel(Data.Clone(cloneNotes: false, cloneLines: true)) {
                Name = Name,
                Difficulty = Difficulty,
                Level = Level,
            };

            NoteTimeComparer.AssertInOrder(_visibleNotes);
            NoteTimeComparer.AssertInOrder(_backgroundNotes);
            var notes = Data.Notes;
            notes.Capacity = _visibleNotes.Count + _backgroundNotes.Count;
            foreach (var note in MergeNotes()) {
                notes.Add(CloneNote(note));
            }
            return chart;

            NoteData CloneNote(NoteData note)
            {
                var clone = note.Clone();
                if (clone.IsSwipe)
                    clone.Duration = 0f;
                return clone;
            }

            IEnumerable<NoteData> MergeNotes()
            {
                using var fg = _visibleNotes.GetEnumerator();
                using var bg = _backgroundNotes.GetEnumerator();

                switch (fg.MoveNext(), bg.MoveNext()) {
                    case (true, true): goto CompareAndNext;
                    case (true, false): goto IterForeground;
                    case (false, true): goto IterBackground;
                    case (false, false): yield break;
                }

            CompareAndNext:
                var f = fg.Current;
                var b = bg.Current;
                while (true) {
                    if (f.Data.Time <= b.Time) {
                        yield return f.Data;
                        if (fg.MoveNext()) {
                            f = fg.Current;
                            continue;
                        }
                        goto IterBackground;
                    }
                    else {
                        yield return b;
                        if (bg.MoveNext()) {
                            b = bg.Current;
                            continue;
                        }
                        goto IterForeground;
                    }
                }

            IterForeground:
                do {
                    yield return fg.Current.Data;
                } while (fg.MoveNext());
                yield break;

            IterBackground:
                do {
                    yield return bg.Current;
                } while (bg.MoveNext());
                yield break;
            }
        }

        public readonly partial struct NoteModelListProxy : IReadOnlyList<NoteModel>
        {
            private readonly ChartModel _chartModel;

            public NoteModelListProxy(ChartModel chart) => _chartModel = chart;

            public NoteModel this[int index] => _chartModel._visibleNotes[index];

            public int Count => _chartModel._visibleNotes.Count;

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            public int Search(NoteModel note)
            {
                var index = _chartModel._visibleNotes.BinarySearch(note, NoteTimeComparer.Instance);
                if (index < 0)
                    return index;

                var find = _chartModel._visibleNotes[index];
                if (ReferenceEquals(find, note))
                    return index;

                for (int i = index - 1; i >= 0; i--) {
                    var near = _chartModel._visibleNotes[i];
                    if (near.Data.Time != note.Data.Time)
                        break;

                    if (ReferenceEquals(near, note))
                        return i;
                }

                for (int i = index + 1; i < _chartModel._visibleNotes.Count; i++) {
                    var near = _chartModel._visibleNotes[i];
                    if (near.Data.Time != note.Data.Time)
                        break;

                    if (ReferenceEquals(near, note))
                        return i;
                }

                return ~index;
            }

            public List<NoteModel>.Enumerator GetEnumerator() => _chartModel._visibleNotes.GetEnumerator();

            IEnumerator<NoteModel> IEnumerable<NoteModel>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static class InitializationHelper
        {
            public static void SetData(ChartModel model, ChartData data)
            {
                if (model.Data == data)
                    return;

                model.Data = data;
                if (model._visibleNotes is null) {
                    model._visibleNotes = new();
                    model._backgroundNotes = new();
                }
                else {
                    model._visibleNotes.Clear();
                    model._backgroundNotes.Clear();
                }

                foreach (var note in data.Notes) {
                    if (note.IsVisible) {
                        model._visibleNotes.Add(new NoteModel(note));
                    }
                    else {
                        model._backgroundNotes.Add(note);
                    }
                }
            }
        }
    }
}