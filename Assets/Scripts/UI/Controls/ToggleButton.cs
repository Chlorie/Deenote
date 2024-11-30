#nullable enable

using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed partial class ToggleButton : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image? _image;
        [SerializeField] TextBlock? _text;

        [Header("Control")]
        [SerializeField] ToggleButtonGroup? _group;
        public ToggleButtonGroup? Group => _group;

        [SerializeField]
        private bool _isToggleOn_bf;

        public bool IsToggleOn
        {
            get => _isToggleOn_bf;
            internal set {
                if (Utils.SetField(ref _isToggleOn_bf, value)) {
                    TranslateVisual();
                }
            }
        }
    }

    partial class ToggleButton : IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual")]
        [SerializeField] bool _isHovering;
        [SerializeField] bool _isPressed;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            TranslateVisual();
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

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            TranslateVisual();
        }

        private void TranslateVisual()
        {
            var colors = MainSystem.Args.UIColors;

            if (IsToggleOn) {
                if (_isPressed) {
                    _backgroundImage.color = colors.AccentDefaultColor;
                    if (_image != null)
                        _image.color = colors.AccentTextSecondaryColor;
                    if (_text != null)
                        _text.TmpText.color = colors.AccentTextSecondaryColor;
                    return;
                }

                if (_isHovering) {
                    _backgroundImage.color = colors.AccentSecondaryColor;
                    if (_image != null)
                        _image.color = colors.AccentTextDefaultColor;
                    if (_text != null)
                        _text.TmpText.color = colors.AccentTextDefaultColor;
                    return;
                }

                _backgroundImage.color = colors.AccentDefaultColor;
                if (_image != null)
                    _image.color = colors.AccentTextDefaultColor;
                if (_text != null)
                    _text.TmpText.color = colors.AccentTextDefaultColor;
            }
            else {
                if (_isPressed) {
                    _backgroundImage.color = colors.ControlTertiaryColor;
                    if (_image != null)
                        _image.color = colors.TextSecondaryColor;
                    if (_text != null)
                        _text.TmpText.color = colors.TextSecondaryColor;
                    return;
                }

                if (_isHovering) {
                    _backgroundImage.color = colors.ControlSecondaryColor;
                    if (_image != null)
                        _image.color = colors.TextDefaultColor;
                    if (_text != null)
                        _text.TmpText.color = colors.TextDefaultColor;
                    return;
                }

                _backgroundImage.color = colors.ControlDefaultColor;
                if (_image != null)
                    _image.color = colors.TextDefaultColor;
                if (_text != null)
                    _text.TmpText.color = colors.TextDefaultColor;
            }
        }

        private void OnValidate()
        {
            TranslateVisual();
        }
    }
}