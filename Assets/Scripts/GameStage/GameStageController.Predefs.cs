using Deenote.GameStage.Elements;
using System;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] StageNoteController _noteControllerPrefab;
        [SerializeField] AudioClip _effectSoundAudioClip;
        [SerializeField] NoteSpritePrefab _blackNoteSpritePrefab;
        [SerializeField] NoteSpritePrefab _noSoundNoteSpritePrefab;
        [SerializeField] NoteSpritePrefab _slideNoteSpritePrefab;
        [SerializeField] NoteHitEffectSpritePrefabs _hitEffectSpritePrefabs;

        public AudioClip EffectSoundAudioClip => _effectSoundAudioClip;

        public ref readonly NoteSpritePrefab BlackNoteSpritePrefab => ref _blackNoteSpritePrefab;
        public ref readonly NoteSpritePrefab NoSoundNoteSpritePrefab => ref _noSoundNoteSpritePrefab;
        public ref readonly NoteSpritePrefab SlideNoteSpritePrefab => ref _slideNoteSpritePrefab;
        public ref readonly NoteHitEffectSpritePrefabs HitEffectSpritePrefabs => ref _hitEffectSpritePrefabs;

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

            public Sprite[] Explosions;
            public float ExplosionScale;
            public float ExplosionTime;

            public Sprite Circle;
            public float CircleScale;
            public float CircleTime;

            public Sprite Wave;
            public float WaveScale;
            public float WaveGrowTime;
            public float WaveFadeTime;

            public Sprite Glow;
            public Color GlowColor;
            public float GlowScale;
            public float GlowGrowTime;
            public float GlowFadeTime;
        }
    }
}