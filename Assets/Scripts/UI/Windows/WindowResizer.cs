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
                    rectTransform.offsetMin += delta with {
                        y = _window.IsFixedAspectRatio ? delta.x / _window.FixedAspectRatio : 0f,
                    };
                    break;
                case ResizeDirection.UpLeft:
                    rectTransform.offsetMin += delta with { y = 0f };
                    rectTransform.offsetMax += _window.IsFixedAspectRatio
                        ? delta with { x = 0f, y = -delta.x / _window.FixedAspectRatio, }
                        : delta with { x = 0f };
                    break;
                case ResizeDirection.Up:
                    rectTransform.offsetMax += delta with {
                        x = _window.IsFixedAspectRatio ? delta.y * _window.FixedAspectRatio : 0f,
                    };
                    break;
                case ResizeDirection.UpRight:
                    rectTransform.offsetMax += _window.IsFixedAspectRatio
                        ? delta with { y = delta.x / _window.FixedAspectRatio }
                        : delta;
                    break;
                case ResizeDirection.Right:
                    if (_window.IsFixedAspectRatio) {
                        rectTransform.offsetMin += new Vector2(0f, -delta.x / _window.FixedAspectRatio);
                    }
                    rectTransform.offsetMax += delta with { y = 0 };
                    break;
                case ResizeDirection.DownRight:
                    rectTransform.offsetMin += _window.IsFixedAspectRatio
                        ? delta with { x = 0f, y = -delta.x / _window.FixedAspectRatio }
                        : delta with { x = 0f };
                    rectTransform.offsetMax += delta with { y = 0f };
                    break;
                case ResizeDirection.Down:
                    rectTransform.offsetMin += delta with { x = 0f };
                    if (_window.IsFixedAspectRatio) {
                        rectTransform.offsetMax += new Vector2(-delta.y * _window.FixedAspectRatio, 0f);
                    }
                    break;
                case ResizeDirection.DownLeft:
                    rectTransform.offsetMin += _window.IsFixedAspectRatio
                        ? delta with { y = delta.x / _window.FixedAspectRatio }
                        : delta;
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