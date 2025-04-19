#nullable enable

namespace Deenote.Entities
{
    public struct TempoRange
    {
        public Tempo Tempo;
        public float EndTime;

        public readonly float Length => EndTime - Tempo.StartTime;

        public TempoRange(Tempo tempo, float endTime)
        {
            Tempo = tempo;
            EndTime = endTime;
        }

        public TempoRange(float bpm, float startTime, float endTime)
        {
            Tempo = new(bpm, startTime);
            EndTime = endTime;
        }

        public readonly void Deconstruct(out Tempo tempo, out float endTime)
        {
            tempo = Tempo;
            endTime = EndTime;
        }
    }
}