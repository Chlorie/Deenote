#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    [RequireComponent(typeof(Image))]
    public sealed class ThemedImage : MonoBehaviour
    {
        [SerializeField] Image _image = default!;
        [SerializeField] UIColor _color;

        private void OnValidate()
        {
            _image ??= GetComponent<Image>();
            if (UISystem.ThemeArgs.GetColor(_color) is Color color)
                _image.color = color;
        }
    }
}