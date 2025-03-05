#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deenote.UIFramework
{
    public static class UISystem
    {
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
                if(Utils.SetField(ref _currentThemeIndex, index)) {
                    ThemeChanged?.Invoke(_themeArgs[index]);
                }
            }
        }

        public static ReadOnlySpan<UIThemeArgs> Themes => _themeArgs;

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

        public static event Action<MouseButton, Vector2> MouseFocusChanged
        {
            add => UIFocusManager.Instance.FocusChanged += value;
            remove => UIFocusManager.Instance.FocusChanged -= value;
        }

        public static event Action<UIThemeArgs>? ThemeChanged;
    }
}