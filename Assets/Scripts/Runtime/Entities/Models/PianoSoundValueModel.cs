#nullable enable

using Newtonsoft.Json;
using System;

namespace Deenote.Entities.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct PianoSoundValueModel
    {
        private static readonly string[] _pianoNoteNames = new[] {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };

        [JsonProperty("w", Order = 0)] public float Delay;
        [JsonProperty("d", Order = 1)] public float Duration;
        [JsonProperty("p", Order = 2)] public int Pitch;
        [JsonProperty("v", Order = 3)] public int Velocity;

        [JsonConstructor]
        public PianoSoundValueModel(float delay, float duration, int pitch, int velocity)
        {
            Delay = delay;
            Duration = duration;
            Pitch = pitch;
            Velocity = velocity;
        }

        public readonly string ToPitchDisplayString()
        {
            int octave = Math.DivRem(Pitch, 12, out var rem) - 2;
            return $"{_pianoNoteNames[rem]}{octave}";
        }

        #region Equality

        public override readonly bool Equals(object obj)
            => obj is PianoSoundValueModel data && data == this;
        public override readonly int GetHashCode()
            => HashCode.Combine(Delay, Duration, Pitch, Velocity);

        public static bool operator ==(PianoSoundValueModel left, PianoSoundValueModel right)
            => left.Delay == right.Delay && left.Duration == right.Duration
            && left.Pitch == right.Pitch && left.Velocity == right.Velocity;
        public static bool operator !=(PianoSoundValueModel left, PianoSoundValueModel right)
            => !(left == right);

        #endregion
    }
}