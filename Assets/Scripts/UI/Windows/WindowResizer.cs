using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Windows
{
    public sealed class WindowResizer : MonoBehaviour, IDragHandler
    {
        [SerializeField] Window _window;
        [SerializeField] ResizeDirection _direction;

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (_direction is ResizeDirection.None)
                return;

            var pointerPos = eventData.position;
            Vector2 delta = default;
            if (pointerPos.x >= 0 && pointerPos.x <= Screen.width) {
                delta.x += eventData.delta.x;
            }
            if (pointerPos.y >= 0 && pointerPos.y <= Screen.height) {
                delta.y += eventData.delta.y;
            }

            // TODO: scale of recttransform doesnt match delta
            var rectTransform = (RectTransform)_window.transform;
            switch (_direction) {
                case ResizeDirection.Left:
                    delta = _window.IsFixedRatio
                        ? delta.WithY(delta.x / _window.FixedRatio)
                        : delta.WithY(0f);
                    rectTransform.offsetMin += delta;
                    break;
                case ResizeDirection.UpLeft:
                    if (_window.IsFixedRatio) {
                        delta = delta.WithY(-delta.x / _window.FixedRatio);
                    }
                    rectTransform.offsetMin += delta.WithY(0f);
                    rectTransform.offsetMax += delta.WithX(0f);
                    break;
                case ResizeDirection.Up:
                    delta = _window.IsFixedRatio
                        ? delta.WithX(delta.y * _window.FixedRatio)
                        : delta.WithX(0);
                    rectTransform.offsetMax += delta;
                    break;
                case ResizeDirection.UpRight:
                    if (_window.IsFixedRatio) {
                        delta = delta.WithY(delta.x / _window.FixedRatio);
                    }
                    rectTransform.offsetMax += delta;
                    break;
                case ResizeDirection.Right:
                    delta = _window.IsFixedRatio
                        ? delta.WithY(-delta.x / _window.FixedRatio)
                        : delta.WithY(0f);
                    rectTransform.offsetMin += delta.WithX(0f);
                    rectTransform.offsetMax += delta.WithY(0f);
                    break;
                case ResizeDirection.DownRight:
                    if (_window.IsFixedRatio) {
                        delta = delta.WithY(-delta.x/ _window.FixedRatio);
                    }
                    rectTransform.offsetMin += delta.WithX(0f);
                    rectTransform.offsetMax += delta.WithY(0f);
                    break;
                case ResizeDirection.Down:
                    delta = _window.IsFixedRatio
                        ? delta.WithX(-delta.y * _window.FixedRatio)
                        : delta.WithX(0);
                    rectTransform.offsetMin += delta.WithX(0f);
                    rectTransform.offsetMax += delta.WithY(0f);
                    break;
                case ResizeDirection.DownLeft:
                    if (_window.IsFixedRatio) {
                        delta=delta.WithY(delta.x / _window.FixedRatio);
                    }
                    rectTransform.offsetMin += delta;
                    break;
            }
        }

        public enum ResizeDirection
        {
            None,
            Left,
            UpLeft,
            Up,
            UpRight,
            Right,
            DownRight,
            Down,
            DownLeft,
        }
    }
}
