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
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _image = default!;
        [SerializeField] TextBlock _text = default!;
        [SerializeField] Image _borderImage = default!;

        [SerializeField] ButtonColorSet _colorSet;

        public Image Image => _image;

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

        protected override void DoVisualTransition(UIThemeArgs args, PressVisualState state)
        {
            Color bg, fg, bdr;
            switch (_colorSet) {
                case ButtonColorSet.Transparent:
                    (bg, fg, bdr) = state switch {
                        PressVisualState.Disabled => (args.ControlDisabledColor, args.TextDisabledColor, args.ControlTransparentColor),
                        PressVisualState.Pressed => (args.ControlTertiaryColor, args.TextTertiaryColor, args.ControlStrokeDefaultColor),
                        PressVisualState.Hovering => (args.ControlSecondaryColor, args.TextSecondaryColor, args.ControlElevationBorderColor),
                        PressVisualState.Default or _ => (args.ControlTransparentColor, args.TextPrimaryColor, args.ControlTransparentColor),
                    };
                    break;
                case ButtonColorSet.Accent:
                    (bg, fg, bdr) = state switch {
                        PressVisualState.Disabled => (args.ControlAccentDisabledColor, args.TextAccentDisabledColor, args.ControlTransparentColor),
                        PressVisualState.Pressed => (args.ControlAccentTertiaryColor, args.TextAccentSecondaryColor, args.ControlTransparentColor),
                        PressVisualState.Hovering => (args.ControlAccentSecondaryColor, args.TextAccentPrimaryColor, args.ControlAccentElevationBorderColor),
                        PressVisualState.Default or _ => (args.ControlAccentDefaultColor, args.TextAccentPrimaryColor, args.ControlAccentElevationBorderColor),
                    };
                    break;
                case ButtonColorSet.Caution:
                    (bg, fg, bdr) = state switch {
                        PressVisualState.Disabled => (args.ControlDisabledColor, args.TextDisabledColor, args.ControlTransparentColor),
                        PressVisualState.Pressed => (args.CautionButtonBackgroundPressed, args.TextTertiaryColor, args.ControlStrokeDefaultColor),
                        PressVisualState.Hovering => (args.CautionButtonBackgroundHovered, args.TextSecondaryColor, args.ControlElevationBorderColor),
                        PressVisualState.Default or _ => (args.ControlTransparentColor, args.TextPrimaryColor, args.ControlTransparentColor),
                    };
                    break;
                case ButtonColorSet.Default or _:
                    (bg, fg, bdr) = state switch {
                        PressVisualState.Disabled => (args.ControlDisabledColor, args.TextDisabledColor, args.ControlStrokeDefaultColor),
                        PressVisualState.Pressed => (args.ControlTertiaryColor, args.TextTertiaryColor, args.ControlStrokeDefaultColor),
                        PressVisualState.Hovering => (args.ControlSecondaryColor, args.TextSecondaryColor, args.ControlElevationBorderColor),
                        PressVisualState.Default or _ => (args.ControlDefaultColor, args.TextPrimaryColor, args.ControlElevationBorderColor),
                    };
                    break;
            }

            _backgroundImage.color = bg;
            _borderImage.color = bdr;
            if (_text is not null)
                _text.TmpText.color = fg;
            if (_image is not null)
                _image.color = fg;
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
