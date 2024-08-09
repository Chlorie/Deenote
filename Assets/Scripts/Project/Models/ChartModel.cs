using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Deenote.Project.Models
{
    public sealed partial class ChartModel
    {
        public string Name { get; set; }

        public Difficulty Difficulty { get; set; }

        public string Level { get; set; }

        private ChartData _data;
        public ChartData Data
        {
            get => _data;
            set {
                if (_data == value)
                    return;

                _data = value;
                _visibleNotes = new List<NoteModel>();
                foreach (var note in _data.Notes) {
                    if (note.IsVisible) {
                        _visibleNotes.Add(new NoteModel(note));
                    }
                }
            }
        }

        // NonSerialize
        private List<NoteModel> _visibleNotes;
        public NoteModelListProxy Notes => new(this);

        public ChartModel(ChartData data)
        {
            Data = data;
        }

        public readonly partial struct NoteModelListProxy : IReadOnlyList<NoteModel>
        {
            private readonly ChartModel _chartModel;

            public NoteModelListProxy(ChartModel chart) => _chartModel = chart;

            public NoteModel this[int index] => _chartModel._visibleNotes[index];

            public int Count => _chartModel._visibleNotes.Count;

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
    }
}