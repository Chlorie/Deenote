#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models.Serialization;
using Deenote.Library.Collections;
using Deenote.Library.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Deenote.Entities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    partial class ChartModel
    {
        [JsonProperty("speed", Order = 0)]
        public float Speed { get; set; }

        [ChartSerializationVersion(ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("oriVMin", Order = 1), Obsolete("For serialzation only")]
        private int _SerializeMinVolume => _SerializeNotes.SelectMany(n => n.Sounds, (n, s) => s.Velocity).Min();

        [ChartSerializationVersion(ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("oriVMax", Order = 2), Obsolete("For serialzation only")]
        private int _SerializeMaxVolume => _SerializeNotes.SelectMany(n => n.Sounds, (n, s) => s.Velocity).Max();

        [ChartSerializationVersion(ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("remapVMin", Order = 3)]
        public int RemapMinVolume { get; set; }

        [ChartSerializationVersion(ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("remapVMax", Order = 4)]
        public int RemapMaxVolume { get; set; }

        [JsonProperty("notes", Order = 5), Obsolete("For serialzation only")]
        private IEnumerable<NoteModel> _SerializeNotes
            => NoteNodes.OfType<NoteModel>()
                .Merge(BackgroundSoundNotes.Select(n => n._noteModel), NodeTimeComparer.Instance);

        [JsonProperty("links", Order = 6), Obsolete("For serialzation only")]
        private IEnumerable<NoteLinkIterator> _SerializeLinks
            => _SerializeNotes
                .Where(n => n.IsSlide && n.PrevLink is null)
                .Select(n => new NoteLinkIterator(n));

        [ChartSerializationVersion(ChartSerializationVersions.DeemoIIV2)]
        [JsonProperty("lines", Order = 7), Obsolete("For serialzation only")]
        private IEnumerable<SpeedLineValueModel.Serializer> _SerializeLines
            => SpeedLines.Adjacent()
                .Where(tpl => tpl.Item1.Speed != 1f)
                .Select(tpl => new SpeedLineValueModel.Serializer(tpl.Item1.Speed, tpl.Item1.StartTime, tpl.Item2.StartTime, tpl.Item1.WarningType));

        [JsonConstructor]
        internal ChartModel(float speed, int oriVMin, int oriVMax, int remapVMin, int remapVMax,
            List<NoteModel>? notes,
            IEnumerable<NoteLinkIterator.Deserializer>? links,
            List<SpeedLineValueModel.Serializer>? lines)
        {
            Speed = speed;
            // MinVolume=oriVMin;
            // MaxVolume=oriVMax;
            RemapMinVolume = remapVMin;
            RemapMaxVolume = remapVMax;
            Marshal.DeserializeNotes(notes.AsSpanOrEmpty(), out _holdCount, out var visibleNotes, out var soundNotes, out var speedChanges);
            NoteNodes = visibleNotes ?? new(NodeTimeUniqueComparer.Instance);
            BackgroundSoundNotes = soundNotes ?? new(NodeTimeComparer.Instance);
            SpeedChangeWarnings = speedChanges ??= new(NodeTimeComparer.Instance);
            DeserializeSpeedLines(lines);

            if (links is null)
                return;

            foreach (var link in links) {
                NoteModel? prev = null;
                foreach (var note in link.Notes) {
                    note.Kind = NoteModel.NoteKind.Slide;
                    note._prevLink = prev;
                    if (prev != null)
                        prev._nextLink = note;
                    prev = note;
                }
            }
        }

        [MemberNotNull(nameof(SpeedLines))]
        private void DeserializeSpeedLines(List<SpeedLineValueModel.Serializer>? lines)
        {
            if (lines is null || lines.Count == 0) {
                SpeedLines = new(NodeTimeComparer.Instance) { new SpeedLineValueModel(1f, 0f, WarningType.Default) };
                return;
            }

            lines.Sort((x, y) => Comparer<float>.Default.Compare(x.StartTime, y.StartTime));

            var speedLines = new SortedList<SpeedLineValueModel>(NodeTimeComparer.Instance);
            float prevEndTime = 0f;

            foreach (var line in lines) {
                if (line.StartTime > prevEndTime) {
                    speedLines.AddFromEnd(new(1f, prevEndTime, WarningType.Default));
                }
                speedLines.AddFromEnd(new(line.Speed, line.StartTime, line.WarningType));
                prevEndTime = line.EndTime;
            }
            speedLines.AddFromEnd(new(1f, prevEndTime, WarningType.Default));

            Debug.Assert(NoteNodes.OfType<NoteModel>()
                .All(note =>
                {
                    var line = speedLines.Last(l => l.StartTime <= note.Time);
                    return line.Speed == note.Speed;
                }));

            SpeedLines = speedLines;
        }

        public static bool TryParse(string json, [NotNullWhen(true)] out ChartModel? chart)
        {
            try {
                chart = JsonConvert.DeserializeObject<ChartModel>(json);
                if (chart is not null) return true;
            } catch (Exception) {
                Debug.LogError("Exception on parse chart json");
                /* ignored */
            }

            try {
                chart = ChartAdapter.ParseDeV3Json(json);
                if (chart is not null) return true;
            } catch (Exception) {
                Debug.LogError("Exception on parse v3 chart json");
                /* ignored */
            }

            chart = null;
            return false;
        }
    }
}