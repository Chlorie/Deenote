#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.UIFramework.Font;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Deenote.UIFramework
{
    public static class UISystem
    {
        private static GameObject? _gameObject;

        internal static GameObject GameObject => _gameObject ??= new GameObject(nameof(UISystem));

        #region Theme

        private static UIThemeResources? _darkThemeResources;
        internal static UIThemeResources ThemeResources
        {
            get => _darkThemeResources ??= Resources.Load<UIThemeResources>($"UI/ThemeResources");
        }


        private static UIThemeArgs[] _themeArgs = Resources.LoadAll<UIThemeArgs>("UI/Themes");
        private static int _currentThemeIndex;

        public static UIThemeArgs CurrentTheme
        {
            get => _themeArgs[_currentThemeIndex];
            set {
                var index = Array.IndexOf(_themeArgs, value);
                Guard.IsGreaterThanOrEqualTo(index, 0);
                if (Utils.SetField(ref _currentThemeIndex, index)) {
                    ThemeChanged?.Invoke(_themeArgs[index]);
                }
            }
        }

        public static ReadOnlySpan<UIThemeArgs> Themes => _themeArgs;

        public static event Action<UIThemeArgs>? ThemeChanged;

        public static bool SetTheme(string name)
        {
            for (int i = 0; i < _themeArgs.Length; i++) {
                UIThemeArgs? theme = _themeArgs[i];
                if (theme.ThemeName == name) {
                    SetTheme(i);
                    return true;
                }
            }
            return false;
        }


        public static void SetTheme(int index)
        {
            Guard.IsInRangeFor(index, _themeArgs);

            if (Utils.SetField(ref _currentThemeIndex, index)) {
                ThemeChanged?.Invoke(_themeArgs[index]);
            }
        }

        #endregion

        #region Font

        private static TMP_FontAsset? _fontAsset;
        public static TMP_FontAsset FontAsset
        {
            get {
                if (_fontAsset is null) {
                    var fontAsset = UIFontManager.LoadSystemFontAssets(ThemeResources.PreferedFontName);
                    fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    foreach (string name in ThemeResources.FallbackFontNames) {
                        var fallbackFont = UIFontManager.LoadSystemFontAssets(name);
                        if (fallbackFont != null)
                            fontAsset.fallbackFontAssetTable.Add(fallbackFont);
                        else
                            Debug.LogWarning($"Load font {fallbackFont} failed");
                    }
                    fontAsset.fallbackFontAssetTable.Add(ThemeResources.FinalFallbackFont);
                    _fontAsset = fontAsset;
                }

                return _fontAsset;
            }
        }

        #endregion

        internal static UIFocusManager FocusManager => UIFocusManager.Instance;

        public static event Action<IFocusable> FocusedControlChanged
        {
            add => FocusManager.FocusingChanged += value;
            remove => FocusManager.FocusingChanged -= value;
        }
    }
}