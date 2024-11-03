#nullable enable

using Newtonsoft.Json;
using System.ComponentModel;

namespace Deenote.Project.Models.Datas
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SpeedLine
    {
        [JsonProperty("speed")]
        public float Speed;

        [JsonProperty("startTime")]
        public float StartTime;

        [JsonProperty("endTime")]
        public float EndTime;

        [JsonProperty("warningType")]
        [DefaultValue(WarningType.Default)]
        public WarningType WarningType;

        [JsonConstructor]
        public SpeedLine(float speed, float startTime, float endTime, WarningType warningType)
        {
            Speed = speed;
            StartTime = startTime;
            EndTime = endTime;
            WarningType = warningType;
        }

        public SpeedLine Clone()
        {
            return new(Speed, StartTime, EndTime, WarningType);
        }
    }
}