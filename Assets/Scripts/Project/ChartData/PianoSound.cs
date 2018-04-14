public class PianoSound
{
    public float delay = 0; // w
    public float duration = 0.0f; // d
    public short pitch = 0; // p
    public short volume = 0; // v
    public JsonPianoSound ToJson() => new JsonPianoSound { w = delay, d = duration, p = pitch, v = volume };
    public static PianoSound FromJson(JsonPianoSound sound) => new PianoSound
    {
        delay = sound.w,
        duration = sound.d,
        pitch = sound.p,
        volume = sound.v
    };
}
