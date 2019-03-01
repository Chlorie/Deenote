using System.IO;
using UnityEngine;
using NAudio.Wave;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    [SerializeField] private AudioSource _source;
    public delegate void AudioTimeChangeHandler(float time);
    public delegate void AudioClipChangeHandler(float length);
    public event AudioTimeChangeHandler AudioTimeChangeEvent;
    public event AudioClipChangeHandler AudioClipChangeEvent;
    private float _time;
    public float Time
    {
        get => _time;
        set
        {
            _time = value;
            AudioTimeChangeEvent?.Invoke(_time);
        }
    }
    public float Length => _source.clip.length;
    public void LoadAudioFromStream(Stream stream)
    {
        WaveStream wave = new Mp3FileReader(stream);
        Mp3Frame frame = Mp3Frame.LoadFromStream(stream);
        int initialSampleCount = frame.SampleCount * wave.WaveFormat.Channels;
        stream.Seek(0, SeekOrigin.Begin);
        ISampleProvider provider = wave.ToSampleProvider();
        long length = wave.Length / (wave.WaveFormat.BitsPerSample / 8);
        float[] rawSamples = new float[length];
        provider.Read(rawSamples, 0, (int)length);
        float[] extendedSamples = new float[length + initialSampleCount];
        rawSamples.CopyTo(extendedSamples, initialSampleCount); // Insert empty frame at the beginning
        AudioClip clip = AudioClip.Create("AudioClip", extendedSamples.Length / wave.WaveFormat.Channels, wave.WaveFormat.Channels, wave.WaveFormat.SampleRate, false);
        clip.SetData(extendedSamples, 0);
        _source.clip = clip;
        Time = 0;
        AudioClipChangeEvent?.Invoke(clip.length);
    }
    private void ChangeAudioPlaybackPosition(float time) => _source.time = time;
    public void Play()
    {
        _source.Play();
        _source.time = _time;
    }
    public void Stop() => _source.Stop();
    public float Volume
    {
        get => _source.volume;
        set => _source.volume = value;
    }
    public float Pitch
    {
        get => _source.pitch;
        set => _source.pitch = value;
    }
    public bool IsPlaying => _source.isPlaying;
    public void TogglePlayState()
    {
        if (IsPlaying)
            Stop();
        else
            Play();
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
        AudioTimeChangeEvent += (time) => _time = time;
        AudioTimeChangeEvent += ChangeAudioPlaybackPosition;
    }
    private void Update()
    {
        if (_source.isPlaying) AudioTimeChangeEvent?.Invoke(_source.time);
    }
}
