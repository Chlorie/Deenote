#nullable enable

using UnityEngine;

namespace Deenote.Core.GamePlay.Audio
{
    [CreateAssetMenu(
        fileName = nameof(HitSoundArgs),
        menuName = $"{nameof(Deenote)}/{nameof(GamePlay)}/{nameof(HitSoundArgs)}")]
    public sealed class HitSoundArgs : ScriptableObject
    {
#nullable disable
        public AudioClip ClickHitSoundClip;
        public float ClickHitSoundBaseVolume = 1f;
        public AudioClip SlideHitSoundClip;
        public float SlideHitSoundBaseVolume = 1f;
    }
}