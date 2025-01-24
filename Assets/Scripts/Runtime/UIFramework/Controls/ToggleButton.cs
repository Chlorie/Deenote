#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed class ToggleButton : UIPressableControlBase, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _image = default!;
        [SerializeField] TextBlock _text = default!;
        [SerializeField] ButtonColorSet _colorSet;

        [Header("Control")]
        [SerializeField] ToggleButtonGroup? _group;
        public ToggleButtonGroup? Group => _group;

        [SerializeField]
        private bool _isChecked_bf;

        public bool IsChecked => _isChecked_bf;

        public event Action<bool>? IsCheckedChanged;

        /// <remarks>
        /// If trying to toggle off a button in a group that does not allow toggle off,
        /// the button may still not be toggled off.
        /// </remarks>
        public void SetIsChecked(bool value)
        {
            if (Group is null)
                SetIsCheckedInternal(value);
            else {
                if (IsChecked != value)
                    Group.Toggle(this);
            }
        }

        /// <remarks>
        /// If trying to toggle off a button in a group that does not allow toggle off,
        /// the button may still not be toggled off.
        /// </remarks>
        public void SetIsCheckedWithoutNotify(bool value)
        {
            if (Group is null) {
                if (Utils.SetField(ref _isChecked_bf, value)) {
                    DoVisualTransition();
                }
            }
            else {
                if (IsChecked != value)
                    Group.Toggle(this);
            }
        }

        internal void SetIsCheckedInternal(bool value)
        {
            if (Utils.SetField(ref _isChecked_bf, value)) {
                DoVisualTransition();
                IsCheckedChanged?.Invoke(value);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsLeftButtonOnInteractableControl(eventData)) {
                if (Group is null)
                    SetIsCheckedInternal(!IsChecked);
                else
                    Group.Toggle(this);
            }
        }

        protected override void DoVisualTransition()
        {
            if (!isActiveAndEnabled)
                return;

            var colors = UISystem.ThemeArgs;

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
                    PressVisualState.Default or _ => (_colorSet is ButtonColorSet.Transparent
                        ? colors.ControlTransparentColor : colors.ControlDefaultColor, colors.TextPrimaryColor),
                };
                _backgroundImage.color = bg;
                _image.color = txt;
                _text.TmpText.color = txt;
            }
        }

        public void UpdateVisual() => DoVisualTransition();

        private enum ButtonColorSet
        {
            Default,
            Transparent,
        }
    }
}