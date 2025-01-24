#nullable enable

using UnityEngine;

namespace Deenote.GamePlay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class HitSoundPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource _source = default!;
        [SerializeField] HitSoundArgs _args = default!;

        public float Volume { get; internal set; }

        public void PlayClickSound()
        {
            if (Volume <= 0f)
                return;

            _source.PlayOneShot(_args.ClickHitSoundClip, Volume * _args.ClickHitSoundBaseVolume);
        }

        public void PlaySlideSound()
        {
            if (Volume <= 0f)
                return;
            _source.PlayOneShot(_args.SlideHitSoundClip, Volume * _args.SlideHitSoundBaseVolume);
        }

        private void OnValidate()
        {
            _source ??= GetComponent<AudioSource>();
        }

        public enum SoundKind
        {
            ClickSound,
            SlideSound,
        }
    }
}