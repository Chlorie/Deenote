using Deenote.Project.Models.Datas;
using System.Collections;
using System.Collections.Generic;

namespace Deenote.Project.Models
{
    public sealed partial class ChartModel
    {
        public string Name { get; set; }
        public Difficulty Difficulty { get; set; }
        public string Level { get; set; }
        public ChartData Data { get; set; }

        // NonSerialize
        private readonly List<NoteModel> _visibleNotes;
        public NoteModelListProxy Notes => new(this);

        public ChartModel(ChartData data)
        {
            Data = data;
            _visibleNotes = new(data.Notes.Capacity);
            for (int i = 0; i < data.Notes.Count; i++) {
                var note = data.Notes[i];
                if (note.IsVisible) {
                    _visibleNotes.Add(new NoteModel(note));
                }
            }
        }

        public readonly partial struct NoteModelListProxy : IReadOnlyList<NoteModel>
        {
            private readonly ChartModel _chartModel;

            public NoteModelListProxy(ChartModel chart) => _chartModel = chart;

            public NoteModel this[int index] => _chartModel._visibleNotes[index];

            public int Count => _chartModel._visibleNotes.Count;

            public List<NoteModel>.Enumerator GetEnumerator() => _chartModel._visibleNotes.GetEnumerator();

            IEnumerator<NoteModel> IEnumerable<NoteModel>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}