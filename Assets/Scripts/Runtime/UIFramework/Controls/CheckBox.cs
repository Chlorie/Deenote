#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
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
                    DoVisualTransition();
                    IsCheckedChanged?.Invoke(value);
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

        public event Action<bool?>? IsCheckedChanged;

        public void SetValueWithoutNotify(bool? value)
        {
            var val = value switch {
                true => SerializabeCheckedState.Checked,
                false => SerializabeCheckedState.Unchecked,
                null => SerializabeCheckedState.Indeterminate,
            };

            if (Utils.SetField(ref _isChecked_bf, val)) {
                DoVisualTransition();
                switch (value) {
                    case true:
                        _checkmarkImage.gameObject.SetActive(true);
                        _checkmarkImage.sprite = UISystem.ThemeArgs.CheckBoxCheckedIcon;
                        break;
                    case null:
                        _checkmarkImage.gameObject.SetActive(true);
                        _checkmarkImage.sprite = UISystem.ThemeArgs.CheckBoxIndeterminateIcon;
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

        protected override void DoStaticVisualTransition()
        {
            _checkmarkImage.color = UISystem.ThemeArgs.TextAccentPrimaryColor;
        }

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            var theme = UISystem.ThemeArgs;

            if (IsChecked ?? true) {
                var (bg, bdr) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (theme.ControlAccentDisabledColor, theme.TextAccentDisabledColor),
                    PressVisualState.Pressed => (theme.ControlAccentTertiaryColor, theme.TextAccentTertiaryColor),
                    PressVisualState.Hovering => (theme.ControlAccentSecondaryColor, theme.TextAccentSecondaryColor),
                    PressVisualState.Default or _ => (theme.ControlAccentDefaultColor, theme.ControlTransparentColor),
                };
                _backgroundImage.color = bg;
                _borderImage.color = bdr;
            }
            else {
                var (bg, bdr) = GetPressVisualState() switch {
                    PressVisualState.Disabled => (theme.ControlDisabledColor, theme.TextDisabledColor),
                    PressVisualState.Pressed => (theme.ControlTertiaryColor, theme.TextTertiaryColor),
                    PressVisualState.Hovering => (theme.ControlSecondaryColor, theme.TextSecondaryColor),
                    PressVisualState.Default or _ => (theme.ControlDefaultColor, theme.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                _borderImage.color = bdr;
            }

            switch (IsChecked) {
                case true:
                    _checkmarkImage.gameObject.SetActive(true);
                    _checkmarkImage.sprite = theme.CheckBoxCheckedIcon;
                    break;
                case null:
                    _checkmarkImage.gameObject.SetActive(true);
                    _checkmarkImage.sprite = theme.CheckBoxIndeterminateIcon;
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
