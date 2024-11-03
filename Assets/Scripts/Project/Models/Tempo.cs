#nullable enable

using Deenote.GameStage;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Project.Models
{
    public struct Tempo
    {
        /// <remarks>
        /// 0 or positive number.
        /// If 0, <see cref="GridController"/> should auto adjust bpm to make there's one beat for this tempo
        /// </remarks>
        public float Bpm;
        public float StartTime;

        public readonly float BeatInterval => Bpm == 0f ? float.MaxValue : 60f / Bpm;

        public Tempo(float bpm, float startTime)
        {
            Bpm = bpm;
            StartTime = startTime;
        }

        public readonly int GetBeatIndex(float time)
        {
            if (Bpm == 0f) {
                return time >= StartTime ? 0 : -1;
            }

            float timeOffsetToTempo = time - StartTime;
            float interval = 60 / Bpm;
            return Mathf.FloorToInt(timeOffsetToTempo / interval);
        }

        public readonly int GetCeilingBeatIndex(float time)
        {
            if (Bpm == 0f) {
                return time > StartTime ? 1 : 0;
            }

            float timeOffsetToTempo = time - StartTime;
            float interval = 60 / Bpm;
            return Mathf.CeilToInt(timeOffsetToTempo / interval);
        }

        public readonly float GetBeatTime(int beatIndex)
        {
            if (Bpm == 0f) {
                return beatIndex switch {
                    > 0 => float.MaxValue,
                    0 => StartTime,
                    < 0 => float.MinValue,
                };
            }

            return StartTime + beatIndex * 60 / Bpm;
        }

        public readonly float GetSubBeatTime(float beatIndex)
        {
            if (Bpm == 0f) {
                return beatIndex switch {
                    > 0f => float.MaxValue,
                    < 0f => float.MinValue,
                    _ => StartTime,
                };
            }

            return StartTime + beatIndex * 60f / Bpm;
        }
    }

    public static class TempoExt
    {
        public static int GetTempoIndex(this List<Tempo> tempos, float time)
        {
            int i;
            for (i = 0; i < tempos.Count; i++) {
                if (tempos[i].StartTime < time)
                    break;
            }
            return i - 1;
        }
    }
}