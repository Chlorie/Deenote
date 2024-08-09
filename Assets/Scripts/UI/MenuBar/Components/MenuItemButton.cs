using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar.Components
{
    public sealed class MenuItemButton : Selectable
    {
        private MenuItemController _controller;

        public void InitController(MenuItemController controller) => _controller = controller;

        // Set selected object as MenuItemController, 
        // Do not directly select this button, this button is only for visual display
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (_controller.MenuBar.IsHovering) {
                _controller.DeselectSelf();
            }
            else {
                EventSystem.current.CheckNull()?.SetSelectedGameObject(_controller.gameObject);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData) { }
        public void OnPointerEnter_Base(PointerEventData eventData) => base.OnPointerEnter(eventData);

        public override void OnPointerExit(PointerEventData eventData) { }
        public void OnPointerExit_Base(PointerEventData eventData) => base.OnPointerExit(eventData);

    }
}
