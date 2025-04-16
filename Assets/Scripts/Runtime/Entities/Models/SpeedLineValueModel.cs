#nullable enable

using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;

namespace Deenote.Entities.Models
{
    // Official json file has "lines" array, which represents a interval in which all notes has same speed,
    // but the array is actually duplicated for chart, as all note has its own speed property.
    // I dont know which property DEEMO II use to parse when game playing, I use note's property, and the
    // SpeedLine is just for serialization
    public struct SpeedLineValueModel
    {
        public float Speed;
        public float StartTime;
        public WarningType WarningType;

        public SpeedLineValueModel(float speed, float startTime, WarningType warningType = WarningType.Default)
        {
            Speed = speed;
            StartTime = startTime;
            WarningType = warningType;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public struct Serializer
        {
            [JsonProperty("speed", Order = 0)]
            public float Speed;

            [JsonProperty("startTime", Order = 1)]
            public float StartTime;

            [JsonProperty("endTime", Order = 2)]
            public float EndTime;

            [JsonProperty("warningType", Order = 3, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
            [DefaultValue(WarningType.Default)]
            public WarningType WarningType;

            [JsonConstructor]
            public Serializer(float speed, float startTime, float endTime, WarningType warningType)
            {
                Debug.Assert(endTime > startTime);
                Speed = speed;
                StartTime = startTime;
                EndTime = endTime;
                WarningType = warningType;
            }
        }
    }
}