using System.Collections.Generic;

public class JSONChart
{
    public float speed = 0.0f; //It is in official charts, but no one knows what this means...
    public List<note> notes = new List<note>();
    public List<link> links = new List<link>();
    public class note
    {
        public int id = 0; //This is called "$id" in the officials charts... (Start with 1)
        public int type = 0; //Don't know what this is used for
        public List<sound> sounds = new List<sound>();
        public float pos = 0.0f;
        public float size = 1.0f;
        public float time = 0.0f; //This is called "_time" in the official charts, I don't wanna use variable names beginning with underline
        public float shift = 0.0f; //Someone please tell me... WT* is this...
    };
    public class sound
    {
        public float d = 0.0f; //duration
        public int p = 0; //pitch
        public int v = 0; //volume
        public float w = 0.0f; //wait
    }
    public class link
    {
        public List<int> noteRef = new List<int>(); //Called "$ref" in official charts and even ref is a keyword in C#...
    }
}
