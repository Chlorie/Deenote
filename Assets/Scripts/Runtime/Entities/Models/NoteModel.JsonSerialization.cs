#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Deenote.Entities.Models
{
    [JsonObject(MemberSerialization.OptIn, IsReference = true)]
    partial class NoteModel
    {
        /// <remarks>
        /// I found the enum defination while decompiling DEEMO, so I know its structure,
        /// but looks like that it makes no effect on note display.
        /// And in DEEMO II, the property has been removed, so we decide not to serialize it
        /// </remarks>
        //[JsonProperty("type", Order = 0)]
        [Obsolete("For json serialzation only, deprecated proeprty")]
        private NoteType_Legacy _serializeType = NoteType_Legacy.Hit;

        [JsonProperty("sounds", Order = 1)]
        internal List<PianoSoundValueModel> _sounds;

        [JsonProperty("pos", Order = 2)]
        internal float _position;

        [JsonProperty("size", Order = 3)]
        internal float _size = 1f;

        [JsonProperty("_time", Order = 4)]
        internal float _time;

        /// <remarks>
        /// Unknown property
        /// </remarks>
        [JsonProperty("shift", Order = 5)]
        internal float _shift;

        /// <summary>
        /// Note speed in DEEMO II
        /// </summary>
        [JsonProperty("speed", Order = 6, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(1f)]
        internal float _speed = 1f;

        /// <summary>
        /// Hold duration in DEEMO II
        /// </summary>
        [JsonProperty("duration", Order = 7)]
        private float _duration;

        /// <remarks>
        /// Unknown property, in DEEMO II.
        /// <br/>
        /// This property seems to be removed in latest DEEMO II chart
        /// </remarks>
        [JsonProperty("vibrate", Order = 8)]
        [Obsolete("For json serialization only, deprecated property")]
        private bool _vibrate;

        /// <summary>
        /// Is swipe in DEEMO II V2
        /// </summary>
        [JsonProperty("swipe", Order = 9)]
        [Obsolete("For json serialzation only")]
        private bool _SerializeSwipe => Kind is NoteKind.Swipe;

        /// <remarks>
        /// May be speed warning in DEEMO II
        /// </remarks>
        [JsonProperty("warningType", Order = 10, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(WarningType.Default)]
        private WarningType _warningType = WarningType.Default;

        [JsonProperty("eventId", Order = 11)]
        [DefaultValue("")]
        private string _eventId = "";

        /// <summary>
        /// Another time property without underline prefix,
        /// We treat Time(_time in json) as the real time of the note,
        /// Make this property invalid if the usage of "time" in json is unclear 
        /// </summary>
        [JsonProperty("time", Order = 12)]
        [Obsolete("For json serialzation only, use Time instead", true)]
        private float _SerializeDuplicatedTime => _time;

        [JsonConstructor, Obsolete("For json serialzation only")]
        private NoteModel(NoteType_Legacy type, List<PianoSoundValueModel>? sounds,
            float pos, float size, float _time, float shift,
            float speed, float duration, bool vibrate, bool swipe,
            WarningType warningType, string eventId, string time)
        {
            _serializeType = type;
            _sounds = sounds ?? new();
            _position = pos;
            _size = size;
            this._time = _time;
            _shift = shift;
            _speed = speed;
            _duration = duration;
            _vibrate = vibrate;
            if (swipe)
                Kind = NoteKind.Swipe;
            _warningType = warningType;
        }

        [Obsolete("For json serialization only")]
        private enum NoteType_Legacy { Hit = 0, Slide = 1 }
    }
}