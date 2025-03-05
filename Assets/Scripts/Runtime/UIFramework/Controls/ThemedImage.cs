#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    [RequireComponent(typeof(Image))]
    public sealed class ThemedImage : UIThemedControl
    {
        [SerializeField] Image _image = default!;
        [SerializeField] UIThemeColor _color;

        protected override void OnThemeChanged(UIThemeArgs args)
        {
            if (_color is not UIThemeColor.None)
                _image.color = UISystem.CurrentTheme.GetColor(_color);
        }

        protected override void OnValidate()
        {
            _image ??= GetComponent<Image>();
            base.OnValidate();
        }
    }
}