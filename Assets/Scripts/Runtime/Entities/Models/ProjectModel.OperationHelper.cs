#nullable enable

using UnityEngine;

namespace Deenote.Entities.Models
{
    partial class ProjectModel
    {
        /// <returns>
        /// if <paramref name="index"/> is out of range,
        /// returns 0 or the audio's length
        /// </returns>
        public float GetNonOverflowTempoTime(int index)
        {
            if (index >= _tempos.Count)
                return AudioClip.length;
            if (index < 0)
                return 0f;
            return _tempos[index].StartTime;
        }

        /// <returns>
        /// if <paramref name="index"/> less than 0, return a <see cref="Tempo"/>
        /// with 0 bpm and 0 start time
        /// </returns>
        public Tempo GetActualTempo(int index)
        {
            if (index < 0)
                return new Tempo(0f, 0f);

            return _tempos[index];
        }

        /// <returns>Range: [-1, Count)</returns>
        public int GetTempoIndex(float time)
        {
            int i = 0;
            for (; i < _tempos.Count; i++) {
                if (time < _tempos[i].StartTime)
                    break;
            }
            return i - 1;
        }

        /// <returns>Range: [0, Count]</returns>
        public int GetCeilingTempoIndex(float time)
        {
            int i = 0;
            for (; i < _tempos.Count; i++) {
                if (time <= _tempos[i].StartTime)
                    break;
            }
            return i;
        }
    }
}