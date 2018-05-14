using System.IO;
using UnityEngine;
using NAudio.Wave;
//using NAudio.Vorbis;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    public AudioSource source;
    public void LoadAudioFromStream(Stream stream)
    {
        WaveStream wave;
        int initialSampleCount = 0;
        wave = new Mp3FileReader(stream);
        Mp3Frame frame = Mp3Frame.LoadFromStream(stream);
        initialSampleCount = frame.SampleCount * wave.WaveFormat.Channels;
        stream.Seek(0, SeekOrigin.Begin);
        ISampleProvider provider = wave.ToSampleProvider();
        long length = wave.Length / (wave.WaveFormat.BitsPerSample / 8);
        float[] rawSamples = new float[length];
        provider.Read(rawSamples, 0, (int)length);
        float[] extendedSamples = new float[length + initialSampleCount];
        rawSamples.CopyTo(extendedSamples, initialSampleCount);
        AudioClip clip = AudioClip.Create("AudioClip", extendedSamples.Length / wave.WaveFormat.Channels, wave.WaveFormat.Channels, wave.WaveFormat.SampleRate, false);
        clip.SetData(extendedSamples, 0);
        source.clip = clip;
        source.Play();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of AudioPlayer");
        }
    }
}
