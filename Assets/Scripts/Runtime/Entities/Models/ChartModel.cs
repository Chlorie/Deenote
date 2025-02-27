#nullable enable

using Deenote.Entities.Comparisons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
    }
}