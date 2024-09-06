using Reflex.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Deenote.Localization
{
    public class SystemFontFallbackSetter : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset _fontAsset = null!;
        [SerializeField] private SystemFontFallbackSettings _settings = null!;
        [Inject] private LocalizationSystem _localizationSystem = null!;
        private Dictionary<string, TMP_FontAsset> _systemFontAssets = new();
        private string[] _installedFontPaths = null!;

        private void Awake()
        {
            _installedFontPaths = Font.GetPathsToOSFonts();
            _localizationSystem.OnLanguageChanged += LanguageChanged;
            LanguageChanged(_localizationSystem.CurrentLanguage);
        }

        private void LanguageChanged(string languageCode)
        {
            if (_settings.Settings.FirstOrDefault(s => s.LanguageCode == languageCode)
                is var settings && settings.LanguageCode != languageCode)
                return;
            SetFontFallbacks(settings.FontFileNames);
        }

        private void SetFontFallbacks(string[] fontFileName)
        {
            var lastResort = _fontAsset.fallbackFontAssetTable[^1];
            var table = _fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
            foreach (var fileName in fontFileName) {
                if (_systemFontAssets.TryGetValue(fileName, out var asset)) {
                    table.Add(asset);
                    continue;
                }
                if (_installedFontPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == fileName)
                    is { } path)
                    table.Add(_systemFontAssets[fileName] = TMP_FontAsset.CreateFontAsset(new Font(path)));
            }
            table.Add(lastResort);
        }
    }
}