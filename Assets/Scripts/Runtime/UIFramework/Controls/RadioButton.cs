#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class RadioButton : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _handleImage = default!;

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

        protected override void DoVisualTransition(UIThemeArgs args, PressVisualState state)
        {
            const float HoverScale = 18f / 14f;

            if (IsChecked) {
                var bg = state switch {
                    PressVisualState.Disabled => args.ControlAccentDisabledColor,
                    PressVisualState.Pressed => args.ControlAccentTertiaryColor,
                    PressVisualState.Hovering => args.ControlAccentSecondaryColor,
                    PressVisualState.Default or _ => args.ControlAccentDefaultColor,
                };
                var scl = state is PressVisualState.Hovering ? HoverScale : 1f;
                _backgroundImage.color = bg;
                _borderImage.color = args.ControlTransparentColor;
                _handleImage.gameObject.SetActive(true);
                _handleImage.color = state is PressVisualState.Disabled
                    ? args.TextAccentDisabledColor
                    : args.TextAccentPrimaryColor;

                _handleImage.rectTransform.localScale = new Vector3(scl, scl, scl);
            }
            else {
                var (bg, bdr) = state switch {
                    PressVisualState.Disabled => (args.ControlAltDisabledColor, args.TextDisabledColor),
                    PressVisualState.Pressed => (args.ControlAltQuarternaryColor, args.TextTertiaryColor),
                    PressVisualState.Hovering => (args.ControlAltTertiaryColor, args.TextSecondaryColor),
                    PressVisualState.Default or _ => (args.ControlAltSecondaryColor, args.TextPrimaryColor),
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
