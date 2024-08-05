using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    partial class MenuBarController
    {
        [Header("Values")]
        [SerializeField] private Color _menuItemNormalColor;
        [SerializeField] private Color _menuItemHightlightedColor;
        [SerializeField] private Color _menuItemPressedColor;
        [SerializeField] private Color _menuItemSelectedColor;

        public ColorBlock UnselectedMenuItemColors => new()
        {
            normalColor = _menuItemNormalColor,
            highlightedColor = _menuItemHightlightedColor,
            pressedColor = _menuItemPressedColor,
            selectedColor = _menuItemNormalColor,
            colorMultiplier = 1f,
        };

        public ColorBlock SelectedMenuItemColors => new()
        {
            normalColor = _menuItemSelectedColor,
            highlightedColor = _menuItemHightlightedColor,
            pressedColor = _menuItemPressedColor,
            selectedColor = _menuItemSelectedColor,
            colorMultiplier = 1f,
        };
    }
}