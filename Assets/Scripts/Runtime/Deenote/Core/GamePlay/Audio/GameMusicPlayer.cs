#nullable enable

using Deenote.Core.Audio;
using System;
using UnityEngine;

namespace Deenote.Core.GamePlay.Audio
{
    public enum TransportChangeReason
    {
        Started,
        Paused,
        Seeked,
        RateChanged,
    }

    [RequireComponent(typeof(AudioSource))]
    public sealed class GameMusicPlayer : MonoBehaviour
    {
        private const double MinimumStartLeadSeconds = 0.02d;
        private const double StartLeadSafetySeconds = 0.005d;

        [SerializeField] private AudioSource _source = default!;

        private readonly AudioTransport _transport = new();
        private readonly RenderClock _renderClock = new();

        private IClipProvider? _clipProvider;
        private float _time;
        private double _rate = 1d;
        private bool _warnedMissingClip;

        /// <summary>
        /// The chart time used by rendering. It is phase-locked to the audio
        /// transport and is intentionally updated once per frame.
        /// </summary>
        public float Time
        {
            get => _time;
            set => SetTime(value);
        }

        /// <summary>
        /// The exact DSP-derived chart time used by audio scheduling. Do not use
        /// this value for rendering.
        /// </summary>
        internal double PreciseTime
        {
            get {
                double time = _transport.ChartTimeAtDsp(AudioSettings.dspTime);
                return _source.clip is { length: var length }
                    ? Math.Clamp(time, 0d, length)
                    : time;
            }
        }

        internal AudioTransport Transport => _transport;

        public float ClipLength
        {
            get {
                if (_source.clip is { length: var len })
                    return len;
                if (!_warnedMissingClip) {
                    Debug.LogWarning("GameMusicPlayer.Clip is null");
                    _warnedMissingClip = true;
                }
                return 0f;
            }
        }

        public float Volume
        {
            get => _source.volume;
            set => _source.volume = value;
        }

        public float Pitch
        {
            get => (float)_rate;
            set {
                value = Mathf.Max(0.01f, value);
                if (Mathf.Approximately(value, (float)_rate))
                    return;

                _rate = value;
                _source.pitch = value;

                if (_transport.State == AudioTransport.TransportState.Playing) {
                    double dspTime = AudioSettings.dspTime;
                    double chartTime = Math.Clamp(
                        _transport.ChartTimeAtDsp(dspTime), 0d, ClipLength);
                    _transport.Reanchor(dspTime, chartTime, value);
                }
                else {
                    _transport.SetRate(value);
                }

                TransportChanged?.Invoke(TransportChangeReason.RateChanged);
            }
        }

        public bool IsPlaying => _transport.IsRunning;

        /// <summary>
        /// Called on each render frame when the render clock changes.
        /// </summary>
        public event Action<TimeChangedEventArgs>? TimeChanged;

        /// <summary>
        /// Called when playback state, rate, or position changes. Audio
        /// scheduling uses this to immediately rebase its lookahead window.
        /// </summary>
        public event Action<TransportChangeReason>? TransportChanged;

        /// <summary>
        /// Called when the audio clip is changed.
        /// </summary>
        public event Action<AudioClip>? ClipChanged;

        private void Awake()
        {
            _transport.Seek(0d);
        }

        private void Update()
        {
            if (_source.clip == null)
                return;

            double dspTime = AudioSettings.dspTime;
            _transport.MarkStartedIfDue(dspTime);

            if (_transport.IsAdvancingAt(dspTime) &&
                dspTime >= _transport.DspTimeAtChart(ClipLength)) {
                Stop();
                return;
            }

            double targetTime = Math.Clamp(
                _transport.ChartTimeAtDsp(dspTime), 0d, ClipLength);
            double renderTime = _renderClock.Update(
                UnityEngine.Time.unscaledDeltaTime,
                targetTime,
                _rate,
                _transport.IsAdvancingAt(dspTime),
                _transport.Epoch);

            float newTime = (float)Math.Clamp(renderTime, 0d, ClipLength);
            if (Mathf.Approximately(newTime, _time))
                return;

            float oldTime = _time;
            _time = newTime;
            TimeChanged?.Invoke(new TimeChangedEventArgs(oldTime, newTime, false));
        }

