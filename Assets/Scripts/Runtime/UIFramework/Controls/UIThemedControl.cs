#nullable enable

using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    [DisallowMultipleComponent]
    public abstract class UIThemedControl : MonoBehaviour
    {
        protected abstract void OnThemeChanged(UIThemeArgs args);

        protected virtual void Awake()
        {
            OnThemeChanged(UISystem.CurrentTheme);
            UISystem.ThemeChanged += OnThemeChanged;
        }

        protected virtual void OnDestroy()
        {
            UISystem.ThemeChanged -= OnThemeChanged;
        }

        protected virtual void OnValidate()
        {
            OnThemeChanged(UISystem.CurrentTheme);
        }
    }
}