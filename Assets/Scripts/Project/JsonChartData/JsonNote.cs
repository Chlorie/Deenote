// ReSharper disable InconsistentNaming, NotAccessedField.Global
using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(IsReference = true)]
public class JsonNote
{
    public int type;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<JsonPianoSound> sounds;
    public float pos;
    public float size;
    public float _time;
    public float shift;
    public float time;
}
