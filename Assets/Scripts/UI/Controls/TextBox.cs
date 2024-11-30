#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed partial class TextBox : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _elevationImage = default!;
    }

    partial class TextBox
        : IPointerEnterHandler, IPointerExitHandler
        , IPointerDownHandler
    {
        [Header("Visual")]
        [SerializeField] bool _isHovering;
        [SerializeField] bool _isFocused;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isFocused = true;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            TranslateVisual();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            TranslateVisual();
        }

        private void TranslateVisual()
        {
            var colors = MainSystem.Args.UIColors;
        
            if (_isFocused) {
                _elevationImage.rectTransform.sizeDelta = _elevationImage.rectTransform.sizeDelta with { y = 2 };
                _elevationImage.color = colors.TextControlElevationFocusedColor;
                _backgroundImage.color = colors.ControlTertiaryColor;
                return;
            }
            _elevationImage.rectTransform.sizeDelta = _elevationImage.rectTransform.sizeDelta with { y = 1 };
            _elevationImage.color = colors.TextControlElevationColor;

            if (_isHovering) {
                _backgroundImage.color = colors.ControlSecondaryColor;
                return;
            }

            _backgroundImage.color = colors.ControlDefaultColor;
        }

        private void OnValidate()
        {
            TranslateVisual();
        }
    }
}