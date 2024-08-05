using NAudio.Wave;
using System.IO;
using UnityEngine;

namespace Deenote.Utilities
{
    // Copied from Chlorie's version
    public static class AudioUtils
    {
        public static bool TryLoad(Stream stream, string audioType, out AudioClip? clip)
        {
            WaveStream reader;
            int initialSampleCount;
            switch (audioType)
            {
                case ".wav":
                    reader = new WaveFileReader(stream);
                    initialSampleCount = 0;
                    break;
                case ".mp3":
                    reader = new Mp3FileReader(stream);
                    Mp3Frame frame = Mp3Frame.LoadFromStream(stream);
                    initialSampleCount = frame.SampleCount * reader.WaveFormat.Channels;
                    stream.Seek(0, SeekOrigin.Begin);
                    break;
                default:
                    clip = null;
                    return false;
            }

            ISampleProvider provider = reader.ToSampleProvider();
            long length = reader.Length / (reader.WaveFormat.BitsPerSample / 8);
            float[] raw = new float[length];
            try
            {
                provider.Read(raw, 0, (int)length);
            }
            catch (NAudio.MmException)
            {
                clip = null;
                return false;
            }
            float[] data = new float[length + initialSampleCount];
            raw.CopyTo(data, initialSampleCount);
            AudioClip newClip = AudioClip.Create("SongAudioClip", data.Length / reader.WaveFormat.Channels, reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, false);
            newClip.SetData(data, 0);
            clip = newClip;
            return true;
        }
    }
}