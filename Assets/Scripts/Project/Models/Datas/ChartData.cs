using Deenote.Project.Comparers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Project.Models.Datas
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ChartData
    {
        [SerializeField]
        [JsonProperty("speed")]
        public float Speed;

        [SerializeField]
        [JsonProperty("oriVMin")]
        public int MinVelocity;
        [SerializeField]
        [JsonProperty("oriVMax")]
        public int MaxVelocity;

        [SerializeField]
        [JsonProperty("remapVMin")]
        public int RemapMinVelocity;
        [SerializeField]
        [JsonProperty("remapVMax")]
        public int RemapMaxVelocity;

        [SerializeField]
        [JsonProperty("notes")]
        private List<NoteData> _notes;
        public List<NoteData> Notes => _notes;

        /// <summary>
        /// Lazy load
        /// </summary>
        [JsonProperty("links")]
        public IEnumerable<Link> Links
        {
            get {
                foreach (var note in Notes) {
                    if (note.IsSlide && note.PrevLink is null)
                        yield return new Link(note);
                }
            }
        }

        [SerializeField]
        [JsonProperty("lines")]
        private List<SpeedLine> _speedLines;
        public List<SpeedLine> SpeedLines => _speedLines ??= new();

        #region Constructors

        public ChartData() : this(6.0f, 10, 70, new(), new())
        { }

        private ChartData(float speed, int remapMinVelocity, int remapMaxVelocity, List<NoteData> notes, List<SpeedLine> lines)
        {
            Speed = speed;
            RemapMinVelocity = remapMinVelocity;
            RemapMaxVelocity = remapMaxVelocity;
            _notes = notes ?? new();
            _speedLines = lines ?? new();

            // Why are there unordered notes in official chart ???
            // I mean Magnolia hard $id 1093 and 1094
            _notes.Sort(NoteTimeComparer.Instance);
        }

        [JsonConstructor]
        internal ChartData(float speed, int remapMinVelocity, int remapMaxVelocity, List<NoteData> notes, IEnumerable<Link.Deserialzier> links, List<SpeedLine> speedLines) :
            this(speed, remapMinVelocity, remapMaxVelocity, notes, speedLines)
        {
            if (links is null)
                return;

            foreach (var link in links) {
                NoteData prev = null;
                foreach (var note in link.Notes) {
                    note.IsSlide = true;
                    note.PrevLink = prev;
                    if (prev != null)
                        prev.NextLink = note;
                    prev = note;
                }
            }
        }

        #endregion

        public static ChartData Load(string json)
        {
            return JsonConvert.DeserializeObject<ChartData>(json);
        }

        public static bool TryLoad(string json, out ChartData chart)
        {
            try {
                chart = Load(json);
            } catch (Exception) {
                chart = null;
                return false;
            }
            return true;
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}