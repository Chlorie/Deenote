#nullable enable

using TMPro;
using UnityEngine;

namespace Deenote.UIFramework.Font
{
    [CreateAssetMenu(
      fileName = nameof(DynamicallyFallbackFontAsset),
      menuName = $"Deenote.UIFramework/{nameof(DynamicallyFallbackFontAsset)}")]
    public sealed class DynamicallyFallbackFontAsset : ScriptableObject
    {
        [SerializeField] TMP_FontAsset _fontAsset = default!;
        public string[] FallbackFontNames = default!;

        private bool _initialized = false;

        public TMP_FontAsset FontAsset
        {
            get {
                if (!_initialized) {
                    foreach (var name in FallbackFontNames) {
                        var fallbackFont = UIFontManager.LoadSystemFontAssets(name);
                        if (fallbackFont != null)
                            _fontAsset.fallbackFontAssetTable.Add(fallbackFont);
                        else
                            Debug.LogWarning($"Load font {fallbackFont} failed");
                    }
                    _initialized = true;
                }
                return _fontAsset;
            }
        }
    }
}