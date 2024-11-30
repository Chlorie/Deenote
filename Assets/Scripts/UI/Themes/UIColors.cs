#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Themes
{
    [CreateAssetMenu(
        fileName = nameof(UIColors),
        menuName = $"{nameof(Deenote)}/UI/{nameof(UIColors)}")]
    public sealed class UIColors : ScriptableObject
    {
        // https://github.com/microsoft/microsoft-ui-xaml/blob/winui3/release/1.5-stable/controls/dev/CommonStyles/Common_themeresources_any.xaml
        public Color SolidBottomBackgroundColor; // FF202020
        public Color SolidBottomAlterBackgroundColor; // FF0A0A0A
        public Color SolidContentBackgroundColor; // FF282828
        public Color ControlDefaultColor;   // 0FFFFFFF
        public Color ControlSecondaryColor; // 15FFFFFF HoverColor
        public Color ControlTertiaryColor; // 08FFFFFF PressColor
        public Color ControlTransparentColor; // 00FFFFFF
        public Color ControlAlterSecondaryColor; // 19000000 // RestColor toggle off
        public Color ControlAlterTertiaryColor; // 0BFFFFFF // Hover toggle
        public Color ControlAlterQuarternaryColor; // 12FFFFFF // Pressed toggle
        public Color ControlSolidDefaultColor; // 454545 // 比如Slider的Handle的背景色
        public Color SurfaceStrokeDefaultColor; // 66757575 // Dialog的bordercolor

        public Color AccentDefaultColor; // d8b0c8 // 没找到具体颜色，目前是测出来的实心颜色
        public Color AccentSecondaryColor; // c5a1b7
        public Color AccentTertiaryColor; // b292a6

        public Color CardBackgroundDefaultColor; // 0DFFFFFF Card
        public Color LayerDefaultColor; // 4C3A3A3A // Panel背景色

        public Color TextControlElevationColor; // 9A9A9A height 1 // 没找到具体颜色，目前是测出来的实心颜色
        public Color TextControlElevationFocusedColor; // D8B0C8 height 2

        public Color TextDefaultColor; // FFFFFFFF
        public Color TextSecondaryColor; // C5FFFFFF
        public Color TextTertiaryColor; // 87FFFFFF
        public Color AccentTextDefaultColor; // FF000000
        public Color AccentTextSecondaryColor; // 80000000
        public Color AccentTextTertiaryColor; // d8b0c8 // 没找到具体颜色，这和AccentDefaultColor是一个颜色？

        // 暂命名
        public Color TextPlaceHolderColor; // C5c5c5

        [Header("Legacy")]
        public Color WindowColor;
        public Color BackgroundColor;
        public Color HighlightedColor;
        public Color SelectedColor;
        public Color ForegroundColor;

        public Color TransparentBackgroundColor;
        public Color TransparentHighlightedColor;
        public Color TransparentSelectedColor;
        public Color TransparentForegroundColor;

        //public Color TextPlaceHolderColor;

        public void ApplyWindowColor(Image windowBackgroundImage)
        {
            windowBackgroundImage.color = WindowColor;
        }

        private void OnValidate()
        {
        }
    }
}