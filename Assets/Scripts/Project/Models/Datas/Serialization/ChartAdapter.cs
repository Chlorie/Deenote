using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deenote.Project.Models.Datas.Serialization
{
    internal static class ChartAdapter
    {
        private static readonly JsonSerializer _serializer = JsonSerializer.Create(new JsonSerializerSettings {
            ReferenceResolverProvider = () => new DeserializerReferenceResolver(),
        });

        private sealed class DeserializerReferenceResolver : IReferenceResolver
        {
            public void AddReference(object context, string reference, object value) => throw new NotSupportedException();
            public string GetReference(object context, object value) => throw new NotSupportedException();
            public bool IsReferenced(object context, object value) => throw new NotSupportedException();
            public object ResolveReference(object context, string reference) => new DeV3Link.NoteRef { id = int.Parse(reference) };
        }

        public static ChartData? ParseDeV3Json(string json)
        {
            JObject? jobj = JsonConvert.DeserializeObject<JObject>(json);
            if (jobj is null)
                return null;

            // Cast $ref to string, 
            // or else deserialization failed, idk why
            foreach (var lkTok in jobj["links"]) {
                foreach (var refTok in lkTok["notes"]) {
                    refTok["$ref"] = refTok["ref"].ToString();
                }
            }

            var chart = jobj.ToObject<DeV3Chart>(_serializer);
            if (chart is null)
                return null;

            // Convert to general chart

            List<NoteData> notes = chart.notes.Select(note => new NoteData {
                Position = note.pos,
                Size = note.size,
                Time = note._time,
                Sounds = note.sounds,
            }).ToList();

            IEnumerable<Link.Deserialzier> links = chart.links.Select(link => new Link.Deserialzier(link.notes.Select(nref =>
            {
                int index = nref.id - 1;
                if (chart.notes[index].id == nref.id)
                    return notes[index];
                else
                    throw new FormatException("Wrong note id");
            }) ?? null));

            return new ChartData(chart.speed, 10, 70, notes, links, null);
        }

        private sealed class DeV3Chart
        {
            public List<DeV3Note> notes;
            public IEnumerable<DeV3Link> links;
            public float speed;
        }

        private sealed class DeV3Link
        {
            public IEnumerable<NoteRef> notes;
            public sealed class NoteRef
            {
                [JsonProperty("$ref")]
                public int id;
            }
        }

        private sealed class DeV3Note
        {
            public float _time;
            [JsonProperty("$id")]
            public int id;
            public List<PianoSoundData> sounds;
            public float pos;
            public float size;
        }
    }
}
