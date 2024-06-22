using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    partial class MenuBarController
    {
        [Header("Values")]
        [SerializeField] Color _menuItemNormalColor;
        [SerializeField] Color _menuItemHightlightedColor;
        [SerializeField] Color _menuItemPressedColor;
        [SerializeField] Color _menuItemSelectedColor;

        public ColorBlock UnselectedMenuItemColors => new ColorBlock {
            normalColor = _menuItemNormalColor,
            highlightedColor = _menuItemHightlightedColor,
            pressedColor = _menuItemPressedColor,
            selectedColor = _menuItemNormalColor,
            colorMultiplier = 1f,
        };

        public ColorBlock SelectedMenuItemColors => new ColorBlock {
            normalColor = _menuItemSelectedColor,
            highlightedColor = _menuItemHightlightedColor,
            pressedColor = _menuItemPressedColor,
            selectedColor = _menuItemSelectedColor,
            colorMultiplier = 1f,
        };
    }
}