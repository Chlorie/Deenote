using System.Collections.Generic;
using System.Diagnostics;

public class Chart
{
    public float speed; // Speed value of the official charts
    public int difficulty; // Difficulty 0 Easy, 1 Normal, 2 Hard, 3 Extra
    public string level = ""; // Level of the chart
    public List<Note> notes = new List<Note>(); // All the notes info
    public static Chart FromJsonChart(JsonChart chart)
    {
        Chart result = new Chart { speed = chart.speed };
        if (chart.notes != null)
            foreach (JsonNote jnote in chart.notes)
            {
                Note note = new Note
                {
                    type = jnote.type,
                    position = jnote.pos,
                    size = jnote.size,
                    time = jnote._time,
                    shift = jnote.shift
                };
                if (jnote.sounds != null)
                    foreach (JsonPianoSound sound in jnote.sounds)
                        note.sounds.Add(PianoSound.FromJson(sound));
                result.notes.Add(note);
            }
        if (chart.links != null)
            foreach (JsonLink jlink in chart.links)
            {
                int previous = -1;
                if (jlink.notes != null)
                    foreach (JsonNote jnote in jlink.notes)
                    {
                        int current;
                        for (current = 0; current < chart.notes.Count; current++)
                            if (chart.notes[current] == jnote)
                                break;
                        result.notes[current].isLink = true;
                        if (previous != -1) result.notes[previous].next = current;
                        previous = current;
                    }
                if (previous != -1) result.notes[previous].next = -1;
            }
        return result;
    }
    public JsonChart ToJsonChart()
    {
        JsonChart result = new JsonChart { speed = speed };
        foreach (Note note in notes)
        {
            JsonNote jnote = new JsonNote
            {
                type = note.type,
                pos = note.position,
                size = note.size,
                _time = note.time,
                shift = note.shift,
                time = note.time
            };
            if (note.sounds.Count != 0)
            {
                jnote.sounds = new List<JsonPianoSound>();
                foreach (PianoSound sound in note.sounds)
                    jnote.sounds.Add(sound.ToJson());
            }
            result.notes.Add(jnote);
        }
        bool[] appeared = new bool[notes.Count];
        for (int i = 0; i < notes.Count; i++)
            if (notes[i].isLink && !appeared[i])
            {
                int current = i;
                JsonLink link = new JsonLink();
                while (current != -1)
                {
                    appeared[current] = true;
                    link.notes.Add(result.notes[current]);
                    current = notes[current].next;
                }
                result.links.Add(link);
            }
        return result;
    }
}
