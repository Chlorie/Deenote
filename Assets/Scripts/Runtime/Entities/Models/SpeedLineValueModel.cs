#nullable enable

using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;

namespace Deenote.Entities.Models
{
    public struct SpeedLineValueModel
    {
        public float Speed;
        public float StartTime;
        public WarningType WarningType;

        public SpeedLineValueModel(float speed, float startTime, WarningType warningType)
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