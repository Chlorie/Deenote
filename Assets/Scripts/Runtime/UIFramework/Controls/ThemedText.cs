#nullable enable

using TMPro;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class ThemedText : UIThemedControl
    {
        [SerializeField] TextMeshProUGUI _text = default!;
        [SerializeField] UIThemeColor _color = UIThemeColor.TextPrimaryColor;

        protected override void OnThemeChanged(UIThemeArgs args)
        {
            if (_color is not UIThemeColor.None)
                _text.color = UISystem.CurrentTheme.GetColor(_color);

        }

        protected override void OnValidate()
        {
            _text ??= GetComponent<TextMeshProUGUI>();
            base.OnValidate();
        }
    }
}