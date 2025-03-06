#nullable enable

using Deenote.Localization;
using System;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class TextBox : UIFocusableControlBase
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _elevationImage = default!;
        [SerializeField] TMP_InputField _inputField = default!;
        [SerializeField] TextBlock _placeHolderText = default!;
        [SerializeField] TMP_Text _inputText = default!;

        public string Value
        {
            get => _inputField.text;
            set => _inputField.text = value;
        }

        public override bool IsInteractable
        {
            get {
                return base.IsInteractable;
            }
            set {
                base.IsInteractable = value;
                _inputField.interactable = value;
            }
        }

        public event Action<string>? EditSubmitted;
        public event Action<string>? ValueChanged;

        public void SetValueWithoutNotify([AllowNull] string value)
            => _inputField.SetTextWithoutNotify(value!);

        public void SetPlaceHolderText(LocalizableText text)
            => _placeHolderText.SetText(text);

        protected override void Awake()
        {
            base.Awake();
            _inputField.onEndEdit.AddListener(val =>
            {
                Unfocus();
                EditSubmitted?.Invoke(val);
            });
            _inputField.onValueChanged.AddListener(val => ValueChanged?.Invoke(val));
        }

        protected override void DoVisualTransition(UIThemeArgs args, FocusVisualState state)
        {
            Color el, bg, bdr;

            (el, bg, bdr) = state switch {
                FocusVisualState.Disabled => (args.ControlStrokeDefaultColor, args.ControlDisabledColor, args.ControlStrokeDefaultColor),
                FocusVisualState.Focused => (args.TextControlElevationFocusedBorderColor, args.ControlInputActiveColor, args.ControlStrokeDefaultColor),
                FocusVisualState.Hovering => (args.TextControlElevationBorderColor, args.ControlSecondaryColor, args.ControlElevationBorderColor),
                FocusVisualState.Default or _ => (args.TextControlElevationBorderColor, args.ControlDefaultColor, args.ControlElevationBorderColor),
            };
            _elevationImage.rectTransform.sizeDelta = _elevationImage.rectTransform.sizeDelta with {
                y = state is FocusVisualState.Focused ? 2f : 1f,
            };
            _elevationImage.color = el;
            _backgroundImage.color = bg;
            _borderImage.color = bdr;

            if (state is FocusVisualState.Disabled) {
                _placeHolderText.TmpText.color = args.TextDisabledColor;
                _inputText.color = args.TextDisabledColor;
            }
            else {
                _placeHolderText.TmpText.color = args.TextSecondaryColor;
                _inputText.color = args.TextPrimaryColor;
            }
        }

        protected override void OnThemeChanged(UIThemeArgs args)
        {
            base.OnThemeChanged(args);
            _inputField.selectionColor = args.ControlAccentSelectedTextColor;
        }
    }
}