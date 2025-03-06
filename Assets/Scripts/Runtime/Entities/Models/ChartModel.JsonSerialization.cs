#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Entities.Models.Serialization;
using Deenote.Library.Collections;
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

        [JsonProperty("oriVMin", Order = 1), Obsolete("For serialzation only")]
        private int _SerializeMinVolume => _SerializeNotes.SelectMany(n => n.Sounds, (n, s) => s.Velocity).Min();

        [JsonProperty("oriVMax", Order = 2), Obsolete("For serialzation only")]
        private int _SerializeMaxVolume => _SerializeNotes.SelectMany(n => n.Sounds, (n, s) => s.Velocity).Max();

        [JsonProperty("remapVMin", Order = 3)]
        public int RemapMinVolume { get; set; }

        [JsonProperty("remapVMax", Order = 4)]
        public int RemapMaxVolume { get; set; }

        [JsonProperty("notes", Order = 5), Obsolete("For serialzation only")]
        private IEnumerable<NoteModel> _SerializeNotes
            => _visibleNoteNodes.OfType<NoteModel>()
                .Merge(_backgroundNotes.Select(n => n._noteModel), NoteTimeComparer.Instance);

        [JsonProperty("links", Order = 6), Obsolete("For serialzation only")]
        private IEnumerable<NoteLinkIterator> _SerializeLinks
            => _SerializeNotes
                .Where(n => n.IsSlide && n.PrevLink is null)
                .Select(n => new NoteLinkIterator(n));

        [JsonProperty("lines", Order = 7), Obsolete("For serialzation only")]
        private IEnumerable<SpeedLineValueModel.Serializer> _SerializeLines
            => _speedLines.Adjacent()
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
            Marshal.DeserializeNotes(notes.AsSpanOrEmpty(), out _holdCount, out _visibleNoteNodes!, out _backgroundNotes!, out _speedChangeWarnings!);
            _visibleNoteNodes ??= new();
            _backgroundNotes ??= new();
            DeserializeSpeedLines(lines);

            if (links is null)
                return;

            foreach (var link in links) {
                NoteModel? prev = null;
                foreach (var note in link.Notes) {
                    note._kind = NoteModel.NoteKind.Slide;
                    note._prevLink = prev;
                    if (prev != null)
                        prev._nextLink = note;
                    prev = note;
                }
            }
        }

        [MemberNotNull(nameof(_speedLines))]
        private void DeserializeSpeedLines(List<SpeedLineValueModel.Serializer>? lines)
        {
            if (lines is null || lines.Count == 0) {
                _speedLines = new() { new SpeedLineValueModel(1f, 0f, WarningType.Default) };
                return;
            }

            lines.Sort((x, y) => Comparer<float>.Default.Compare(x.StartTime, y.StartTime));

            var speedLines = new List<SpeedLineValueModel>();
            float prevEndTime = 0f;

            foreach (var line in lines) {
                if (line.StartTime > prevEndTime) {
                    speedLines.Add(new(1f, prevEndTime, WarningType.Default));
                }
                speedLines.Add(new(line.Speed, line.StartTime, line.WarningType));
                prevEndTime = line.EndTime;
            }
            speedLines.Add(new(1f, prevEndTime, WarningType.Default));

            Debug.Assert(_visibleNoteNodes.OfType<NoteModel>()
                .All(note =>
                {
                    var line = speedLines.Last(l => l.StartTime <= note.Time);
                    return line.Speed == note.Speed;
                }));

            _speedLines = speedLines;
        }

        public static bool TryParse(string json, [NotNullWhen(true)] out ChartModel? chart)
        {
            try {
                chart = JsonConvert.DeserializeObject<ChartModel>(json);
                if (chart is not null) return true;
            } catch (Exception) {
                /* ignored */
            }

            try {
                chart = ChartAdapter.ParseDeV3Json(json);
                if (chart is not null) return true;
            } catch (Exception) {
                /* ignored */
            }

            chart = null;
            return false;
        }
    }
}