#nullable enable

using UnityEngine;

namespace Deenote.Settings
{
    [CreateAssetMenu(fileName = nameof(GameStageViewArgs), menuName = $"{nameof(Deenote)}/{nameof(GameStageViewArgs)}")]
    public sealed class GameStageViewArgs : ScriptableObject
    {
        [Header("Difficulty Resources")]
        public Sprite EasyDifficultyIconSprite;
        public Color EasyLevelTextColor;
        public Sprite NormalDifficultyIconSprite;
        public Color NormalLevelTextColor;
        public Sprite HardDifficultyIconSprite;
        public Color HardLevelTextColor;
        public Sprite ExtraDifficultyIconSprite;
        public Color ExtraLevelTextColor;
        [Header("Combo")]
        public int MinDisplayCombo;
        [Header("Combo Number")]
        public float ComboNumberGreyIncTime;
        public float ComboNumberGreyDecTime;
        [Header("Combo Number Shadow")]
        public float ComboShadowDuration;
        public float ComboShadowMinAlpha;
        public float ComboShadowMaxScale;
        [Header("Combo Wave")]
        public float ComboCircleStartTime;
        public float ComboCircleScaleIncTime;
        public float ComboCircleMaxScale;
        public float ComboCircleFadeOutStartTime;
        [Header("Combo ShockWave")]
        public float ComboShockWaveAlphaIncTime;
        public float ComboShockWaveMoveTime;
        public float ComboShockWaveAlphaDecTime;
        public float ComboShockWaveStartX;
        public float ComboShockWaveEndX;
        [Header("Combo Charming")]
        public float ComboCharmingGrowTime;
        public float ComboCharmingFadeTime;
        public float ComboCharmingAlphaIncTime;
        public float ComboCharmingAlphaDecStartTime;
        public float ComboCharmingAlphaDecTime;
        public float ComboCharmingMaxScaleY;
    }
}