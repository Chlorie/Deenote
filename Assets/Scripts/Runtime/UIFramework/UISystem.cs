#nullable enable

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deenote.UIFramework
{
    public static class UISystem
    {
        private static UIThemeResources? _darkThemeResources;
        public static UIThemeResources ThemeArgs
        {
            get => _darkThemeResources ??= Resources.Load<UIThemeResources>($"UIThemes/Dark");
        }

        private static UIThemeColorArgs? _colorArgs;

        public static UIThemeColorArgs ColorArgs
        {
            get => _colorArgs ??= Resources.Load<UIThemeColorArgs>("UIThemes/DarkColorArgs");
        }

        public static event Action<MouseButton, Vector2> MouseFocusChanged
        {
            add => UIFocusManager.Instance.FocusChanged += value;
            remove => UIFocusManager.Instance.FocusChanged -= value;
        }
    }
}