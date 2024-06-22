using UnityEngine;

namespace Deenote.Project.Models
{
    public struct Tempo
    {
        public float Bpm;
        public float StartTime;

        public Tempo(float bpm,float startTime)
        {
            Bpm = bpm;
            StartTime = startTime;
        }

        public readonly int GetBeatIndex(float time)
        {
            float timeOffsetToTempo = time - StartTime;
            float interval = 60 / Bpm;
            return Mathf.FloorToInt(timeOffsetToTempo / interval);
        }

        public readonly float GetBeatTime(int beatIndex)
        {
            return StartTime + beatIndex * 60 / Bpm;
        }
    }
}