using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Themes
{
    [CreateAssetMenu(
        fileName = nameof(UIColorTheme),
        menuName = $"{nameof(Deenote)}/UIThemes/{nameof(UIColorTheme)}")]
    public sealed class UIColorTheme : ScriptableObject
    {
        public Color WindowColor;
        public Color BackgroundColor;
        public Color HighlightedColor;
        public Color SelectedColor;
        public Color ForegroundColor;

        public Color TransparentBackgroundColor;
        public Color TransparentHighlightedColor;
        public Color TransparentSelectedColor;
        public Color TransparentForegroundColor;

        public Color TextPlaceHolderColor;

        public void ApplyWindowColor(Image windowBackgroundImage)
        {
            windowBackgroundImage.color = WindowColor;
        }

        private void OnValidate()
        {
        }
    }
}