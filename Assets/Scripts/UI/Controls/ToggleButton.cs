#nullable enable

using Deenote.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class ToggleButton : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _image = default!;
        [SerializeField] TextBlock _text = default!;

        [Header("Control")]
        [SerializeField] ToggleButtonGroup? _group;
        public ToggleButtonGroup? Group => _group;

        [SerializeField]
        private bool _isChecked_bf;

        public bool IsChecked => _isChecked_bf;

        public UnityEvent<bool> OnValueChanged { get; } = new();

        internal void SetIsChecked(bool value)
        {
            if (Utils.SetField(ref _isChecked_bf, value)) {
                TranslateVisual();
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                if (Group is null) {
                    SetIsChecked(!IsChecked);
                }
                else {
                    Group.Toggle(this);
                }
            }
        }

        protected override void TranslateVisual()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = MainSystem.Args.UIColors;

            if (IsChecked) {
                var (bg, txt) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (colors.ControlAccentDisabledColor, colors.TextAccentDisabledColor),
                    PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, colors.TextAccentTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, colors.TextAccentSecondaryColor),
                    PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, colors.TextAccentPrimaryColor),
                };
                _backgroundImage.color = bg;
                _image.color = txt;
                _text.TmpText.color = txt;
            }
            else {
                var (bg, txt) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (colors.ControlDisabledColor, colors.TextDisabledColor),
                    PressVisualState.Pressed => (colors.ControlTertiaryColor, colors.TextTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlSecondaryColor, colors.TextSecondaryColor),
                    PressVisualState.Default or _ => (colors.ControlDefaultColor, colors.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                _image.color = txt;
                _text.TmpText.color = txt;
            }
        }

        public void UpdateVisual() => TranslateVisual();
    }
}