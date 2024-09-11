using Deenote.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class MusicController : MonoBehaviour
    {
        /// <summary>
        /// Handler for when the playback position changes.
        /// </summary>
        /// <param name="oldTime">The old playback position in seconds.</param>
        /// <param name="newTime">The new playback position in seconds.</param>
        /// <param name="isManuallyChanged">
        /// Whether the playback position is manually changed, possibly due to user input.
        /// </param>
        public delegate void TimeChangedHandler(float oldTime, float newTime, bool isManuallyChanged);

        /// <summary>
        /// Called on each frame when the playback position changes.
        /// </summary>
        public event TimeChangedHandler? OnTimeChanged;

        /// <summary>
        /// Called when the audio clip is changed.
        /// The argument is the length of the new clip in seconds.
        /// </summary>
        public event Action<float>? OnClipChanged;

        /// <summary>
        /// The current playback position in seconds.
        /// This value is only updated once per frame.
        /// </summary>
        public float Time
        {
            get => _time;

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            set {
                if (_time == value) return;
                var old = _time;
                Source.time = _time = value;
                OnTimeChanged?.Invoke(old, value, isManuallyChanged: true);
            }
        }

        public float Length => Source.clip.length;
        public float Volume { get => Source.volume; set => Source.volume = value; }
        public float Pitch { get => Source.pitch; set => Source.pitch = value; }
        public bool IsPlaying => Source.isPlaying;

        /// <summary>
        /// Starts playing the audio clip.
        /// </summary>
        /// <param name="autoReset">
        /// If <see langword="true"/>, mark the current playback position,
        /// and reset the position to the mark when <see cref="Stop"/> is called later.
        /// </param>
        public void Play(bool autoReset = false)
        {
            if (IsPlaying) return;
            Source.Play();
            Source.time = _time;
            if (autoReset) _autoResetTime = _time;
        }

        public void Stop()
        {
            if (!IsPlaying) return;
            Source.Stop();
            if (_autoResetTime is not { } t) return;
            _autoResetTime = null;
            Time = t;
        }

        public void TogglePlayingState()
        {
            if (IsPlaying)
                Stop();
            else
                Play();
        }

        public void NudgePlaybackPosition(float seconds) => Time = Mathf.Clamp(0f, _time + seconds, Length);

        public void ReplaceClip(IClipProvider provider)
        {
            (_factory as IDisposable)?.Dispose();
            _factory = provider;
            var clip = provider.Clip;
            Source.clip = clip;
            OnClipChanged?.Invoke(clip.length);
        }

        /// <summary>
        /// If the music is playing and will still be playing at the same rate (pitch),
        /// get the DSP time for when the music would be at the given position.
        /// </summary>
        /// <remarks>
        /// This can be used to synchronize sound effects (note sounds or metronome beats) to the music
        /// more accurately than just using <see cref="Time"/> and <see cref="AudioSource.PlayDelayed"/>.
        /// </remarks>
        /// <param name="position">The target playback position in seconds.</param>
        /// <returns>The expected DSP time.</returns>
        public double DspTimeAtPosition(float position)
        {
            var source = Source;
            double current = AudioSettings.dspTime;
            double time = (double)source.timeSamples / source.clip.frequency;
            return current + (position - time) * source.pitch;
        }

        private AudioSource? _source;
        private AudioSource Source => this.MaybeGetComponent(ref _source);
        private float? _autoResetTime;
        private float _time;
        private IClipProvider? _factory;

        private void Update()
        {
            if (!IsPlaying) return;

            float oldTime = _time;
            _time = Source.time;
            OnTimeChanged?.Invoke(oldTime,_time, isManuallyChanged: false);
        }

        private void OnDestroy() => (_factory as IDisposable)?.Dispose();
    }
}