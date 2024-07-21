using Deenote.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    public sealed class MenuDropdownItemController : MonoBehaviour
    {
        private const float HorizontalOneSidePadding = 12f;

        [SerializeField] MenuItemController _menuItem;
        [Header("UI")]
        [SerializeField] Button _button;
        [SerializeField] LocalizedText _descriptionText;
        [SerializeField] TMP_Text _shortcutText;
        [Header("Layout")]
        [SerializeField] LayoutElement _layoutElement;

        public Button Button => _button;

        private void Awake()
        {
            _button.onClick.AddListener(() => _menuItem.DeselectSelf());
            _descriptionText.OnTextUpdated += text =>
            {
                var shortcutWidth = _shortcutText == null ? 0f : _shortcutText.preferredWidth;
                _layoutElement.preferredWidth = text.TmpText.preferredWidth + shortcutWidth + 4f * HorizontalOneSidePadding;
            };
        }
    }
}
