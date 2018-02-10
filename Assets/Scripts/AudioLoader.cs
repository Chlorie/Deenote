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
        if (audioType == ".wav")
            reader = new WaveFileReader(stream);
        else if (audioType == ".ogg")
            reader = new VorbisWaveReader(stream);
        else if (audioType == ".mp3")
            reader = new Mp3FileReader(stream);
        else
        {
            stream.Close();
            return null;
        }
        ISampleProvider provider = reader.ToSampleProvider();
        long length = reader.Length / (reader.WaveFormat.BitsPerSample / 8);
        float[] data = new float[length];
        provider.Read(data, 0, (int)length);
        stream.Close();
        AudioClip newClip = AudioClip.Create("SongAudioClip", data.Length / reader.WaveFormat.Channels, reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, false);
        newClip.SetData(data, 0);
        return newClip;
    }
}
