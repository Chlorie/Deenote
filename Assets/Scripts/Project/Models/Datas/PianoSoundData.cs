#nullable enable

using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Deenote.Project.Models.Datas
{
    [Serializable]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class PianoSoundData
    {
        [SerializeField]
        [JsonProperty("w")]
        public float Delay;
        [SerializeField]
        [JsonProperty("d")]
        public float Duration;
        [SerializeField]
        [JsonProperty("p")]
        public int Pitch;
        [SerializeField]
        [JsonProperty("v")]
        public int Velocity;

        [JsonConstructor]
        public PianoSoundData(float delay, float duration, int pitch, int velocity)
        {
            Delay = delay;
            Duration = duration;
            Pitch = pitch;
            Velocity = velocity;
        }

        public PianoSoundData(PianoSoundValueData data)
            : this(data.Delay, data.Duration, data.Pitch, data.Velocity)
        {
        }

        public PianoSoundData Clone()
            => new(Delay, Duration, Pitch, Velocity);

        public void CopyTo(PianoSoundData other)
        {
            other.Delay = Delay;
            other.Duration = Duration;
            other.Pitch = Pitch;
            other.Velocity = Velocity;
        }

        public PianoSoundValueData GetValues()
        {
            return new(Delay, Duration, Pitch, Velocity);
        }

        public void SetValues(in PianoSoundValueData values)
        {
            Delay = values.Delay;
            Duration = values.Duration;
            Pitch = values.Pitch;
            Velocity = values.Velocity;
        }
    }

    public struct PianoSoundValueData
    {
        public float Delay;
        public float Duration;
        public int Pitch;
        public int Velocity;

        public PianoSoundValueData(float delay, float duration, int pitch, int velocity)
        {
            Delay = delay;
            Duration = duration;
            Pitch = pitch;
            Velocity = velocity;
        }

        public override readonly bool Equals(object obj)
            => obj is PianoSoundValueData data && data == this;
        public override readonly int GetHashCode()
            => HashCode.Combine(Delay, Duration, Pitch, Velocity);

        public static bool operator ==(PianoSoundValueData left, PianoSoundValueData right)
            => left.Delay == right.Delay && left.Duration == right.Duration
            && left.Pitch == right.Pitch && left.Velocity == right.Velocity;
        public static bool operator !=(PianoSoundValueData left, PianoSoundValueData right)
            => !(left == right);
    }
}