#nullable enable

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Deenote.Project.Models.Datas
{
    [JsonObject(MemberSerialization.OptIn)]
    public readonly struct Link
    {
        private readonly NoteData _head;

        /// <summary>
        /// Lazy load
        /// </summary>
        [JsonProperty("notes")]
        public IEnumerable<NoteData> Notes
        {
            get {
                NoteData? current = _head;
                while (current is not null) {
                    yield return current;
                    current = current.NextLink;
                }
            }
        }

        public Link(NoteData linkHead) => _head = linkHead;

        internal readonly struct Deserialzier
        {
            public readonly IEnumerable<NoteData> Notes;

            [JsonConstructor]
            public Deserialzier(IEnumerable<NoteData> notes) => Notes = notes;
        }
    }
}