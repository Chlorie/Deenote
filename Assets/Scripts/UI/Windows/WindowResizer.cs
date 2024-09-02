using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Windows
{
    public sealed class WindowResizer : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
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
                    delta = _window.IsFixedAspectRatio
                        ? delta.WithY(delta.x / _window.FixedAspectRatio)
                        : delta.WithY(0f);
                    rectTransform.offsetMin += delta;
                    break;
                case ResizeDirection.UpLeft:
                    if (_window.IsFixedAspectRatio) {
                        delta = delta.WithY(-delta.x / _window.FixedAspectRatio);
                    }
                    rectTransform.offsetMin += delta.WithY(0f);
                    rectTransform.offsetMax += delta.WithX(0f);
                    break;
                case ResizeDirection.Up:
                    delta = _window.IsFixedAspectRatio
                        ? delta.WithX(delta.y * _window.FixedAspectRatio)
                        : delta.WithX(0);
                    rectTransform.offsetMax += delta;
                    break;
                case ResizeDirection.UpRight:
                    if (_window.IsFixedAspectRatio) {
                        delta = delta.WithY(delta.x / _window.FixedAspectRatio);
                    }
                    rectTransform.offsetMax += delta;
                    break;
                case ResizeDirection.Right:
                    delta = _window.IsFixedAspectRatio
                        ? delta.WithY(-delta.x / _window.FixedAspectRatio)
                        : delta.WithY(0f);
                    rectTransform.offsetMin += delta.WithX(0f);
                    rectTransform.offsetMax += delta.WithY(0f);
                    break;
                case ResizeDirection.DownRight:
                    if (_window.IsFixedAspectRatio) {
                        delta = delta.WithY(-delta.x / _window.FixedAspectRatio);
                    }
                    rectTransform.offsetMin += delta.WithX(0f);
                    rectTransform.offsetMax += delta.WithY(0f);
                    break;
                case ResizeDirection.Down:
                    delta = _window.IsFixedAspectRatio
                        ? delta.WithX(-delta.y * _window.FixedAspectRatio)
                        : delta.WithX(0);
                    rectTransform.offsetMin += delta.WithX(0f);
                    rectTransform.offsetMax += delta.WithY(0f);
                    break;
                case ResizeDirection.DownLeft:
                    if (_window.IsFixedAspectRatio) {
                        delta = delta.WithY(delta.x / _window.FixedAspectRatio);
                    }
                    rectTransform.offsetMin += delta;
                    break;
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            var args = MainSystem.WindowsManager.CursorsArgs;
            switch (_direction) {
                case ResizeDirection.Left:
                case ResizeDirection.Right:
                    Cursor.SetCursor(args.HorizontalCursor, args.HorizontalCursorHotspot, CursorMode.Auto);
                    break;
                case ResizeDirection.UpLeft:
                case ResizeDirection.DownRight:
                    Cursor.SetCursor(args.DiagonalCursor, args.DiagonalCursorHotspot, CursorMode.Auto);
                    break;
                case ResizeDirection.Up:
                case ResizeDirection.Down:
                    Cursor.SetCursor(args.VerticalCursor, args.VerticalCursorHotspot, CursorMode.Auto);
                    break;
                case ResizeDirection.UpRight:
                case ResizeDirection.DownLeft:
                    Cursor.SetCursor(args.AntiDiagonalCursor, args.AntiDiagonalCursorHotspot, CursorMode.Auto);
                    break;
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            var args = MainSystem.WindowsManager.CursorsArgs;
            Cursor.SetCursor(args.DefaultCursor, args.DefaultCursorHotspot, CursorMode.Auto);
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