#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
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
                if (Utils.SetField(ref _isChecked_bf, value)) {
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
                        _checkmarkImage.sprite = UISystem.ThemeResources.CheckBoxCheckedIcon;
                        break;
                    case null:
                        _checkmarkImage.gameObject.SetActive(true);
                        _checkmarkImage.sprite = UISystem.ThemeResources.CheckBoxIndeterminateIcon;
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

        protected override void DoVisualTransition(UIThemeArgs args, PressVisualState state)
        {
            Color bg, fg, bdr;
            if (IsChecked == false) {
                (bg, fg, bdr) = state switch {
                    PressVisualState.Disabled => (args.ControlDisabledColor, args.TextDisabledColor, args.ControlStrongStrokeDisabledColor),
                    PressVisualState.Pressed => (args.ControlTertiaryColor, args.TextPrimaryColor, args.ControlStrongStrokeDisabledColor),
                    PressVisualState.Hovering => (args.ControlSecondaryColor, args.TextPrimaryColor, args.ControlStrongStrokeDefaultColor),
                    PressVisualState.Default or _ => (args.ControlDefaultColor, args.TextPrimaryColor, args.ControlStrongStrokeDefaultColor),
                };
            }
            else {
                (bg, fg, bdr) = state switch {
                    PressVisualState.Disabled => (args.ControlAccentDisabledColor, args.TextAccentDisabledColor, args.ControlStrongStrokeDisabledColor),
                    PressVisualState.Pressed => (args.ControlAccentTertiaryColor, args.TextAccentSecondaryColor, args.ControlAccentTertiaryColor),
                    PressVisualState.Hovering => (args.ControlAccentSecondaryColor, args.TextAccentPrimaryColor, args.ControlSecondaryColor),
                    PressVisualState.Default or _ => (args.ControlAccentDefaultColor, args.TextAccentPrimaryColor, args.ControlAccentDefaultColor),
                };
            }
            _backgroundImage.color = bg;
            _borderImage.color = bdr;
            _checkmarkImage.color = fg;

            var theme = UISystem.ThemeResources;

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