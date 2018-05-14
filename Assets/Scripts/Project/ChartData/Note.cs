using System.Collections.Generic;

public class Note
{
    public float position = 1.0f; // pos
    public float size = 0.0f; // size
    public float time = 0.0f; // _time or time
    public float shift = 0.0f; // shift
    public int type = 0; // type
    public List<PianoSound> sounds = new List<PianoSound>(); // sounds
    public bool isLink = false; // Whether the note is a link note
    public int next = -1; // Index of next link note in the same link, -1 means current note is the last
}
