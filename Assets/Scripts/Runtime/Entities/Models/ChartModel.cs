#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Library.Collections;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.Entities.Models
{
    public sealed partial class ChartModel
    {
        public string Name { get; set; } = "";

        public Difficulty Difficulty { get; set; }

        public string Level { get; set; } = "";

        public ChartModel(float speed = 6f, int remapMinVolume = 10, int remapMaxVolume = 70)
        {
            Speed = speed;
            RemapMinVolume = remapMinVolume;
            RemapMaxVolume = remapMaxVolume;
            _holdCount = 0;
            _visibleNoteNodes = new List<IStageNoteNode>();
            _backgroundNotes = new List<SoundNoteModel>();
            _speedLines = new List<SpeedLineValueModel>() { new(1f, 0f, WarningType.Default) };
        }

        public ChartModel Clone()
        {
            var chart = new ChartModel(Speed, RemapMinVolume, RemapMaxVolume) {
                Name = Name,
                Difficulty = Difficulty,
                Level = Level,
            };

            NoteTimeComparer.AssertInOrder(_visibleNoteNodes);
            NoteTimeComparer.AssertInOrder(_backgroundNotes);

            chart._visibleNoteNodes.Capacity = _visibleNoteNodes.Capacity;
            chart._holdCount = _holdCount;
            var clones = new Dictionary<NoteModel, NoteModel>(_visibleNoteNodes.Count - _holdCount);
            foreach (var note in _visibleNoteNodes) {
                if (note is NoteModel model) {
                    var cloneNote = model.Clone();
                    chart._visibleNoteNodes.Add(cloneNote);
                    if (model.IsHold) {
                        clones.Add(model, cloneNote);
                    }
                }
                else if (note is NoteTailNode tail) {
                    var cloneNote = new NoteTailNode(clones[tail.HeadModel]);
                    chart._visibleNoteNodes.Add(cloneNote);
                }
                else {
                    throw new InvalidOperationException("Unknown IStageNoteNode type");
                }
            }

            chart._backgroundNotes.Capacity = _backgroundNotes.Capacity;
            foreach (var note in _backgroundNotes) {
                chart._backgroundNotes.Add(note.Clone());
            }

            chart._speedLines.Capacity = _speedLines.Capacity;
            foreach (var line in _speedLines) {
                chart._speedLines.Add(line);
            }

            return chart;
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public readonly partial struct NoteModelListProxy : IReadOnlyList<IStageNoteNode>
        {
            private readonly ChartModel _chartModel;

            public NoteModelListProxy(ChartModel chart) => _chartModel = chart;

            public IStageNoteNode this[int index] => _chartModel._visibleNoteNodes[index];

            /// <summary>
            /// Count of all <see cref="IStageNoteNode"/>, including <see cref="NoteModel"/>
            /// and <see cref="NoteTailNode"/>
            /// </summary>
            public int Count => _chartModel._visibleNoteNodes.Count;

            /// <summary>
            /// Count of <see cref="NoteModel"/>, also the total combo in game
            /// </summary>
            public int NoteCount => Count - _chartModel._holdCount;

            public ReadOnlySpan<IStageNoteNode> AsSpan() => _chartModel._visibleNoteNodes.AsSpan();

            public List<IStageNoteNode>.Enumerator GetEnumerator() => _chartModel._visibleNoteNodes.GetEnumerator();

            public CollectionUtils.SpanOfTypeIterator<IStageNoteNode, NoteModel> EnumerateSelectableModels()
                => AsSpan().OfType<IStageNoteNode, NoteModel>();

            IEnumerator<IStageNoteNode> IEnumerable<IStageNoteNode>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}