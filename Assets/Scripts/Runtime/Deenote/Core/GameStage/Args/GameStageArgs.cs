#nullable enable

using Deenote.Core.Editing;
using Deenote.Library.Mathematics;
using System;
using UnityEngine;

namespace Deenote.Core.GameStage.Args
{
    [CreateAssetMenu(fileName = nameof(GameStageArgs), menuName = $"{nameof(Deenote)}/{nameof(GameStageArgs)}")]
    public sealed class GameStageArgs : ScriptableObject
    {
        public float NotePanelWidth;
        public float NotePanelBaseLength;
        /// <summary>
        /// Multiplier when note speed is 1
        /// </summary>
        public float NoteTimeToZBaseMultiplier;
        [SerializeField, Tooltip("Actual multipliers for every speed from 1.0 ~ 9.5, with interval 0.5")]
        private float[] NoteTimeToZBaseMultipliers = default!;

        private PiecewiseLinearFunction? _noteTimeToZBaseMultiplierFunc;
        public PiecewiseLinearFunction NoteTimeToZBaseMultiplierFunction
        {
            get {
                if (_noteTimeToZBaseMultiplierFunc is null) {
                    var nodes = (stackalloc (float Speed, float Multiplier)[NoteTimeToZBaseMultipliers.Length]);
                    for (int i = 0; i < NoteTimeToZBaseMultipliers.Length; i++) {
                        nodes[i] = (i * 0.5f + 1.0f, NoteTimeToZBaseMultipliers[i]);
                    }
                    _noteTimeToZBaseMultiplierFunc = new PiecewiseLinearFunction(nodes, fillHorizontalBothSide: true);
                }
                return _noteTimeToZBaseMultiplierFunc;
            }
        }

        [Range(0f, 1f)] public float NoteFadeInRangePercent;
        //public float NoteFadeInZRange;=42.8
        // Note: Use ZRange will cause low alpha notes when adjust sudden +,
        // suprised that the percent is such a large number...
        // NoteFadeRangePercent = NoteFadeInZRange / (NoteTimeToZMultiplier * NotePanelLength);

        [Header("Prefabs")]
        [SerializeField]
        internal GameStageNoteController GamePlayNotePrefab = default!;
        [SerializeField]
        internal PlacementNoteIndicatorController PlacementNoteIndicatorPrefab = default!;

        [Header("Note")]
        public AudioClip EffectSoundAudioClip = default!;
        public NoteSpritePrefab BlackNoteSpritePrefab;
        public NoteSpritePrefab NoSoundNoteSpritePrefab;
        public NoteSpritePrefab SlideNoteSpritePrefab;
        public NoteSpritePrefab SwipeNoteSpritePrefab;
        public HoldNoteBodySpritePrefab HoldSpritePrefab;
        public NoteHitEffectSpritePrefabs HitEffectSpritePrefabs;
        public Color NoteSelectedColor = new(85f / 255f, 192f / 255f, 1f);
        public Color NoteCollidedColor = new(1f, 85f / 255f, 85f / 255f);
        [Range(0f, 1f)] public float NoteDownplayAlpha = 0.5f;

        [Serializable]
        public struct NoteSpritePrefab
        {
            public float Scale;
            public Sprite Sprite;
            public Color WaveColor;
        }

        [Serializable]
        public struct HoldNoteBodySpritePrefab
        {
            public float ScaleX;
            public Sprite Sprite;
        }

        [Serializable]
        public struct NoteHitEffectSpritePrefabs
        {
            /// <remarks>
            /// This should be >= any specific effect time below
            /// </remarks>
            public float HitEffectTime;

            [Header("Explosion")]
            public Sprite HoldingExplosion;
            public Sprite[] Explosions;
            public float ExplosionScale;
            public float ExplosionTime;

            [Header("Circle")]
            public float CircleScale;
            public float CircleTime;

            [Header("Wave")]
            public Vector2 WaveScale;
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