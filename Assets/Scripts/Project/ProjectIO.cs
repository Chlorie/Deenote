using System.IO;
using System.Collections.Generic;

public static class ProjectIO
{
    public static Chart ReadChart(this BinaryReader reader)
    {
        Chart chart = new Chart
        {
            speed = reader.ReadSingle(),
            difficulty = reader.ReadInt32(),
            level = reader.ReadString()
        };
        int noteCount = reader.ReadInt32();
        chart.notes = new List<Note>();
        for (int i = 0; i < noteCount; i++) chart.notes.Add(reader.ReadNote());
        return chart;
    }
    public static Note ReadNote(this BinaryReader reader)
    {
        Note note = new Note
        {
            position = reader.ReadSingle(),
            size = reader.ReadSingle(),
            time = reader.ReadSingle(),
            shift = reader.ReadSingle()
        };
        int soundCount = reader.ReadInt32();
        note.sounds = new List<PianoSound>();
        for (int i = 0; i < soundCount; i++) note.sounds.Add(reader.ReadPianoSound());
        note.isLink = reader.ReadBoolean();
        note.next = reader.ReadInt32();
        return note;
    }
    public static PianoSound ReadPianoSound(this BinaryReader reader)
    {
        PianoSound sound = new PianoSound
        {
            delay = reader.ReadSingle(),
            duration = reader.ReadSingle(),
            pitch = reader.ReadInt16(),
            volume = reader.ReadInt16()
        };
        return sound;
    }

    public static void Write(this BinaryWriter writer, Chart chart)
    {
        writer.Write(chart.speed);
        writer.Write(chart.difficulty);
        writer.Write(chart.level);
        writer.Write(chart.notes.Count);
        foreach (Note note in chart.notes) writer.Write(note);
    }
    public static void Write(this BinaryWriter writer, Note note)
    {
        writer.Write(note.position);
        writer.Write(note.size);
        writer.Write(note.time);
        writer.Write(note.shift);
        writer.Write(note.type);
        writer.Write(note.sounds.Count);
        foreach (PianoSound sound in note.sounds) writer.Write(sound);
        writer.Write(note.isLink);
        writer.Write(note.next);
    }
    public static void Write(this BinaryWriter writer, PianoSound sound)
    {
        writer.Write(sound.delay);
        writer.Write(sound.duration);
        writer.Write(sound.pitch);
        writer.Write(sound.volume);
    }
}
