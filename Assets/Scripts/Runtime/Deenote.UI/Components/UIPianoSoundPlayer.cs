#nullable enable

using Deenote.Audio;
using Deenote.Entities.Models;
using System;

namespace Deenote.UI.Views
{
    public sealed class UIPianoSoundPlayer
    {
        private readonly PianoSoundSource _source;

        public UIPianoSoundPlayer(PianoSoundSource source)
        {
            _source = source;
        }

        public void PlaySound(int pitch)
            => _source.PlaySoundAsync(pitch, 95, null, 0f, 1f).Forget();

        public void PlaySound(PianoSoundValueModel sound)
            => _ = _source.PlaySoundAsync(sound.Pitch, (int)(sound.Velocity * 1f), sound.Duration, sound.Delay, 1f);

        public void PlaySounds(ReadOnlySpan<PianoSoundValueModel> sounds)
        {
            foreach (var sound in sounds) {
                PlaySound(sound);
            }
        }
    }
}