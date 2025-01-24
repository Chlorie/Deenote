#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deenote.Entities.Models.Serialization
{
    internal static class ChartAdapter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings {
            ReferenceResolverProvider = () => new DeserializerReferenceResolver(),
        });

        public static ChartModel? ParseDeV3Json(string json)
        {
            JObject? jobj = JsonConvert.DeserializeObject<JObject>(json);
            if (jobj is null)
                return null;

            // Cast $ref to string
            // or else deserialization will fail, idk why
            foreach (var lktok in jobj["links"]!) {
                foreach (var reftok in lktok["notes"]!) {
                    reftok["$ref"] = reftok["ref"]!.ToString();
                }
            }

            var chart = jobj.ToObject<DeV3Chart>(_serializer);
            if (chart is null)
                return null;

            // Convert to chart model

            var notes = chart.notes.Select(static note =>
            {
                var model = new NoteModel {
                    Position = note.pos,
                    Time = note._time,
                    Size = note.size,
                };
                model._sounds.AddRange(note.sounds);
                return model;
            }).ToList();

            var links = chart.links.Select(link => new NoteLinkIterator.Deserializer(link.notes.Select(
                nref =>
                {
                    int index = nref.id - 1;
                    if (chart.notes[index].id != nref.id)
                        throw new FormatException("Wrong note id");
                    return notes[index];
                }) ?? Enumerable.Empty<NoteModel>()));
            return new ChartModel(chart.speed, oriVMin: 0, oriVMax: 0, remapVMin: 10, remapVMax: 70,
                notes, links, null);
        }

        private sealed class DeserializerReferenceResolver : IReferenceResolver
        {
            public void AddReference(object context, string reference, object value) =>
                throw new NotSupportedException();

            public string GetReference(object context, object value) => throw new NotSupportedException();
            public bool IsReferenced(object context, object value) => throw new NotSupportedException();

            public object ResolveReference(object context, string reference) =>
                new DeV3Link.NoteRef { id = int.Parse(reference) };
        }

        private sealed class DeV3Chart
        {
            public List<DeV3Note> notes = default!;
            public IEnumerable<DeV3Link> links = default!;
            public float speed = default;
        }

        private sealed class DeV3Link
        {
            public IEnumerable<NoteRef> notes = default!;

            public sealed class NoteRef
            {
                [JsonProperty("$ref")]
                public int id = default;
            }
        }

        private sealed class DeV3Note
        {
            public float _time = default;
            [JsonProperty("$id")]
            public int id = default;
            public List<PianoSoundValueModel> sounds = default!;
            public float pos = default;
            public float size = default;
        }

    }
}