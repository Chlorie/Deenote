#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class Button : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] UnityEngine.UI.Image _backgroundImage = default!;
        [SerializeField] UnityEngine.UI.Image _image = default!;
        [SerializeField] TextBlock _text = default!;

        [SerializeField] ButtonColorSet _colorSet;

        public UnityEngine.UI.Image Image => _image;

        public TextBlock Text => _text;

        public ButtonColorSet ColorSet
        {
            get => _colorSet;
            set {
                if (Utils.SetField(ref _colorSet, value)) {
                    DoVisualTransition();
                }
            }
        }

        public event Action? Clicked;

        private void OnDisable()
        {
            _isHovering = false;
            _isPressed = false;
        }

        public async UniTask OnClickAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UniTaskCompletionSource utcs = new();
            try {
                using var ctr = cancellationToken.Register(() => utcs.TrySetCanceled());
                Clicked += SetResult;
                await utcs.Task;
            } finally {
                Clicked -= SetResult;
            }

            void SetResult() => utcs.TrySetResult();
        }

        public UniTask OnClickAsync() => OnClickAsync(destroyCancellationToken);

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                Clicked?.Invoke();
            }
        }

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = UISystem.ThemeArgs;

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
                    PressVisualState.Pressed => (_colorSet is ButtonColorSet.Caution
                        ? colors.CautionButtonBackgroundPressedColor : colors.ControlTertiaryColor, colors.TextTertiaryColor),
                    PressVisualState.Hovering => (_colorSet is ButtonColorSet.Caution
                        ? colors.CautionButtonBackgroundHoverColor : colors.ControlSecondaryColor, colors.TextSecondaryColor),
                    PressVisualState.Default or _ => (_colorSet is ButtonColorSet.Transparent or ButtonColorSet.Caution
                        ? colors.ControlTransparentColor : colors.ControlDefaultColor, colors.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                if (_text != null)
                    _text.TmpText.color = txt;
            }
        }

        public enum ButtonColorSet
        {
            Default,
            Transparent,
            Accent,
            Caution,
        }
    }
}
