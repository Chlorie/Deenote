#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models.Serialization;
using Deenote.Library.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

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
            NoteNodes = new SortedList<IStageNoteNode>(NodeTimeUniqueComparer.Instance);
            BackgroundSoundNotes = new SortedList<SoundNoteModel>(NodeTimeComparer.Instance);
            SpeedChangeWarnings = new SortedList<SpeedChangeWarningModel>(NodeTimeComparer.Instance);
            SpeedLines = new SortedList<SpeedLineValueModel>(NodeTimeComparer.Instance) { new(1f, 0f, WarningType.Default) };
        }

        public ChartModel Clone()
        {
            var chart = new ChartModel(Speed, RemapMinVolume, RemapMaxVolume) {
                Name = Name,
                Difficulty = Difficulty,
                Level = Level,
            };

            chart._holdCount = _holdCount;
            foreach (var note in EnumerateNoteModels()) {
                chart.NoteNodes.AddFromEnd(note);
                if (note.IsHold) {
                    chart.NoteNodes.AddFromEnd(new NoteTailNode(note));
                }
            }

            Debug.Assert(chart._holdCount == chart.NoteNodes.OfType<NoteTailNode>().Count());

            chart.BackgroundSoundNotes.Capacity = BackgroundSoundNotes.Capacity;
            foreach (var note in BackgroundSoundNotes) {
                chart.BackgroundSoundNotes.AddFromEnd(note.Clone());
            }

            chart.SpeedChangeWarnings.Capacity = SpeedChangeWarnings.Capacity;
            foreach (var note in SpeedChangeWarnings) {
                chart.SpeedChangeWarnings.AddFromEnd(note.Clone());
            }

            chart.SpeedLines.Capacity = SpeedLines.Capacity;
            foreach (var line in SpeedLines) {
                chart.SpeedLines.AddFromEnd(line);
            }

            return chart;
        }

        public string ToJsonString()
        {
            return ToJsonString(ChartSerializationVersion.DeemoIIV2);
        }

        public string ToJsonString(ChartSerializationVersion version)
        {
            var versions = version switch {
                ChartSerializationVersion.DeemoV2 => ChartSerializationVersions.DeemoV2,
                ChartSerializationVersion.DeemoIIV2 => ChartSerializationVersions.DeemoIIV2,
                _ => throw new NotImplementedException(),
            };
            this.GenerateSpeedLines();
            return ChartSerializer.Serialize(this, versions);
        }
    }
}