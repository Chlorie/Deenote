#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    public sealed class PianoSoundManager : MonoBehaviour
    {
        public const int MaxPitch = 127;
        public const int MinPitch = 0;

        private static readonly int[] _pitches = { 24, 38, 43, 48, 53, 57, 60, 64, 67, 71, 74, 77, 81, 84, 89, 95 };
        private static readonly int[] _velocities = { 38, 63, 111, 127 };

        [SerializeField] private AudioSource _audioPlayerPrefab = null!;
        [SerializeField] private Transform _audioPlayerParentTransform = null!;
        private ObjectPool<AudioSource> _audioPlayerPool = null!;

        [SerializeField] private AudioClip[] _soundClips = null!;

        // Mirrored from Chlorie's
        public async UniTaskVoid PlaySoundAsync(int pitch, int velocity, float? duration, float delay, float speed)
        {
            if (velocity == 0 || duration == 0)
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

        // TODO: split playback volume and velocity
        public UniTaskVoid PlaySoundAsync(PianoSoundValueData sound, float volume, float speed)
            => PlaySoundAsync(sound.Pitch, (int)(sound.Velocity * volume), sound.Duration, sound.Delay, speed);

        public UniTaskVoid PlaySoundAsync(PianoSoundData sound, float volume, float speed)
            => PlaySoundAsync(sound.GetValues(), volume, speed);


        public UniTaskVoid PlaySoundsAsync(ReadOnlySpan<PianoSoundData> sounds, float volume, float speed)
        {
            foreach (var sound in sounds) {
                PlaySoundAsync(sound, volume, speed).Forget();
            }
            return default;
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

        private void Awake()
        {
            _audioPlayerPool = UnityUtils.CreateObjectPool(_audioPlayerPrefab, _audioPlayerParentTransform, defaultCapacity: 0);
        }
    }
}