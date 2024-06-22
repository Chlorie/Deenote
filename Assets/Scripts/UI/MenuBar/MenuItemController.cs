using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar
{
    public sealed class MenuItemController : MonoBehaviour, IPointerEnterHandler
    {
        [Header("UI")]
        [SerializeField] MenuBarController _menuBar;
        [SerializeField] Toggle _toggle;
        [SerializeField] GameObject _menuDropDownParentGameObject;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool value)
        {
            if (value) {
                _menuDropDownParentGameObject.SetActive(true);
                _menuBar.IsHovering = true;
            }
            else {
                _menuDropDownParentGameObject.SetActive(false);
                _menuBar.IsHovering = false;
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_menuBar.IsHovering && !_toggle.isOn) {
                _toggle.Select();
                _toggle.isOn = true;
            }
        }

        public void Collapse()
        {
            _toggle.isOn = false;
        }
    }
}
