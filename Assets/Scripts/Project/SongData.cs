using System.Collections.Generic;

[System.Serializable]
public class SongData
{
    public string songName;
    public string artist;
    public string noter;
    public Chart[] charts;
    public int coverHeight;
    public int coverXOffset = 0;
    public float musicVolume = 1.0f;
    public float pianoVolume = 1.0f;
    public List<TempoEvent> tempos = new List<TempoEvent>(); // All the tempo events
    public byte[] music; // In .mp3 format
    public byte[] preview; // In .mp3 format
    public byte[] cover; // In .png format
    public byte[] coverFC; // In .png format
}
