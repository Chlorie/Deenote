#nullable enable

using Deenote.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class CheckBox : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _checkmarkImage = default!;

#if UNITY_EDITOR
        [SerializeField]
        private SerializabeCheckedState _isChecked_bf;
#else
        private bool? _isChecked_bf;
#endif

        public bool? IsChecked
        {
#if UNITY_EDITOR
            get => _isChecked_bf switch {
                SerializabeCheckedState.Unchecked => false,
                SerializabeCheckedState.Checked => true,
                _ => null,
            };
            set {
                var val = value switch {
                    true => SerializabeCheckedState.Checked,
                    false => SerializabeCheckedState.Unchecked,
                    null => SerializabeCheckedState.Indeterminate,
                };

                if (Utils.SetField(ref _isChecked_bf, val)) {
                    TranslateVisual();
                    OnValueChanged.Invoke(value);
                }
            }
#else
            get => _isChecked_bf;
            set {
                if(Utils.SetField(ref _isChecked_bf, value)) {
                    TranslateVisual();
                    OnValueChanged.Invoke(value);
                }
            }
#endif
        }

        [Obsolete("新ui用Ischecked")]
        public bool? Value
        {
            get => IsChecked;
            set => IsChecked = value;
        }

        public UnityEvent<bool?> OnValueChanged { get; } = new();

        public void SetValueWithoutNotify(bool? value)
        {
            var val = value switch {
                true => SerializabeCheckedState.Checked,
                false => SerializabeCheckedState.Unchecked,
                null => SerializabeCheckedState.Indeterminate,
            };

            if (Utils.SetField(ref _isChecked_bf, val)) {
                TranslateVisual();
                switch (value) {
                    case true:
                        _checkmarkImage.gameObject.SetActive(true);
                        _checkmarkImage.sprite = MainSystem.Args.UIIcons.CheckBoxChecked;
                        break;
                    case null:
                        _checkmarkImage.gameObject.SetActive(true);
                        _checkmarkImage.sprite = MainSystem.Args.UIIcons.CheckBoxIndeterminate;
                        break;
                    case false:
                        _checkmarkImage.gameObject.SetActive(false);
                        break;
                }
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                IsChecked = !(IsChecked ?? false);
            }
        }

        protected override void TranslateStaticVisual()
        {
            _checkmarkImage.color = MainSystem.Args.UIColors.TextAccentPrimaryColor;
        }

        protected override void TranslateVisual()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = MainSystem.Args.UIColors;

            if (IsChecked ?? true) {
                var (bg, bdr) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (colors.ControlAccentDisabledColor, colors.TextAccentDisabledColor),
                    PressVisualState.Pressed => (colors.ControlAccentTertiaryColor, colors.TextAccentTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlAccentSecondaryColor, colors.TextAccentSecondaryColor),
                    PressVisualState.Default or _ => (colors.ControlAccentDefaultColor, colors.ControlTransparentColor),
                };
                _backgroundImage.color = bg;
                _borderImage.color = bdr;
            }
            else {
                var (bg, bdr) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (colors.ControlDisabledColor, colors.TextDisabledColor),
                    PressVisualState.Pressed => (colors.ControlTertiaryColor, colors.TextTertiaryColor),
                    PressVisualState.Hovering => (colors.ControlSecondaryColor, colors.TextSecondaryColor),
                    PressVisualState.Default or _ => (colors.ControlDefaultColor, colors.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                _borderImage.color = bdr;
            }

            switch (IsChecked) {
                case true:
                    _checkmarkImage.gameObject.SetActive(true);
                    _checkmarkImage.sprite = MainSystem.Args.UIIcons.CheckBoxChecked;
                    break;
                case null:
                    _checkmarkImage.gameObject.SetActive(true);
                    _checkmarkImage.sprite = MainSystem.Args.UIIcons.CheckBoxIndeterminate;
                    break;
                case false:
                    _checkmarkImage.gameObject.SetActive(false);
                    break;
            }

        }

#if UNITY_EDITOR
        private enum SerializabeCheckedState { Unchecked, Checked, Indeterminate, }
#endif
    }
}
