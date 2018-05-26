using System.IO;
using UnityEngine;
using NAudio.Wave;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }
    [SerializeField] private AudioSource _source;
    public delegate void AudioTimeChangeHandler(float time);
    public event AudioTimeChangeHandler AudioTimeChangeEvent = null;
    private float _time = 0.0f;
    public float Time
    {
        get { return _time; }
        set
        {
            _time = value;
            AudioTimeChangeEvent.Invoke(_time);
        }
    }
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
        _source.clip = clip;
        Time = 0;
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
        get { return _source.volume; }
        set { _source.volume = value; }
    }
    public bool IsPlaying => _source.isPlaying;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of AudioPlayer");
        }
        AudioTimeChangeEvent += ChangeAudioPlaybackPosition;
    }
    private void Update()
    {
        if (_source.isPlaying) Time = _source.time;
    }
}
