using Deenote.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    public sealed class MenuDropDownItemController : MonoBehaviour
    {
        [SerializeField] MenuItemController _menuItem;
        [Header("UI")]
        [SerializeField] Button _button;
        [SerializeField] LocalizedText _descriptionText;
        [SerializeField] TMP_Text _shortcutText;
        [Header("Layout")]
        [SerializeField] LayoutElement _layoutElement;
        [SerializeField] RectTransform _descriptionTextTransform;
        [SerializeField] RectTransform _shortcutTextTransform;

        private void Awake()
        {
            _button.onClick.AddListener(_menuItem.Collapse);
        }
    }
}
