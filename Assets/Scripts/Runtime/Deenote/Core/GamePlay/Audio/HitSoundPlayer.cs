#nullable enable

using Deenote.Entities.Models;
using UnityEngine;

namespace Deenote.Core.GamePlay.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class HitSoundPlayer : MonoBehaviour
    {
        [SerializeField] AudioSource _source = default!;
        [SerializeField] HitSoundArgs _args = default!;

        public float Volume { get; internal set; }

        public void PlaySound(NoteModel.NoteKind kind)
        {
            if (Volume <= 0f)
                return;

            switch (kind) {
                case NoteModel.NoteKind.Slide:
                    _source.PlayOneShot(_args.SlideHitSoundClip, Volume * _args.SlideHitSoundBaseVolume);
                    break;
                case NoteModel.NoteKind.Click:
                case NoteModel.NoteKind.Swipe:
                    _source.PlayOneShot(_args.ClickHitSoundClip, Volume * _args.ClickHitSoundBaseVolume);
                    break;
            }
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