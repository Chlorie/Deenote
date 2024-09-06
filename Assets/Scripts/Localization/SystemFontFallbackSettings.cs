using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Deenote.Localization
{
    [CreateAssetMenu(fileName = nameof(SystemFontFallbackSettings),
        menuName = $"{nameof(Deenote)}/{nameof(SystemFontFallbackSettings)}",
        order = 0)]
    public class SystemFontFallbackSettings : ScriptableObject
    {
        [field: SerializeField] public PerLanguageSettings[] Settings { get; private set; } = null!;

        [Serializable]
        public struct PerLanguageSettings
        {
            public string LanguageCode;
            [FormerlySerializedAs("FontNames")] public string[] FontFileNames;
        }
    }
}