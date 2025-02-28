#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.GamePlay.UI
{
    [CreateAssetMenu(
        fileName = nameof(GameStageUIArgs),
        menuName = $"Deenote/GamePlay/{nameof(GameStageUIArgs)}")]
    public sealed class GameStageUIArgs : ScriptableObject
    {
        public Sprite EasyIconSprite = default!;
        public Color EasyTextColor;
        public Sprite NormalIconSprite = default!;
        public Color NormalTextColor;
        public Sprite HardIconSprite = default!;
        public Color HardTextColor;
        public Sprite ExtraIconSprite = default!;
        public Color ExtraTextColor;

        public (Sprite IconSprite, Color TextColor) GetDifficultyArgs(Difficulty difficulty)
            => difficulty switch {
                Difficulty.Easy => (EasyIconSprite, EasyTextColor),
                Difficulty.Normal => (NormalIconSprite, NormalTextColor),
                Difficulty.Hard => (HardIconSprite, HardTextColor),
                Difficulty.Extra => (ExtraIconSprite, ExtraTextColor),
                _ => (null!, Color.white),
            };
    }
}