#nullable enable

using UnityEngine;

namespace Deenote.UIFramework
{
    [CreateAssetMenu(
      fileName = nameof(UIThemeArgs),
      menuName = $"Deenote.UIFramework/{nameof(UIThemeArgs)}")]
    public sealed class UIThemeArgs : ScriptableObject
    {
        public string ThemeName = default!;
        //// https://github.com/microsoft/microsoft-ui-xaml/blob/winui3/release/1.5-stable/controls/dev/CommonStyles/Common_themeresources_any.xaml
        [Header("Text Fill")]
        public Color TextPrimaryColor;   // Rest or Hover // TextFillColorPrimary
        public Color TextSecondaryColor; // Rest Hover
        public Color TextTertiaryColor;  // Pressed
        public Color TextDisabledColor;
        //public Color TextInverseColor;
        [Header("Text Accent Fill")]
        public Color TextAccentSelectedTextColor;
        public Color TextAccentPrimaryColor;      // TextOnAccentFillColorPrimary
        public Color TextAccentSecondaryColor;
        public Color TextAccentDisabledColor;

        [Header("Control Fill")]
        public Color ControlDefaultColor;     // 控件普通状态 // ControlFillColorDefault
        public Color ControlSecondaryColor;   // 控件hover  
        public Color ControlTertiaryColor;    // 控件pressed
        public Color ControlDisabledColor;    // 控件disable
        public Color ControlTransparentColor; // 
        public Color ControlInputActiveColor; // textbox输入 //
        [Header("Control Strong Fill")]
        public Color ControlStrongDefaultColor;  // Slider条背景色 // ControlStrongFillColorDefault
        public Color ControlStrongDisabledColor; // Slider条背景色
        [Header("Control Solid")]
        public Color ControlSolidDefaultColor;   // SliderHandle背景色 // ControlSolidFillColorDefault
        [Header("Control Alt Fill")]
        public Color ControlAltTransparentColor; // ControlAltFillColorTransparent
        public Color ControlAltSecondaryColor;
        public Color ControlAltTertiaryColor;
        public Color ControlAltQuarternaryColor;
        public Color ControlAltDisabledColor;
        [Header("Control Accent Fill")]
        public Color ControlAccentDefaultColor;      // ?d8b0c8 // AccentFillColorDefaultBrush - SystemAccentColorLight2
        public Color ControlAccentSecondaryColor;    // ControlAccentFillDefault * alpha 0.9
        public Color ControlAccentTertiaryColor;     // ControlAccentFillDefault * alpha 0.8
        public Color ControlAccentDisabledColor;     // AccentFillColorDisabled
        public Color ControlAccentSelectedTextColor; // AccentFillColorSelectedTextBackgroundBrush - SystemAccentColor

        [Header("Control Stroke")]
        public Color ControlStrokeDefaultColor;         // 控件边框渐变 // 12FFFFFF // ControlStrokeColorDefault // ControlElevationBorderBrush
        public Color ControlStrokeSecondaryColor;       // 控件边框渐变 // 18FFFFFF
        public Color ControlAccentStrokeDefaultColor;   // Accent控件边框渐变 // ControlStrokeColorOnAccentDefault // AccentControlElevationBorderBrush
        public Color ControlAccentStrokeSecondaryColor; // Accent控件边框渐变
        public Color ControlElevationBorderColor => ControlStrokeDefaultColor; // -> ControlStrokeSecondary
        public Color ControlAccentElevationBorderColor => ControlAccentStrokeDefaultColor;
        public Color TextControlElevationBorderColor => ControlStrongStrokeDefaultColor; // -> ControlStrokeDefault
        public Color TextControlElevationFocusedBorderColor => _textControlElevationFocusedBorderColor; // -> ControlStrokeDefault

        [Header("Control Strong Stroke")]
        public Color ControlStrongStrokeDefaultColor;  // ControlStrongStrokeColorDefault
        public Color ControlStrongStrokeDisabledColor;

        [Header("Surface Stroke")]
        public Color SurfaceStrokeDefaultColor;
        public Color SurfaceStrokeFlyoutColor;  // dialog边框

        [Header("Card Background Fill")]
        public Color CardBackgroundDefaultColor;   // CardBackgroundFillColorDefault
        public Color CardBackgroundSecondaryColor;

        [Header("Layer")]
        public Color LayerDefaultColor;
        public Color LayerAltColor;

        [Header("Solid Background Fill")]
        public Color SolidBackgroundFillBaseColor;
        public Color SolidBackgroundSecondaryColor;
        public Color SolidBackgroundTertiaryColor;
        public Color SolidBackgroundQuarternaryColor;
        public Color SolidBackgroundTransparentColor;
        public Color SolidBackgroundBaseAltColor;

        [Header("System Background")]
        public Color SystemBackgroundCautionColor;

        [Header("Tmp")]
        public Color CautionButtonBackgroundPressed;
        public Color CautionButtonBackgroundHovered;

        [Header("Hard Code Colors")]
        [SerializeField] private Color _textControlElevationFocusedBorderColor; // Dark: SystemAccentColorLight2, Light: SystemAccentColorDark1

