#nullable enable

using Deenote.Core.Audio;
using System;
using UnityEngine;

namespace Deenote.Core.GamePlay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class GameMusicPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource _source = default!;

        private float _time;
        private IClipProvider? _factory;

        /// <summary>
        /// The current playback position in seconds.
        /// This value is only updated once per frame.
        /// </summary>
        public float Time
        {
            get => _time;
            set => SetTime(value, true);
        }

        public float ClipLength
        {
            get {
                if (_source.clip is { length: var len })
                    return len;
                Debug.LogWarning("GameMusicPlayer.Clip is null");
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
            get => _source.pitch;
            set => _source.pitch = value;
        }

        public bool IsPlaying => _source.isPlaying;

        /// <summary>
        /// Called on each frame when the playback position changes.
        /// </summary>
        public event Action<TimeChangedEventArgs>? TimeChanged;

        /// <summary>
        /// Called when the audio clip is changed.
        /// The argument is the length of the new clip in seconds.
        /// </summary>
        public event Action<AudioClip>? ClipChanged;

        private void SetTime(float value, bool isByJump)
        {
            if (Mathf.Approximately(value, _time)) {
                _time = value;
                return;
            }

            var oldValue = _time;
            _time = value;
            if (isByJump)
                _source.time = value;
            TimeChanged?.Invoke(new TimeChangedEventArgs(oldValue, value, isByJump));
        }

        /// <summary>
        /// Starts playing the audio clip.
        /// </summary>
        public void Play()
        {
            if (IsPlaying)
                return;
            _source.Play();
            _source.time = Time;
        }

        /// <summary>
        /// Pause the audio clip
        /// </summary>
        public void Stop()
        {
            if (!IsPlaying)
                return;
            _source.Stop();
        }

        public void TogglePlayingState()
        {
            if (IsPlaying)
                Stop();
            else
                Play();
        }

        /// <summary>
        /// Nudge playback position
        /// </summary>
        /// <param name="delta">in seconds</param>
        public void Nudge(float delta) => Time = Mathf.Clamp(0, Time + delta, ClipLength);

        public void ReplaceClip(IClipProvider provider)
        {
            (_factory as IDisposable)?.Dispose();
            _factory = provider;
            var clip = provider.Clip;
            _source.clip = clip;
            ClipChanged?.Invoke(clip);
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
            var source = _source;
            double current = AudioSettings.dspTime;
            double time = (double)source.timeSamples / source.clip.frequency;
            return current + (position - time) * source.pitch;
        }

        private void Update()
        {
            if (IsPlaying) {
                SetTime(_source.time, isByJump: false);
            }
        }

        private void OnDestroy()
        {
            (_factory as IDisposable)?.Dispose();
        }

        private void OnValidate()
        {
            _source ??= GetComponent<AudioSource>();
        }

        /// <summary>
        /// Args for when the playback position changes.
        /// </summary>
        /// <param name="OldTime">The old playback position in seconds.</param>
        /// <param name="NewTime">The new playback position in seconds.</param>
        /// <param name="IsByJump">
        /// Whether the playback position is manually changed, possibly due to user input.
        /// </param>
        public readonly record struct TimeChangedEventArgs(
            float OldTime,
            float NewTime,
            bool IsByJump);
    }
}