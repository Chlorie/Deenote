#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using System;
using UnityEngine;

namespace Deenote.UIFramework
{
    public static class UISystem
    {
        private static GameObject? _gameObject;
        private static UIThemeResources? _darkThemeResources;

        internal static GameObject GameObject => _gameObject ??= new GameObject(nameof(UISystem));

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

        internal static UIFocusManager FocusManager => UIFocusManager.Instance;

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

        public static event Action<UIThemeArgs>? ThemeChanged;

        public static event Action<IFocusable> FocusedControlChanged
        {
            add => FocusManager.FocusingChanged += value;
            remove => FocusManager.FocusingChanged -= value;
        }
    }
}