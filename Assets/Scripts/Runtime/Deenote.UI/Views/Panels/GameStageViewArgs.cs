#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.UI.Views.Panels
{
    public abstract class GameStageViewArgs : ScriptableObject
    {
        public DeemoDifficultyResourceSet DifficultyArgs;

        public struct DeemoDifficultyResourceSet
        {
            public Sprite EasyIconSprite;
            public Color EasyTextColor;
            public Sprite NormalIconSprite;
            public Color NormalTextColor;
            public Sprite HardIconSprite;
            public Color HardTextColor;
            public Sprite ExtraIconSprite;
            public Color ExtraTextColor;

            public readonly (Sprite IconSprite, Color TextColor) Get(Difficulty difficulty)
                => difficulty switch {
                    Difficulty.Easy => (EasyIconSprite, EasyTextColor),
                    Difficulty.Normal => (NormalIconSprite, NormalTextColor),
                    Difficulty.Hard => (HardIconSprite, HardTextColor),
                    Difficulty.Extra => (ExtraIconSprite, ExtraTextColor),
                    _ => (null!, Color.white),
                };
        }
    }
}