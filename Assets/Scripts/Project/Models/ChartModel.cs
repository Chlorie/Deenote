#nullable enable

using Deenote.Project.Comparers;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Deenote.Project.Models
{
    public sealed partial class ChartModel
    {
        // Nullable, display Difficulty.ToString() on placeholder when null
        public string? Name { get; set; }

        public Difficulty Difficulty { get; set; }

        public string Level { get; set; } = "";

        /// <remarks>
        /// Data.Notes will not update once be loaded from file, use <see cref="Notes"/> to
        /// get note infos of the chart
        /// </remarks>
        public ChartData Data { get; private set; }

        // NonSerialize

        private int _holdCount;
        private List<IStageNoteModel> _visibleNotes = null!;
        private List<NoteData> _backgroundNotes = null!;

        /// <summary>
        /// Notes that will be displayed on stage.
        /// <br/>
        /// For hold notes, there will be 2 objects represent the
        /// hold head and hold tail, use <see cref="NoteModelListProxy.EnumerateSelectableModels"/> get distinct notes
        /// </summary>
        public NoteModelListProxy Notes => new(this);

        public ChartModel(ChartData data)
        {
            SetData(data);
        }


        [MemberNotNull(nameof(Data))]
        private void SetData(ChartData data)
        {
            Data = data;
            _holdCount = 0;
            if (_visibleNotes is null) {
                _visibleNotes = new();
                _backgroundNotes = new();
            }
            else {
                _visibleNotes.Clear();
                _backgroundNotes.Clear();
            }

            var tailsBuffer = new List<(float EndTime, NoteModel Note)>();

            foreach (var note in data.Notes) {
                if (note.IsVisible) {
                    while (TryGetFirst(out var tail) && tail.EndTime < note.Time) {
                        _visibleNotes.Add(new NoteTailModel(tail.Note));
                        _holdCount++;
                        tailsBuffer.RemoveAt(0);
                    }
                    var noteModel = new NoteModel(note);
                    _visibleNotes.Add(noteModel);
                    if (note.IsHold) {
                        InsertInOrder(noteModel);
                    }
                }
                else {
                    _backgroundNotes.Add(note);
                }
            }

            if (tailsBuffer.Count > 0) {
                foreach (var (_, note) in tailsBuffer) {
                    _visibleNotes.Add(new NoteTailModel(note));
                    _holdCount++;
                }
            }

            NoteTimeComparer.AssertInOrder(_visibleNotes);
            NoteTimeComparer.AssertInOrder(_backgroundNotes);

            void InsertInOrder(NoteModel note)
            {
                var endTime = note.Data.EndTime;
                int i = 0;
                for (; i < tailsBuffer.Count; i++) {
                    var (time, _) = tailsBuffer[i];
                    if (time > endTime)
                        break;
                }
                tailsBuffer.Insert(i, (endTime, note));
            }

            bool TryGetFirst(out (float EndTime, NoteModel Note) tail)
            {
                if (tailsBuffer.Count > 0) {
                    tail = tailsBuffer[0];
                    return true;
                }
                tail = default;
                return false;
            }
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
            var notes = chart.Data.Notes;
            notes.Capacity = _visibleNotes.Count + _backgroundNotes.Count;
            var iterNotes = _visibleNotes
                .OfType<NoteModel>()
                .Select(n => n.Data)
                .Merge(_backgroundNotes, NoteTimeComparer.Instance);
            foreach (var note in iterNotes) {
                notes.Add(CloneNote(note));
            }
            return chart;

            static NoteData CloneNote(NoteData note)
            {
                var clone = note.Clone();
                if (clone.IsSwipe)
                    clone.Duration = 0f;
                return clone;
            }
        }

        public readonly partial struct NoteModelListProxy : IReadOnlyList<IStageNoteModel>
        {
            private readonly ChartModel _chartModel;

            public NoteModelListProxy(ChartModel chart) => _chartModel = chart;

            public IStageNoteModel this[int index] => _chartModel._visibleNotes[index];

            /// <summary>
            /// Count of all <see cref="IStageNoteModel"/>, including <see cref="NoteModel"/>
            /// and <see cref="NoteTailModel"/>
            /// </summary>
            public int Count => _chartModel._visibleNotes.Count;

            /// <summary>
            /// Count of <see cref="NoteModel"/>, also the total combo in game
            /// </summary>
            public int NoteCount => Count - _chartModel._holdCount;

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
                    if (near.Time != note.Data.Time)
                        break;

                    if (ReferenceEquals(near, note))
                        return i;
                }

                for (int i = index + 1; i < _chartModel._visibleNotes.Count; i++) {
                    var near = _chartModel._visibleNotes[i];
                    if (near.Time != note.Data.Time)
                        break;

                    if (ReferenceEquals(near, note))
                        return i;
                }

                return ~index;
            }

            public int IndexOfTailOf(int noteModelIndex)
            {
                if (this[noteModelIndex] is not NoteModel { Data.IsHold: true } head)
                    return -1;

                for (int i = noteModelIndex + 1; i < Count; i++) {
                    var note = this[i];
                    if (note is NoteTailModel tail && tail.HeadModel == head)
                        return i;
                }

                Debug.Assert(false, "Chart contains a hold note but not its tail model");
                return -1;
            }

            public int IndexOfTailOf(NoteModel note)
            {
                if (!note.Data.IsHold)
                    return -1;

                var noteIndex = Search(note);
                if (noteIndex < 0)
                    return -1;
                return IndexOfTailOf(noteIndex);
            }

            #region Helpers

            /// <param name="noteIndex">Index of a <see cref="NoteModel"/></param>
            /// <returns></returns>
            private IReadOnlyList<NoteModel> GetCollidedNotesTo(int noteIndex)
            {
                Debug.Assert(this[noteIndex] is NoteModel);

                var collidedNotes = new List<NoteModel>();
                var editNote = (NoteModel)this[noteIndex];

                for (int i = noteIndex - 1; i >= 0; i--) {
                    if (this[i] is not NoteModel note)
                        continue;

                    if (!MainSystem.Args.IsTimeCollided(note.Data, editNote.Data))
                        break;
                    if (MainSystem.Args.IsPositionCollided(note.Data, editNote.Data)) {
                        collidedNotes.Add(note);
                    }
                }

                for (int i = noteIndex + 1; i < _chartModel.Notes.Count; i++) {
                    if (this[i] is not NoteModel note)
                        continue;

                    if (!MainSystem.Args.IsTimeCollided(editNote.Data, note.Data))
                        break;
                    if (MainSystem.Args.IsPositionCollided(editNote.Data, note.Data)) {
                        collidedNotes.Add(note);
                    }
                }

                return collidedNotes.Count == 0 ? Array.Empty<NoteModel>() : collidedNotes;
            }

            #endregion

            public ReadOnlySpan<IStageNoteModel> AsSpan() => _chartModel._visibleNotes.AsSpan();

            public List<IStageNoteModel>.Enumerator GetEnumerator() => _chartModel._visibleNotes.GetEnumerator();

            public CollectionUtils.SpanOfTypeIterator<IStageNoteModel, NoteModel> EnumerateSelectableModels()
                => AsSpan().OfType<IStageNoteModel, NoteModel>();

            IEnumerator<IStageNoteModel> IEnumerable<IStageNoteModel>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static class InitializationHelper
        {
            public static void SetData(ChartModel model, ChartData data)
            {
                if (data == model.Data)
                    return;
                model.SetData(data);
            }
        }
    }
}