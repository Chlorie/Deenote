using Deenote.Localization;
using Deenote.UI.MenuBar.Components;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.MenuBar
{
    public sealed class MenuItemController : MonoBehaviour, IPointerEnterHandler
    {
        [Header("UI")]
        [SerializeField] MenuBarController _menuBar;
        [SerializeField] MenuItemToggle _toggle;
        [SerializeField] GameObject _menuDropDownParentGameObject;

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool value)
        {
            if (value) {
                Debug.Log($"Toggle on:{_toggle.gameObject.GetComponentInChildren<LocalizedText>().TmpText.text}");
                _menuDropDownParentGameObject.SetActive(true);
                var colors = _toggle.colors;
                colors.normalColor = colors.normalColor.WithAlpha(1f);
                colors.selectedColor = colors.selectedColor.WithAlpha(1f);
                _toggle.colors = colors;
                _menuBar.IsHovering = true;
            }
            else {
                Debug.Log($"Toggle off:{_toggle.gameObject.GetComponentInChildren<LocalizedText>().TmpText.text}");
                _menuDropDownParentGameObject.SetActive(false);
                var colors = _toggle.colors;
                colors.normalColor = colors.normalColor.WithAlpha(0f);
                colors.selectedColor = colors.selectedColor.WithAlpha(0f);
                _toggle.colors = colors;
                _menuBar.IsHovering = false;
                // TODO: 手动取消选择后，toggle的状态是Selected，导致没有highlight
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
