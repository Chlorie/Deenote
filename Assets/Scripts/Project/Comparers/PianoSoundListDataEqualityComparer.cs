using Deenote.Project.Models.Datas;
using System;
using System.Collections.Generic;

namespace Deenote.Project.Comparers
{
    public sealed class PianoSoundListDataEqualityComparer : IEqualityComparer<List<PianoSoundData>>
    {
        public static readonly PianoSoundListDataEqualityComparer Instance = new();

        public bool Equals(List<PianoSoundData> x, List<PianoSoundData> y)
        {
            if (x.Count != y.Count) return false;

            for (int i = 0; i < x.Count; i++) {
                var l = x[i];
                var r = y[i];
                if (!PianoSoundDataEqualityComparer.Instance.Equals(l, r))
                    return false;
            }
            return true;
        }

        public int GetHashCode(List<PianoSoundData> obj)
        {
            var hashcode = new HashCode();
            foreach (var item in obj) {
                hashcode.Add(item);
            }
            return hashcode.ToHashCode();
        }
    }
}