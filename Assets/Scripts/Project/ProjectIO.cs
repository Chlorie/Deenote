using System;
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
    public static PianoSound ReadPianoSound(this BinaryReader reader) => new PianoSound
    {
        delay = reader.ReadSingle(),
        duration = reader.ReadSingle(),
        pitch = reader.ReadInt16(),
        volume = reader.ReadInt16()
    };
    public static TempoEvent ReadTempoEvent(this BinaryReader reader) => new TempoEvent
    {
        time = reader.ReadSingle(),
        tempo = reader.ReadSingle()
    };

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
    public static void Write(this BinaryWriter writer, TempoEvent tempo)
    {
        writer.Write(tempo.time);
        writer.Write(tempo.tempo);
    }

    public static SongData ReadSongData(this BinaryReader reader)
    {
        int version = reader.ReadInt32();
        if (version != 1) throw new Exception("Undefined version of \".dnt\" file.");
        SongData data = new SongData
        {
            songName = reader.ReadString(),
            artist = reader.ReadString(),
            noter = reader.ReadString()
        };
        int chartArrayLength = reader.ReadInt32();
        data.charts = new Chart[chartArrayLength];
        for (int i = 0; i < chartArrayLength; i++) data.charts[i] = reader.ReadChart();
        data.coverHeight = reader.ReadInt32();
        data.coverXOffset = reader.ReadInt32();
        int tempoEventArrayLength = reader.ReadInt32();
        data.tempos = new List<TempoEvent>();
        for (int i = 0; i < tempoEventArrayLength; i++) data.tempos.Add(reader.ReadTempoEvent());
        data.music = reader.ReadByteArray();
        data.preview = reader.ReadByteArray();
        data.cover = reader.ReadByteArray();
        data.coverFC = reader.ReadByteArray();
        return data;
    }
    public static void Write(this BinaryWriter writer, SongData data)
    {
        writer.Write(1); // Current version of data file, 1
        writer.Write(data.songName);
        writer.Write(data.artist);
        writer.Write(data.noter);
        writer.Write(data.charts.Length);
        foreach (Chart chart in data.charts) writer.Write(chart);
        writer.Write(data.coverHeight);
        writer.Write(data.coverXOffset);
        writer.Write(data.tempos.Count);
        foreach (TempoEvent tempo in data.tempos) writer.Write(tempo);
        writer.WriteArray(data.music);
        writer.WriteArray(data.preview);
        writer.WriteArray(data.cover);
        writer.WriteArray(data.coverFC);
    }
}
