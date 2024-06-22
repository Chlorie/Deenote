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

        public PianoSoundData Clone() 
            => new(Delay, Duration, Pitch, Velocity);

        private static readonly string[] _pianoNoteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        public string ToPitchDisplayString()
        {
            int octave = Pitch / 12 - 2;
            return $"{_pianoNoteNames[Pitch % 12]}{octave}";
        }
    }
}
