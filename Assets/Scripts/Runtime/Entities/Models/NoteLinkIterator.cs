#nullable enable

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Deenote.Entities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public readonly struct NoteLinkIterator
    {
        private readonly NoteModel _head;

        [JsonProperty("notes")]
        public IEnumerable<NoteModel> Notes
        {
            get {
                NoteModel? current = _head;
                while (current is not null) {
                    yield return current;
                    current = current._nextLink;
                }
            }
        }

        public NoteLinkIterator(NoteModel linkHead) => _head = linkHead;

        internal readonly struct Deserializer
        {
            public readonly IEnumerable<NoteModel> Notes;

            [JsonConstructor]
            public Deserializer(IEnumerable<NoteModel> notes) => Notes = notes;
        }
    }
}