        /// <summary>
        /// Starts playback at a future DSP time. The first visual frame remains
        /// at the chart anchor until that DSP time is reached.
        /// </summary>
        public void Play()
        {
            if (_transport.IsRunning || _source.clip == null)
                return;

            double dspTime = AudioSettings.dspTime;
            double chartTime = Math.Clamp(
                _transport.ChartTimeAtDsp(dspTime), 0d, ClipLength);
            double startDsp = dspTime + GetStartLeadSeconds();

            _source.time = (float)chartTime;
            _source.pitch = (float)_rate;
            _transport.Start(startDsp, chartTime, _rate);
            _source.PlayScheduled(startDsp);
            TransportChanged?.Invoke(TransportChangeReason.Started);
        }

        /// <summary>
        /// Pauses at the exact transport time. Resuming creates a new future
        /// DSP anchor while preserving the chart time.
        /// </summary>
        public void Stop()
        {
            if (!_transport.IsRunning)
                return;

            double chartTime = Math.Clamp(
                _transport.ChartTimeAtDsp(AudioSettings.dspTime), 0d, ClipLength);

            float oldTime = _time;
            float newTime = (float)chartTime;

            _source.Stop();
            _transport.Pause(chartTime);
            SetRenderedTime(newTime);
            if (!Mathf.Approximately(oldTime, newTime))
                TimeChanged?.Invoke(new TimeChangedEventArgs(oldTime, newTime, false));
            TransportChanged?.Invoke(TransportChangeReason.Paused);
        }

        public void TogglePlayingState()
        {
            if (IsPlaying)
                Stop();
            else
                Play();
        }

        public void Nudge(float delta) => SetTime(Time + delta);

        /// <summary>
        /// Seeks the transport. While playing, the music source is kept alive and
        /// re-anchored; while paused, only the paused chart time is changed.
        /// </summary>
        public void SetTime(float value)
        {
            value = Mathf.Clamp(value, 0f, ClipLength);

            float oldTime = _time;
            bool wasRunning = _transport.IsRunning;

            if (_transport.State == AudioTransport.TransportState.Playing) {
                _source.time = value;
                _transport.Reanchor(AudioSettings.dspTime, value, _rate);
            }
            else {
                if (wasRunning)
                    _source.Stop();

                _source.time = value;
                _transport.Seek(value);

                if (wasRunning)
                    Play();
            }

            SetRenderedTime(value);
            if (!Mathf.Approximately(oldTime, value)) {
                TimeChanged?.Invoke(new TimeChangedEventArgs(oldTime, value, true));
            }
            TransportChanged?.Invoke(TransportChangeReason.Seeked);
        }

        public void ReplaceClip(IClipProvider provider)
        {
            (_clipProvider as IDisposable)?.Dispose();
            _clipProvider = provider;

            var clip = provider.Clip;
            _source.clip = clip;
            _warnedMissingClip = false;
            ClipChanged?.Invoke(clip);
        }

        /// <summary>
        /// Returns the DSP time corresponding to a chart position. This is the
        /// primitive that should be used to schedule audio effects.
        /// </summary>
        public double DspTimeAtPosition(float position)
        {
            if (_transport.CanSchedule)
                return _transport.DspTimeAtChart(position);

            return AudioSettings.dspTime +
                   (position - PreciseTime) / Math.Max(_rate, 0.001d);
        }

        private void SetRenderedTime(float value)
        {
            _time = Mathf.Clamp(value, 0f, ClipLength);
        }

        private static double GetStartLeadSeconds()
        {
            AudioSettings.GetDSPBufferSize(out int bufferLength, out _);
            int sampleRate = AudioSettings.outputSampleRate;
            if (sampleRate <= 0)
                return MinimumStartLeadSeconds;

            double bufferSeconds = (double)bufferLength / sampleRate;

            // The transport change notification schedules the first audio
            // events synchronously, so one audio buffer plus a small safety
            // margin is enough. Waiting for two buffers made the play button
            // feel delayed.
            return Math.Max(
                MinimumStartLeadSeconds,
                bufferSeconds + StartLeadSafetySeconds);
        }

        private void OnDestroy()
        {
            (_clipProvider as IDisposable)?.Dispose();
        }

        private void OnValidate()
        {
            _source ??= GetComponent<AudioSource>();
        }

        public readonly record struct TimeChangedEventArgs(
            float OldTime,
            float NewTime,
            bool IsByJump);
    }
}
