#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage.Args
{
    [CreateAssetMenu(fileName = nameof(DeemoGameStageArgs), menuName = $"{nameof(Deenote)}/{nameof(DeemoGameStageArgs)}")]
    public sealed class DeemoGameStageArgs : ScriptableObject
    {
        [Header("Effect")]
        public float JudgeLinePeriod;
        public float BackgroundMaskMinAlpha;
        public float BackgroundMaskMaxAlpha;
        public float BackgroundMaskPeriod;
        public float JudgeLineHitEffectAlphaDecTime;
        [Header("Note")]
        public Color HoldingBodyColor;
        public float HoldingExplosionScaleY;
    }
}