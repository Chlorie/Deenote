#nullable enable

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
        [SerializeField] Image _borderImage = default!;
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
                    DoVisualTransition(UISystem.ColorArgs);
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
                DoVisualTransition(UISystem.ColorArgs);
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

        protected override void DoVisualTransition(UIThemeColorArgs args, PressVisualState state)
        {
            Color bg, fg, bdr;
            if (IsChecked) {
                (bg, fg, bdr) = state switch {
                    PressVisualState.Disabled => (args.ControlAccentDisabledColor, args.TextAccentDisabledColor, args.ControlTransparentColor),
                    PressVisualState.Pressed => (args.ControlAccentTertiaryColor, args.TextAccentSecondaryColor, args.ControlTransparentColor),
                    PressVisualState.Hovering => (args.ControlAccentSecondaryColor, args.TextAccentPrimaryColor, args.ControlAccentElevationBorderColor),
                    PressVisualState.Default or _ => (args.ControlAccentDefaultColor, args.TextAccentPrimaryColor, args.ControlAccentElevationBorderColor),
                };
            }
            else {
                switch (_colorSet) {
                    case ButtonColorSet.Transparent:
                        (bg, fg, bdr) = state switch {
                            PressVisualState.Disabled => (args.ControlDisabledColor, args.TextDisabledColor, args.ControlTransparentColor),
                            PressVisualState.Pressed => (args.ControlTertiaryColor, args.TextTertiaryColor, args.ControlStrokeDefaultColor),
                            PressVisualState.Hovering => (args.ControlSecondaryColor, args.TextSecondaryColor, args.ControlElevationBorderColor),
                            PressVisualState.Default or _ => (args.ControlTransparentColor, args.TextPrimaryColor, args.ControlTransparentColor),
                        };
                        break;
                    case ButtonColorSet.Default:
                    default:
                        (bg, fg, bdr) = state switch {
                            PressVisualState.Disabled => (args.ControlDisabledColor, args.TextDisabledColor, args.ControlStrokeDefaultColor),
                            PressVisualState.Pressed => (args.ControlTertiaryColor, args.TextTertiaryColor, args.ControlStrokeDefaultColor),
                            PressVisualState.Hovering => (args.ControlSecondaryColor, args.TextSecondaryColor, args.ControlElevationBorderColor),
                            PressVisualState.Default or _ => (args.ControlDefaultColor, args.TextPrimaryColor, args.ControlElevationBorderColor),
                        };
                        break;
                }
            }

            _backgroundImage.color = bg;
            _text.TmpText.color = _image.color = fg;
            _borderImage.color = bdr;
        }

        public void UpdateVisual() => DoVisualTransition(UISystem.ColorArgs);

        private enum ButtonColorSet
        {
            Default,
            Transparent,
        }
    }
}