#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

namespace Deenote.UI.Controls
{
    //[RequireComponent(typeof(UnityButton))]
    public sealed partial class Button : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _image = default!;
        [SerializeField] TextBlock _text = default!;

        [SerializeField] ButtonColorSet _colorSet;

        public Image Image => _image;
        public LocalizedText Text => _text.LocalizedText;

        public bool IsInteractable { get; set; }

        public UnityEvent OnClick { get; } = new();

        public UniTask OnClickAsync(CancellationToken cancellationToken)
            => new AsyncUnityEventHandler(OnClick, cancellationToken, true).OnInvokeAsync();

        public IAsyncClickEventHandler GetAsyncClickEventHandler()
            => new AsyncUnityEventHandler(OnClick, destroyCancellationToken, false);

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
            => OnClick.Invoke();

        private enum ButtonColorSet
        {
            Default,
            Transparent,
            Accent,
        }
    }

    partial class Button
        : IPointerEnterHandler, IPointerExitHandler
        , IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual")]
        [SerializeField] bool _isHovering;
        [SerializeField] bool _isPressed;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;
            Debug.Log("Down");
            _isPressed = true;
            TranslateVisual();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Enter");
            _isHovering = true;
            TranslateVisual();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Exit");
            _isHovering = false;
            TranslateVisual();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;

            Debug.Log(eventData.dragging);
            if (eventData.dragging)
                return;
            Debug.Log("Up");
            _isPressed = false;
            TranslateVisual();
        }

        private void TranslateVisual()
        {
            var colors = MainSystem.Args.UIColors;

            if (_colorSet is ButtonColorSet.Accent) {
                if (_isPressed) {
                    _backgroundImage.color = colors.AccentDefaultColor;
                    if (_text != null)
                        _text.TmpText.color = colors.AccentTextTertiaryColor;
                    return;
                }

                if (_isHovering) {
                    _backgroundImage.color = colors.AccentSecondaryColor;
                    if (_text != null)
                        _text.TmpText.color = colors.AccentTextSecondaryColor;
                    return;
                }

                _backgroundImage.color = colors.AccentDefaultColor;
                if (_text != null)
                    _text.TmpText.color = colors.AccentTextDefaultColor;
            }
            else {
                if (_isPressed) {
                    _backgroundImage.color = colors.ControlTertiaryColor;
                    if (_text != null)
                        _text.TmpText.color = colors.TextTertiaryColor;
                    return;
                }

                if (_isHovering) {
                    _backgroundImage.color = colors.ControlSecondaryColor;
                    if (_text != null)
                        _text.TmpText.color = colors.TextSecondaryColor;
                    return;
                }

                _backgroundImage.color = _colorSet switch {
                    ButtonColorSet.Default => colors.ControlDefaultColor,
                    ButtonColorSet.Transparent => colors.ControlTransparentColor,
                    _ => colors.ControlDefaultColor,
                };
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