        public Color GetColor(UIThemeColor color)
            => color switch {
                UIThemeColor.TextPrimaryColor => TextPrimaryColor,
                UIThemeColor.TextSecondaryColor => TextSecondaryColor,
                UIThemeColor.TextTertiaryColor => TextTertiaryColor,
                UIThemeColor.TextDisabledColor => TextDisabledColor,

                UIThemeColor.TextAccentSelectedTextColor => TextAccentSelectedTextColor,
                UIThemeColor.TextAccentPrimaryColor => TextAccentPrimaryColor,
                UIThemeColor.TextAccentSecondaryColor => TextAccentSecondaryColor,
                UIThemeColor.TextAccentDisabledColor => TextAccentDisabledColor,

                UIThemeColor.ControlDefaultColor => ControlDefaultColor,
                UIThemeColor.ControlSecondaryColor => ControlSecondaryColor,
                UIThemeColor.ControlTertiaryColor => ControlTertiaryColor,
                UIThemeColor.ControlDisabledColor => ControlDisabledColor,
                UIThemeColor.ControlTransparentColor => ControlTransparentColor,
                UIThemeColor.ControlInputActiveColor => ControlInputActiveColor,

                UIThemeColor.ControlStrongDefaultColor => ControlStrongDefaultColor,
                UIThemeColor.ControlStrongDisabledColor => ControlStrongDisabledColor,

                UIThemeColor.ControlSolidDefaultColor => ControlSolidDefaultColor,

                UIThemeColor.ControlAltTransparentColor => ControlAltTransparentColor,
                UIThemeColor.ControlAltSecondaryColor => ControlAltSecondaryColor,
                UIThemeColor.ControlAltTertiaryColor => ControlAltTertiaryColor,
                UIThemeColor.ControlAltQuarternaryColor => ControlAltQuarternaryColor,
                UIThemeColor.ControlAltDisabledColor => ControlAltDisabledColor,

                UIThemeColor.ControlAccentDefaultColor => ControlAccentDefaultColor,
                UIThemeColor.ControlAccentSecondaryColor => ControlAccentSecondaryColor,
                UIThemeColor.ControlAccentTertiaryColor => ControlAccentTertiaryColor,
                UIThemeColor.ControlAccentDisabledColor => ControlAccentDisabledColor,
                UIThemeColor.ControlAccentSelectedTextColor => ControlAccentSelectedTextColor,

                UIThemeColor.ControlStrokeDefaultColor => ControlStrokeDefaultColor,
                UIThemeColor.ControlStrokeSecondaryColor => ControlStrokeSecondaryColor,
                UIThemeColor.ControlAccentStrokeDefaultColor => ControlAccentStrokeDefaultColor,
                UIThemeColor.ControlAccentStrokeSecondaryColor => ControlAccentStrokeSecondaryColor,

                UIThemeColor.ControlStrongStrokeDefaultColor => ControlStrongStrokeDefaultColor,
                UIThemeColor.ControlStrongStrokeDisabledColor => ControlStrongStrokeDisabledColor,

                UIThemeColor.SurfaceStrokeDefaultColor => SurfaceStrokeDefaultColor,
                UIThemeColor.SurfaceStrokeFlyoutColor => SurfaceStrokeFlyoutColor,

                UIThemeColor.CardBackgroundDefaultColor => CardBackgroundDefaultColor,
                UIThemeColor.CardBackgroundSecondaryColor => CardBackgroundSecondaryColor,

                UIThemeColor.LayerDefaultColor => LayerDefaultColor,
                UIThemeColor.LayerAltColor => LayerAltColor,

                UIThemeColor.SolidBackgroundFillBaseColor => SolidBackgroundFillBaseColor,
                UIThemeColor.SolidBackgroundSecondaryColor => SolidBackgroundSecondaryColor,
                UIThemeColor.SolidBackgroundTertiaryColor => SolidBackgroundTertiaryColor,
                UIThemeColor.SolidBackgroundQuarternaryColor => SolidBackgroundQuarternaryColor,
                UIThemeColor.SolidBackgroundTransparentColor => SolidBackgroundTransparentColor,
                UIThemeColor.SolidBackgroundBaseAltColor => SolidBackgroundBaseAltColor,

                UIThemeColor.SystemBackgroundCautionColor => SystemBackgroundCautionColor,

                UIThemeColor.CautionButtonBackgroundPressed => CautionButtonBackgroundPressed,
                UIThemeColor.CautionButtonBackgroundHovered => CautionButtonBackgroundHovered,

                UIThemeColor.ControlElevationBorderColor => ControlElevationBorderColor,
                UIThemeColor.ControlAccentElevationBorderColor => ControlAccentElevationBorderColor,
                UIThemeColor.TextControlElevationBorderColor => TextControlElevationBorderColor,
                UIThemeColor.TextControlElevationFocusedBorderColor => TextControlElevationFocusedBorderColor,

                _ => Color.white,//throw new System.InvalidOperationException("Invalid Color"),
            };
    }
}