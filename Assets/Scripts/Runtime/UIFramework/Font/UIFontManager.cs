#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using TMPro;

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
            asset = TMP_FontAsset.CreateFontAsset(new UnityEngine.Font(fontPath));
            asset.atlasPopulationMode = TMPro.AtlasPopulationMode.Dynamic;
            (_cacheFontAssets ??= new()).Add(name, asset);
            return asset;
        }
    }
}