#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class Button : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _image = default!;
        [SerializeField] TextBlock _text = default!;

        [SerializeField] ButtonColorSet _colorSet;

        public Image Image => _image;

        [Obsolete("新ui从TextBlock获取")]
        public LocalizedText LocText => _text.LocalizedText;

        public TextBlock Text => _text;

        public UnityEvent OnClick { get; } = new();

        public UniTask OnClickAsync(CancellationToken cancellationToken)
            => new AsyncUnityEventHandler(OnClick, cancellationToken, true).OnInvokeAsync();

        public IAsyncClickEventHandler GetAsyncClickEventHandler()
            => new AsyncUnityEventHandler(OnClick, destroyCancellationToken, false);

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!IsLeftButtonOnInteractableControl(eventData)) {
                OnClick.Invoke();
            }
        }

        protected override void TranslateVisual()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = MainSystem.Args.UIColors;

            if (_colorSet is ButtonColorSet.Accent) {
                var (bg, txt) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (colors.ControlAccentDisabledColor, colors.TextAccentDisabledColor),
                    PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, colors.TextAccentTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, colors.TextAccentSecondaryColor),
                    PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, colors.TextAccentPrimaryColor),
                };
                _backgroundImage.color = bg;
                if (_text != null)
                    _text.TmpText.color = txt;
            }
            else {
                var (bg, txt) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (colors.ControlDisabledColor, colors.TextDisabledColor),
                    PressVisualState.Pressed => (colors.ControlTertiaryColor, colors.TextTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlSecondaryColor, colors.TextSecondaryColor),
                    PressVisualState.Default or _ => (_colorSet is ButtonColorSet.Transparent
                        ? colors.ControlTransparentColor : colors.ControlDefaultColor, colors.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                if (_text != null)
                    _text.TmpText.color = txt;
            }
        }

        private enum ButtonColorSet
        {
            Default,
            Transparent,
            Accent,
        }
    }
}
