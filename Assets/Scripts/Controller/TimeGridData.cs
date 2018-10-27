public struct TimeGridData
{
    public enum Type
    {
        SubBeat,
        Beat,
        TempoChange,
        FreeTempo
    }

    public float time;
    public Type type;
}
