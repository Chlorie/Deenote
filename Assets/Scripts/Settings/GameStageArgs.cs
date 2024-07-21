using Deenote.GameStage.Elements;
using System;
using UnityEngine;

namespace Deenote.Settings
{
    [CreateAssetMenu(fileName =nameof(GameStageArgs),menuName ="Settings/GameStageArgs")]
    public sealed class GameStageArgs : ScriptableObject
    {
        public float NotePanelLength;
        public float NoteTimeToZMultiplier;
        public float NoteFadeInZRange;
        // TODO: Use ZRange will cause notes unrecognizable when adjust sudden +,
        // suprised that the percent is such a large number...
        // public float NoteFadeRangePercent = 0.7417582418;

        [Header("Effect")]
        public float JudgeLinePeriod;
        public float BackgroundMaskMinAlpha;
        public float BackgroundMaskMaxAlpha;
        public float BackgroundMaskPeriod;
        public float JudgeLineHitEffectAlphaDecTime;
        [Header("Note")]
        public StageNoteController NoteControllerPrefab;
        public AudioClip EffectSoundAudioClip;
        public NoteSpritePrefab BlackNoteSpritePrefab;
        public NoteSpritePrefab NoSoundNoteSpritePrefab;
        public NoteSpritePrefab SlideNoteSpritePrefab;
        public NoteHitEffectSpritePrefabs HitEffectSpritePrefabs;

        [Serializable]
        public struct NoteSpritePrefab
        {
            public float Scale;
            public Sprite Sprite;
            public Color WaveColor;
        }

        [Serializable]
        public struct NoteHitEffectSpritePrefabs
        {
            /// <remarks>
            /// This should be >= any specific effect time below
            /// </remarks>
            public float HitEffectTime;

            [Header("Explosion")]
            public Sprite[] Explosions;
            public float ExplosionScale;
            public float ExplosionTime;

            [Header("Circle")]
            public float CircleScale;
            public float CircleTime;

            [Header("Wave")]
            public Vector2 WaveScale;
            public float WaveMaxAlpha;
            public float WaveGrowTime;
            public float WaveFadeTime;

            [Header("Glow")]
            public Color GlowColor;
            public Vector2 GlowScale;
            public float GlowGrowTime;
            public float GlowFadeTime;
        }

    }
} 