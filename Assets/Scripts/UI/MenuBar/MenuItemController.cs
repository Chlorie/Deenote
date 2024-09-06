using Deenote.Localization;
using Deenote.UI.MenuBar.Components;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    public sealed class MenuItemController : MonoBehaviour
        , ISelectHandler, IDeselectHandler
        , IPointerEnterHandler, IPointerExitHandler
    {
        private const float HorizontalOneSidePadding = 12f;

        [Header("UI")]
        [SerializeField] MenuBarController _menuBar;
        [SerializeField] MenuItemButton _button;
        [SerializeField] LayoutElement _layoutElement;
        [SerializeField] LocalizedText _titleText;
        [SerializeField] GameObject _menuDropDownParentGameObject;

        public MenuBarController MenuBar => _menuBar;

        public bool IsPointerIn { get; private set; }

        private void Awake()
        {
            _titleText.OnTextUpdated += text =>
                _layoutElement.preferredWidth = text.Text.preferredWidth + 2 * HorizontalOneSidePadding;
            _button.InitController(this);
        }

        public void DeselectSelf()
        {
            EventSystem.current.CheckNull()?.SetSelectedGameObject(null);
            _button.OnDeselect(null);
            MenuBar.IsHovering = false;
            _menuDropDownParentGameObject.SetActive(false);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsPointerIn = true;
            if (MenuBar.IsHovering) {
                _button.OnPointerEnter_Base(eventData);
                EventSystem.current.CheckNull()?.SetSelectedGameObject(gameObject);
            }
            else {
                _button.OnPointerEnter_Base(eventData);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsPointerIn = false;
            _button.OnPointerExit_Base(eventData);
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            _button.OnSelect(eventData);
            MenuBar.IsHovering = true;
            _menuDropDownParentGameObject.SetActive(true);
        }

        // This method is for conditions that
        // user clicked on other position outside MenuItem.
        // For manually deselecting, use DeselectSelf();
        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (!IsPointerIn) {
                _button.OnDeselect(eventData);
                MenuBar.IsHovering = false;
                _menuDropDownParentGameObject.SetActive(false);
            }
        }
    }
}