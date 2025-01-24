#nullable enable

using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Deenote.Library;
using UnityEngine.Events;
using System;

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

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = UISystem.ThemeArgs;

            const float HoverScale = 18f / 14f;

            if (IsChecked) {
                var state = GetPressVisualState();
                var (bg, knbscale) = state switch {
                    PressVisualState.Disabled => (colors.ControlAccentDisabledColor, 1f),
                    PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, HoverScale),
                    PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, HoverScale),
                    PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, 1f),
                };
                _backgroundImage.color = bg;
                _borderImage.color = bg;
                _knobImage.color = state is PressVisualState.Disabled ? colors.TextAccentDisabledColor : colors.TextAccentPrimaryColor;
                var knobTsfm = _knobImage.rectTransform;
                knobTsfm.localScale = new Vector3(knbscale, knbscale, knbscale);
                knobTsfm.anchorMax = knobTsfm.anchorMax with { x = 1f };
                knobTsfm.anchorMin = knobTsfm.anchorMin with { x = 1f };
            }
            else {
                var state = GetPressVisualState();
                var (bg, knbscale) = state switch {
                    PressVisualState.Disabled => (colors.ControlAltDisabledColor, 1f),
                    PressVisualState.Pressed => (colors.ControlAltQuarternaryColor, HoverScale),
                    PressVisualState.Hovering => (colors.ControlAltTertiaryColor, HoverScale),
                    PressVisualState.Default or _ => (colors.ControlAltSecondaryColor, 1f),
                };
                _backgroundImage.color = bg;
                _borderImage.color = state is PressVisualState.Disabled ? colors.ControlStrongStrokeDisabledColor : colors.ControlStrongStrokeDefaultColor;
                _knobImage.color = state is PressVisualState.Disabled ? colors.TextDisabledColor : colors.TextSecondaryColor;
                var knobTsfm = _knobImage.rectTransform;
                knobTsfm.localScale = new Vector3(knbscale, knbscale, knbscale);
                knobTsfm.anchorMax = knobTsfm.anchorMax with { x = 0f };
                knobTsfm.anchorMin = knobTsfm.anchorMin with { x = 0f };
            }
        }
    }
}