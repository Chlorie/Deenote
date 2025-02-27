#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class RadioButton : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] UnityEngine.UI.Image _backgroundImage = default!;
        [SerializeField] UnityEngine.UI.Image _borderImage = default!;
        [SerializeField] UnityEngine.UI.Image _handleImage = default!;

        [SerializeField] RadioButtonGroup _group = default!;
        public RadioButtonGroup Group => _group;

        [SerializeField]
        private bool _isChecked_bf;
        public bool IsChecked => _isChecked_bf;

        public override bool IsInteractable
        {
            get => base.IsInteractable && Group.IsInteractable;
            set => base.IsInteractable = value;
        }

        public event Action? Checked;

        public void SetChecked()
        {
            Group.SetCheckedRatio(this);
        }

        internal void SetIsCheckedInternal(bool value, bool notify)
        {
            if (Utils.SetField(ref _isChecked_bf, value)) {
                DoVisualTransition();
                if (value is true && notify)
                    Checked?.Invoke();
            }
        }

        protected override void Awake()
        {
            if (_group == null) {
                _group = RadioButtonGroup.Shared;
            }
            _group.AddButtonOnAwake(this);
            base.Awake();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                SetChecked();
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
                var (bg, scl) = state switch {
                    PressVisualState.Disabled => (colors.ControlAccentDisabledColor, 1f),
                    PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, 1f),
                    PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, HoverScale),
                    PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, 1f),
                };
                _backgroundImage.color = bg;
                _borderImage.color = colors.ControlTransparentColor;
                _handleImage.gameObject.SetActive(true);
                _handleImage.color = state is PressVisualState.Disabled ? colors.TextAccentDisabledColor : colors.TextAccentPrimaryColor;
                _handleImage.rectTransform.localScale = new Vector3(scl, scl, scl);
            }
            else {
                var state = GetPressVisualState();
                var (bg, bdr) = state switch {
                    PressVisualState.Disabled => (colors.ControlAltDisabledColor, colors.TextDisabledColor),
                    PressVisualState.Pressed => (colors.ControlAltQuarternaryColor, colors.TextTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlAltTertiaryColor, colors.TextSecondaryColor),
                    PressVisualState.Default or _ => (colors.ControlAltSecondaryColor, colors.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                _borderImage.color = bdr;
                if (state is PressVisualState.Pressed) {
                    _handleImage.gameObject.SetActive(true);
                    _handleImage.rectTransform.localScale = Vector3.one;
                }
                else {
                    _handleImage.gameObject.SetActive(false);
                }
            }
        }

        internal void UpdateVisual() => DoVisualTransition();
    }
}
