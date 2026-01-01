#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Audio
{
    public sealed class PianoSoundSource : MonoBehaviour
    {
        public const int MaxPitch = 127;
        public const int MinPitch = 0;

        private static readonly int[] _pitches = { 24, 38, 43, 48, 53, 57, 60, 64, 67, 71, 74, 77, 81, 84, 89, 95 };
        private static readonly int[] _velocities = { 38, 63, 111, 127 };

        [SerializeField] private AudioSource _audioPlayerPrefab = null!;
        private ObjectPool<AudioSource> _audioPlayerPool = null!;

        [SerializeField] private AudioClip[] _soundClips = null!;
        [SerializeField] private bool _interpolateVelocity;

        private readonly bool[] _clearPitch = new bool[128];

        // Mirrored from Chlorie's
        public async UniTaskVoid PlaySoundAsync(int pitch, int velocity, float? duration, float delay, float speed, float volume = 1)
        {
            if (velocity == 0 || duration == 0)
                return;

            Play(pitch, velocity, duration, delay, volume, speed);
            return;

            var clip = GetSoundClip(pitch, velocity, out var speedMultiplier, out var playVolume);
            float playSpeed = speed * speedMultiplier;
            float playDelay_s = delay / speed;

            var player = _audioPlayerPool.Get();
            player.clip = clip;
            player.pitch = playSpeed;
            player.volume = playVolume;
            player.PlayDelayed(playDelay_s);

            // TODO: currently the duration parameter is not used, as if the pedal is held down
            // all the time, like what it's like in the original game.
            // Maybe consider some way to improve this if necessary.
            while (player.isPlaying) {
                await UniTask.NextFrame();
            }
            _audioPlayerPool.Release(player);
            // if (duration != null) {
            //     float playDuration = duration.Value * playSpeed;
            //     await UniTask.Delay(TimeSpan.FromSeconds(playDelay_s + playDuration));
            // 
            //     while (player.isPlaying && playVolume >= 0.01f) {
            //         playVolume = playVolume * Mathf.Pow(1e-6f, player.time - playDuration);
            //         player.volume = playVolume;
            //         await UniTask.NextFrame();
            //     }
            //     player.Stop();
            //     _audioPlayerPool.Release(player);
            // }
            // else {
            //     while (player.isPlaying) {
            //         await UniTask.NextFrame();
            //     }
            //     _audioPlayerPool.Release(player);
            // }
        }

        // Mirrored from Chlorie's
        private AudioClip GetSoundClip(int pitch, int velocity, out float speedMultiplier, out float playVelocity)
        {
            Debug.Assert(velocity != 0);

            int pitchDiff = MaxPitch;
            int nearestPitchIndex = 0;
            for (int i = 0; i < _pitches.Length; i++) {
                int diff = Mathf.Abs(pitch - _pitches[i]);
                if (diff < pitchDiff) {
                    pitchDiff = diff;
                    nearestPitchIndex = i;
                }
            }

            float velRatio = MaxPitch + 1f;
            int nearestVelIdx = 0;
            for (int i = 0; i < 4; i++) {
                int v = _velocities[i];
                float ratio = v > velocity ? (float)v / velocity : (float)velocity / v;
                if (ratio < velRatio) {
                    velRatio = ratio;
                    nearestVelIdx = i;
                }
            }

            int index = nearestPitchIndex * 4 + nearestVelIdx;
            speedMultiplier = Mathf.Pow(2f, (pitch - _pitches[nearestPitchIndex]) / 12f);
            playVelocity = (float)velocity / _velocities[nearestVelIdx];
            return _soundClips[index];
        }

        private void Play(int pitch, int velocity, float? duration, float delay, float volume, float speed)
        {
            velocity = Math.Min(velocity, 127);
            var (resultPitch, lowSound, highSound) = GetNearestSoundClip(pitch, velocity);
            Debug.Assert(highSound.Clip != null);

            float sourcePitch = Mathf.Pow(1.059463f, pitch - resultPitch);

            if (_interpolateVelocity) {
                var playerHigh = _audioPlayerPool.Get();
                playerHigh.clip = highSound.Clip;
                playerHigh.pitch = sourcePitch * speed;
                var velDiff = highSound.Velocity - lowSound.Velocity;
                playerHigh.volume = (float)(velocity - lowSound.Velocity) / velDiff * volume;

                PlayAudioSourceAsync(playerHigh, pitch, duration, delay, speed).Forget();

                if (lowSound.Clip is null)
                    return;

                var playerLow = _audioPlayerPool.Get();
                playerLow.clip = lowSound.Clip;
                playerLow.pitch = sourcePitch * speed;
                playerLow.volume = (float)(highSound.Velocity - velocity) / velDiff * volume;

                PlayAudioSourceAsync(playerLow, pitch, duration, delay, speed).Forget();
            }
            else {
                var player = _audioPlayerPool.Get();
                player.clip = highSound.Clip;
                player.pitch = sourcePitch * speed;
                player.volume = (float)velocity / highSound.Velocity * volume;

                PlayAudioSourceAsync(player, pitch, duration, delay, speed).Forget();
            }

            _clearPitch[pitch] = true;
        }

        private async UniTaskVoid PlayAudioSourceAsync(AudioSource source, int pitch, float? duration, float delay, float speed)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay / speed));
            source.enabled = true;
            source.Play();

            if (duration is null) {
                while (source.isPlaying) {
                    await UniTask.Yield();
                }
            }
            else {
                float timer = 0f;

                float fadeoutSpeed = source.volume * 0.015f;
                float clearSpeed = source.volume * 0.1f;
                float endVolume = source.volume * 0.01f;
                float actualDuration = duration.Value / speed;

                while (true) {
                    await UniTask.Yield();
                    timer += Time.deltaTime;
                    if (timer >= actualDuration) {
                        if (_clearPitch[pitch]) {
                            fadeoutSpeed = clearSpeed;
                        }
                        source.volume -= fadeoutSpeed;
                        if (source.volume < endVolume) {
                            break;
                        }
                    }
                }
                source.volume = 0f;
                source.Stop();
            }

            source.enabled = false;
            _audioPlayerPool.Release(source);
        }

        // Mirrored from decompiled
        private (int Pitch, (AudioClip? Clip, int Velocity) Low, (AudioClip Clip, int Velocity) High) GetNearestSoundClip(int pitch, int velocity)
        {
            var clips = GeNearestClipsByPitch(pitch, out var resultPitch);
            Debug.Assert(clips.Length == _velocities.Length);
            for (int i = 0; i < _velocities.Length; i++) {
                if (velocity <= _velocities[i]) {
                    if (i > 0) {
                        return (resultPitch, (clips[i - 1], _velocities[i - 1]), (clips[i], _velocities[i]));
                    }
                    else {
                        return (resultPitch, default, (clips[i], _velocities[i]));
                    }
                }
            }
            return (resultPitch, default, default);
        }

        private ReadOnlySpan<AudioClip> GeNearestClipsByPitch(int pitch, out int resultPitch)
        {
            var idx = _pitches.AsSpan().BinarySearch(pitch);
            if (idx >= 0) {
                resultPitch = _pitches[idx];
                return _soundClips.AsSpan(idx * 4, 4);
            }
            else {
                idx = ~idx;
                int resultIndex;
                if (idx == 0)
                    resultIndex = 0;
                else if (idx >= _pitches.Length)
                    resultIndex = _pitches.Length - 1;
                else if (_pitches[idx] - pitch < pitch - _pitches[idx - 1])
                    resultIndex = idx;
                else
                    resultIndex = idx - 1;
                resultPitch = _pitches[resultIndex];
                return _soundClips.AsSpan(resultIndex * 4, 4);
            }
        }

        private void Awake()
        {
            _audioPlayerPool = UnityUtils.CreateObjectPool(_audioPlayerPrefab, transform, defaultCapacity: 0);
        }

        private void LateUpdate()
        {
            Array.Fill(_clearPitch, false);
        }
    }
}