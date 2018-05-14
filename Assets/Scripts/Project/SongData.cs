using System.Collections.Generic;

public class SongData
{
    public string songName = "";
    public string artist = "";
    public string noter = "";
    public Chart[] charts = new Chart[4];
    public int coverHeight = 0;
    public int coverXOffset = 0;
    public List<TempoEvent> tempos = new List<TempoEvent>(); // All the tempo events
    public byte[] music; // In .mp3 format
    public byte[] preview; // In .mp3 format
    public byte[] cover; // In .png format
    public byte[] coverFC; // In .png format
    public SongData() { for (int i = 0; i < 4; i++) charts[i] = new Chart(); }
}
