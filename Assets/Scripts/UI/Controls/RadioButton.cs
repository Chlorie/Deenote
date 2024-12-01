#nullable enable

using Deenote.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class RadioButton : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _handleImage = default!;

        [SerializeField] RadioButtonGroup? _group;
        public RadioButtonGroup Group => _group!;

        [SerializeField]
        private bool isChecked_bf;
        public bool IsChecked => isChecked_bf;

        public override bool IsInteractable
        {
            get => base.IsInteractable && (Group?.IsInteractable ?? true);
            set => base.IsInteractable = value;
        }

        public UnityEvent OnChecked { get; } = new();

        internal void SetIsChecked(bool value)
        {
            if (Utils.SetField(ref isChecked_bf, value)) {
                TranslateVisual();

                Group.SelectCheckedRadio(this);
                OnChecked.Invoke();
            }
        }

        protected override void Awake()
        {
            if (_group == null) {
                _group = RadioButtonGroup.Shared;
            }
            _group.AddButton(this);
            base.Awake();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                SetIsChecked(true);
            }
        }

        protected override void TranslateVisual()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = MainSystem.Args.UIColors;

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

        public void UpdateVisual() => TranslateVisual();
    }
}
