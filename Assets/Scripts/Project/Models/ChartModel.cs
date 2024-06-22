using Deenote.Project.Models.Datas;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public NoteModelList Notes => new(this);

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

        public readonly partial struct NoteModelList : IReadOnlyList<NoteModel>
        {
            private readonly ChartModel _chartModel;

            public NoteModelList(ChartModel chart) => _chartModel = chart;

            public NoteModel this[int index] => _chartModel._visibleNotes[index];

            public int Count => _chartModel._visibleNotes.Count;

            public AddNoteOperation Add(NoteCoord coord, NoteData notePrototype)
            {
                NoteData data = notePrototype.Clone();
                data.Position = coord.Position;
                data.Time = coord.Time;
                NoteModel model = new(data);

                var noteModels = _chartModel._visibleNotes;
                int iModel;
                for (iModel = noteModels.Count - 1; iModel >= 0; iModel--) {
                    if (noteModels[iModel].Data.Time <= coord.Time) {
                        break;
                    }
                }
                iModel++;

                // TODO:Optimize
                var notedatas = _chartModel.Data.Notes;
                int iData;
                for (iData = notedatas.Count - 1; iData >= 0; iData--) {
                    if (notedatas[iData].Time <= coord.Time) {
                        break;
                    }
                }
                iData++;

                return new AddNoteOperation(iModel, iData, _chartModel, model);
            }

            public RemoveNotesOperation RemoveNotes(List<NoteModel> noteInTimeOrder)
            {
                NoteTimeComparer.AssertInOrder(noteInTimeOrder);

                // Record all visible notes, mark their indices
                var removeModelIndices = new List<int>();

                var notemodels = _chartModel._visibleNotes;
                int iModel = 0;
                foreach (var note in noteInTimeOrder) {
                    if (!note.Data.IsVisible)
                        continue;

                    for (; iModel < notemodels.Count; iModel++) {
                        if (notemodels[iModel] == note) {
                            removeModelIndices.Add(iModel);
                            break; // continue MoveNext();
                        }
                    }
                    Debug.Assert(iModel < notemodels.Count);
                    iModel++;
                }

                var removeDataIndices = new int[noteInTimeOrder.Count];
                int index = 0;

                var notedatas = _chartModel.Data.Notes;
                int iData = 0;
                foreach (var note in noteInTimeOrder) {
                    for (; iData < notedatas.Count; iData++) {
                        if (notedatas[iData] == note.Data) {
                            removeDataIndices[index++] = iData;
                            break;// continue iterate selected notes
                        }
                    }
                    Debug.Assert(iData < notedatas.Count);
                    iData++;
                }

                Debug.Assert(IsInOrder(removeDataIndices));
                Debug.Assert(IsInOrder(removeModelIndices));

                return new RemoveNotesOperation(removeDataIndices, removeModelIndices, _chartModel);

                static bool IsInOrder(IEnumerable<int> indices)
                {
                    if (!indices.Any())
                        return true;
                    var prev = indices.First();

                    foreach (var item in indices.Skip(1)) {
                        if (item < prev)
                            return false;
                        prev = item;
                    }
                    return true;
                }

            }
            public List<NoteModel>.Enumerator GetEnumerator() => _chartModel._visibleNotes.GetEnumerator();

            IEnumerator<NoteModel> IEnumerable<NoteModel>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}