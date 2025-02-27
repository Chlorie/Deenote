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
        [SerializeField] UnityEngine.UI.Image _backgroundImage = default!;
        //[SerializeField] Image _borderImage = default!; // 找到的颜色好像不对
        [SerializeField] UnityEngine.UI.Image _elevationImage = default!;
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

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = UISystem.ThemeArgs;

            var state = GetFocusVisualState();
            var (elh, el, bg) = state switch {
                FocusVisualState.Disabled => (1, colors.ControlStrokeDefaultColor, colors.ControlDisabledColor),
                FocusVisualState.Focused => (2, colors.TextControlElevationFocusedColor, colors.ControlInputActiveColor),
                FocusVisualState.Hovering => (1, colors.TextControlElevationColor, colors.ControlSecondaryColor),
                FocusVisualState.Default or _ => (1, colors.TextControlElevationColor, colors.ControlDefaultColor),
            };
            _elevationImage.rectTransform.sizeDelta = _elevationImage.rectTransform.sizeDelta with { y = elh };
            _elevationImage.color = el;
            _backgroundImage.color = bg;

            _placeHolderText.TmpText.color = state is FocusVisualState.Disabled ? colors.TextDisabledColor : colors.TextSecondaryColor;
            _inputText.color = state is FocusVisualState.Disabled ? colors.TextDisabledColor : colors.TextPrimaryColor;
        }

        protected override void DoStaticVisualTransition()
        {
            var colors = UISystem.ThemeArgs;
            _inputField.selectionColor = colors.ControlAccentSelectedTextColor;
            //_borderImage.color = colors.ControlStrokeDefaultColor;
        }
    }
}