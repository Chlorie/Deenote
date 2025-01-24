#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.UIFramework
{
    [CreateAssetMenu(
      fileName = nameof(UIThemeResources),
      menuName = $"Deenote.UIFramework/{nameof(UIThemeResources)}")]
    public sealed class UIThemeResources : ScriptableObject
    {
        // https://github.com/microsoft/microsoft-ui-xaml/blob/winui3/release/1.5-stable/controls/dev/CommonStyles/Common_themeresources_any.xaml
        [Header("Solid Background")]
        public Color SolidBottomBackgroundColor; // FF202020
        public Color SolidBottomAlterBackgroundColor; // FF0A0A0A
        public Color SolidContentBackgroundColor; // FF282828

        [Header("Control Fill")]
        public Color ControlDefaultColor;   // 0FFFFFFF
        public Color ControlSecondaryColor; // 15FFFFFF HoverColor
        public Color ControlTertiaryColor; // 08FFFFFF PressColor
        public Color ControlDisabledColor; // 0BFFFFFF
        public Color ControlInputActiveColor; // B31E1E1E
        public Color ControlTransparentColor; // 00FFFFFF
        [Header("Control Alter Fill")]
        public Color ControlAltSecondaryColor; // 19000000 // RestColor toggle off
        public Color ControlAltTertiaryColor; // 0BFFFFFF // Hover toggle
        public Color ControlAltQuarternaryColor; // 12FFFFFF // Pressed toggle
        public Color ControlAltDisabledColor; // 00FFFFFF
        [Header("Control Strong Fill")]
        public Color ControlStrongDefaultColor; // 0BFFFFFF // Slider条的背景色
        public Color ControlStrongDisabledColor; // 3FFFFFFF // Slider条的背景色
        [Header("Control Accent Fill")]
        public Color ControlAccentDefaultColor; // d8b0c8 // 没找到具体颜色，目前是测出来的实心颜色 // Maybe SystemAccentColorLight2
        public Color ControlAccentSecondaryColor; // c5a1b7
        public Color ControlAccentTertiaryColor; // b292a6
        public Color ControlAccentDisabledColor; // 434343
        public Color ControlSolidDefaultColor; // 454545 // 比如Slider的Handle的背景色
        public Color ControlAccentSelectedTextColor; // a3607d

        public Color TextControlElevationColor => ControlStrongStrokeDefaultColor;
        [Header("Control Elevation Stroke")]
        public Color TextControlElevationFocusedColor; // d8b0c8
        [Header("Control Stroke")]
        public Color ControlStrokeDefaultColor; // 12FFFFFF
        public Color ControlStrokeSecondaryColor; // 18FFFFFF
        public Color ControlStrongStrokeDefaultColor; // 8BFFFFFF // TextBox elevation color
        public Color ControlStrongStrokeDisabledColor; // 28FFFFFF
        [Header("Background Surface Stroke")]
        public Color SurfaceStrokeDefaultColor; // 66757575 // Dialog的bordercolor

        [Header("Panel Card Background")]
        public Color CardBackgroundDefaultColor; // 0DFFFFFF Card
        public Color LayerDefaultColor; // 4C3A3A3A // Panel背景色

        [Header("Text")]
        public Color TextPrimaryColor; // FFFFFFFF
        public Color TextSecondaryColor; // C5FFFFFF
        public Color TextTertiaryColor; // 87FFFFFF
        public Color TextDisabledColor; // 5DFFFFFF
        [Header("Text Accent")]
        public Color TextAccentPrimaryColor; // FF000000
        public Color TextAccentSecondaryColor; // 80000000
        public Color TextAccentTertiaryColor; // d8b0c8 // 没找到具体颜色，这和AccentDefaultColor是一个颜色？
        public Color TextAccentDisabledColor; // 87FFFFFF

        [Header("Signal")]
        public Color SignalCautionBackgroundColor; // 433519

        [Header("Tmp")]
        public Color CautionButtonBackgroundHoverColor; // e81123
        public Color CautionButtonBackgroundPressedColor; // f1707a

        [Header("Icons")]
        [Header("CheckBox")]
        public Sprite CheckBoxCheckedIcon = default!;
        public Sprite CheckBoxIndeterminateIcon = default!;

        [Header("Prefabs")]
        public DropdownItem DropdownItemPrefab = default!;
    }
}