#nullable enable

using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    [DisallowMultipleComponent]
    public abstract class UIThemedControl : MonoBehaviour
    {
        protected abstract void OnThemeChanged(UIThemeColorArgs args);

        protected virtual void Awake()
        {
            OnThemeChanged(UISystem.ColorArgs);
        }

        protected virtual void OnValidate()
        {
            OnThemeChanged(UISystem.ColorArgs);
        }
    }
}