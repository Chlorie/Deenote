using Deenote.Project.Models.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.Project.Comparers
{
    public sealed class PianoSoundDataEqualityComparer : IEqualityComparer<PianoSoundData>
    {
        public static readonly PianoSoundDataEqualityComparer Instance = new();

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public bool Equals(PianoSoundData x, PianoSoundData y)
            => x.Pitch == y.Pitch
            && x.Duration == y.Duration
            && x.Delay == y.Delay
            && x.Velocity == y.Velocity;

        public int GetHashCode(PianoSoundData obj)
        {
            return HashCode.Combine(obj.Pitch, obj.Duration, obj.Delay, obj.Velocity);
        }
    }
}