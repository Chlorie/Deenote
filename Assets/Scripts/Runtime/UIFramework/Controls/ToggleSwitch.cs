#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class ToggleSwitch : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _knobImage = default!;

        [SerializeField]
        private bool _isChecked_bf;
        public bool IsChecked
        {
            get => _isChecked_bf;
            set {
                if (Utils.SetField(ref _isChecked_bf, value)) {
                    DoVisualTransition();
                    IsCheckedChanged?.Invoke(value);
                }
            }
        }

        public event Action<bool>? IsCheckedChanged;

        public void SetIsCheckedWithoutNotify(bool value)
        {
            _isChecked_bf = value;
            DoVisualTransition();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                IsChecked = !IsChecked;
            }
        }

        protected override void DoVisualTransition(UIThemeArgs args, PressVisualState state)
        {
            const float HoverScale = 18f / 14f;

            Color bg, bdr, knb;
            float pos;
            if (IsChecked) {
                bg = state switch {
                    PressVisualState.Disabled => args.ControlAccentDisabledColor,
                    PressVisualState.Pressed => args.ControlAccentTertiaryColor,
                    PressVisualState.Hovering => args.ControlAccentSecondaryColor,
                    PressVisualState.Default or _ => args.ControlAccentDefaultColor,
                };
                knb = state is PressVisualState.Disabled
                    ? args.TextAccentDisabledColor
                    : args.TextAccentPrimaryColor;
                pos = 1f;

                _backgroundImage.color = bg;
                _borderImage.color = bg;
                _knobImage.color = knb;
            }
            else {
                bg = state switch {
                    PressVisualState.Disabled => args.ControlAltDisabledColor,
                    PressVisualState.Pressed => args.ControlAltQuarternaryColor,
                    PressVisualState.Hovering => args.ControlAltTertiaryColor,
                    PressVisualState.Default or _ => args.ControlAltSecondaryColor,
                };
                (bdr, knb) = state is PressVisualState.Disabled
                    ? (args.ControlStrongStrokeDisabledColor, args.TextDisabledColor)
                    : (args.ControlStrongStrokeDefaultColor, args.TextSecondaryColor);
                pos = 0f;

                _backgroundImage.color = bg;
                _borderImage.color = bdr;
                _knobImage.color = knb;
            }
            var knbscale = state is PressVisualState.Pressed or PressVisualState.Hovering
                ? HoverScale : 1f;

            var knobTsfm = _knobImage.rectTransform;
            knobTsfm.localScale = new Vector3(knbscale, knbscale, knbscale);
            knobTsfm.anchorMax = knobTsfm.anchorMax with { x = pos };
            knobTsfm.anchorMin = knobTsfm.anchorMin with { x = pos };
        }
    }
}