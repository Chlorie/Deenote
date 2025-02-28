#nullable enable

using UnityEngine;

namespace Deenote.Core.Audio
{
    public interface IClipProvider
    {
        AudioClip Clip { get; }
    }

    public sealed class DecodedClipProvider : IClipProvider
    {
        public DecodedClipProvider(AudioClip clip) => Clip = clip;
        public AudioClip Clip { get; }
    }

    public abstract class StreamingClipProvider : IClipProvider
    {
        public int Channels { get; init; }
        public int SampleRate { get; init; }
        public int TotalSamples { get; init; }
        public AudioClip Clip => _clip ??= CreateClip();

        protected abstract void SetPosition(int position);
        protected abstract void ReadSamples(float[] samples);

        private AudioClip? _clip;

        private AudioClip CreateClip() => AudioClip.Create("Audio",
            TotalSamples / Channels, Channels, SampleRate, stream: true,
            pcmreadercallback: ReadSamples, pcmsetpositioncallback: SetPosition);
    }
}