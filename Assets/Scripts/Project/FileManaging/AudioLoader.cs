using UnityEngine;
using System.IO;
using NAudio.Wave;
using NAudio.Vorbis;

public class AudioLoader
{
    public static AudioClip LoadFromBuffer(byte[] buffer, string audioType)
    {
        return LoadFromStream(new MemoryStream(buffer), audioType);
    }
    public static AudioClip LoadFromFile(string fileName) // Not used currently
    {
        return LoadFromStream(new FileStream(fileName, FileMode.Open), Path.GetExtension(fileName));
    }
    private static AudioClip LoadFromStream(Stream stream, string audioType)
    {
        WaveStream reader;
        int initialSampleCount = 0;
        if (audioType == ".wav")
            reader = new WaveFileReader(stream);
        else if (audioType == ".ogg")
            reader = new VorbisWaveReader(stream);
        else if (audioType == ".mp3")
        {
            reader = new Mp3FileReader(stream);
            Mp3Frame frame = Mp3Frame.LoadFromStream(stream);
            initialSampleCount = frame.SampleCount * reader.WaveFormat.Channels;
            stream.Seek(0, SeekOrigin.Begin);
        }
        else
        {
            stream.Close();
            return null;
        }
        ISampleProvider provider = reader.ToSampleProvider();
        long length = reader.Length / (reader.WaveFormat.BitsPerSample / 8);
        float[] raw = new float[length];
        provider.Read(raw, 0, (int)length);
        float[] data = new float[length + initialSampleCount];
        raw.CopyTo(data, initialSampleCount);
        stream.Close();
        AudioClip newClip = AudioClip.Create("SongAudioClip", data.Length / reader.WaveFormat.Channels, reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, false);
        newClip.SetData(data, 0);
        return newClip;
    }
}
