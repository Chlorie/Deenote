#nullable enable

using UnityEngine;

namespace Deenote.GamePlay.Stage
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
    }
}