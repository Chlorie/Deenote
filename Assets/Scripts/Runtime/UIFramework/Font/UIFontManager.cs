#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Deenote.UIFramework.Font
{
    public static class UIFontManager
    {
        private static string[]? _systemFontPaths;
        private static Dictionary<string, TMP_FontAsset>? _cacheFontAssets;

        public static TMP_FontAsset LoadSystemFontAssets(string name)
        {
            if (_cacheFontAssets?.TryGetValue(name, out var asset) is true)
                return asset;

            _systemFontPaths ??= UnityEngine.Font.GetPathsToOSFonts();
            var fontPath = Array.Find(_systemFontPaths, path => Path.GetFileNameWithoutExtension(path) == name);
            if (fontPath is null) {
                Debug.LogWarning($"Font '{name}' not found on this system, falling back to first available font");
                fontPath = _systemFontPaths.Length > 0 ? _systemFontPaths[0] : null;
                if (fontPath is null)
                    throw new InvalidOperationException("No system fonts available");
            }
            asset = TMP_FontAsset.CreateFontAsset(new UnityEngine.Font(fontPath));
            asset.atlasPopulationMode = TMPro.AtlasPopulationMode.Dynamic;
            (_cacheFontAssets ??= new()).Add(name, asset);
            return asset;
        }
    }
}