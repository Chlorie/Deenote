#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Dropdown = Deenote.UIFramework.Controls.Dropdown;

namespace Deenote.UIFramework
{
    internal sealed class DropdownExpandBlocker : MonoBehaviour, IPointerClickHandler
    {
        private static DropdownExpandBlocker? _instance;

        public static DropdownExpandBlocker Instance
        {
            get {
                if (_instance is null) {
                    var go = new GameObject(nameof(DropdownExpandBlocker));
                    var canvas = go.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 100;
                    var scaler = go.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    var raycaster = go.AddComponent<GraphicRaycaster>();
                    go.transform.SetAsLastSibling();

                    var imggo = new GameObject("Img");
                    imggo.AddComponent<CanvasRenderer>();
                    var img = imggo.AddComponent<Image>();
                    img.color = Color.clear;
                    img.rectTransform.SetParent(go.transform, false);
                    img.rectTransform.anchorMin = Vector2.zero;
                    img.rectTransform.anchorMax = Vector2.one;
                    img.rectTransform.offsetMin = Vector2.zero;
                    img.rectTransform.offsetMax = Vector2.zero;

                    _instance = imggo.AddComponent<DropdownExpandBlocker>();
                    _instance._raycaster = raycaster;
                }
                return _instance;
            }
        }

        private GraphicRaycaster _raycaster = default!;
        private Dropdown? _activeDropdown;

        internal void EnableBlockFor(Dropdown dropdown)
        {
            _activeDropdown = dropdown;
            _raycaster.enabled = true;
        }

        internal void DisableBlocker()
        {
            _activeDropdown = null;
            _raycaster.enabled = false;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (_activeDropdown is null)
                return;

            _activeDropdown.IsExpanded = false;
            _activeDropdown = null;
            _raycaster.enabled = false;
        }
    }
}