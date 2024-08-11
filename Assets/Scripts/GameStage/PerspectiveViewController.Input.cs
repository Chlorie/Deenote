using Deenote.Inputting;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.GameStage
{
    partial class PerspectiveViewController : IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler, IScrollHandler
    {
        private MouseButtons _pressedMouseButtons;
        private MouseActionState _mouseActionState;

        private void UpdateNoteIndicatorPosition(Vector2 mousePosition)
        {
            // Move indicator
            if (TryConvertScreenPointToNoteCoord(mousePosition, out var coord)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    _editor.MoveNoteIndicator(coord, true);
                else
                    _editor.MoveNoteIndicator(coord, false);
            }
            else {
                _editor.HideNoteIndicator();
            }
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (!_stage.IsActive)
                return;

            // Note selection
            if (_pressedMouseButtons.HasFlag(MouseButtons.Left) && _mouseActionState is MouseActionState.NoteSelecting) {
                if (TryConvertScreenPointToNoteCoord(eventData.position, out var coord)) {
                    _editor.UpdateNoteSelection(coord);
                }
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!_stage.IsActive)
                return;

            switch (eventData.button) {
                case PointerEventData.InputButton.Left: {
                    _pressedMouseButtons.Add(MouseButtons.Left);
                    _mouseActionState = MouseActionState.NoteSelecting;
                    if (TryConvertScreenPointToNoteCoord(eventData.position, out var coord)) {
                        _editor.StartNoteSelection(coord, toggleMode: UnityUtils.IsFunctionalKeyHolding(ctrl: true));
                    }
                    break;
                }
                case PointerEventData.InputButton.Right: {
                    _pressedMouseButtons.Add(MouseButtons.Right);
                    _mouseActionState = MouseActionState.NotePlacing;
                    break;
                }
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!_stage.IsActive)
                return;

            switch (eventData.button) {
                case PointerEventData.InputButton.Left: {
                    _pressedMouseButtons.Remove(MouseButtons.Left);
                    _mouseActionState = MouseActionState.None;
                    _editor.EndNoteSelection();
                    break;
                }
                case PointerEventData.InputButton.Right: {
                    _pressedMouseButtons.Remove(MouseButtons.Right);
                    _mouseActionState = MouseActionState.None;
                    if (TryConvertScreenPointToNoteCoord(eventData.position, out var coord)) {
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                            _editor.PlaceNoteAt(coord, true);
                        else
                            _editor.PlaceNoteAt(coord, false);
                    }
                    break;
                }
            }
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (!_stage.IsActive)
                return;

            float scroll = eventData.scrollDelta.y;
            if (scroll != 0f) {
                float deltaTime = scroll * 0.1f * MainSystem.Input.MouseScrollSensitivity;
                _stage.CurrentMusicTime -= deltaTime;
            }
        }

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, out NoteCoord coord)
        {
            var transform = _cameraViewRawImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, screenPoint, null, out var localPoint)) {
                coord = default;
                return false;
            }

            var viewPoint = new Vector2(
                localPoint.x / transform.rect.width,
                localPoint.y / (transform.rect.height * ViewCamera.rect.height));

            if (viewPoint is not { x: >= 0f and <= 1f, y: >= 0f and <= 1f }) {
                coord = default;
                return false;
            }

            if (!TryConvertViewPointToNoteCoord(viewPoint, out coord))
                return false;

            // Ignore when press position is too far
            if (coord.Time > _stage.CurrentMusicTime + _stage.StageNoteAheadTime)
                return false;

            if (coord.Position is > MainSystem.Args.NoteSelectionMaxPosition or < -MainSystem.Args.NoteSelectionMaxPosition)
                return false;

            return true;
        }

        private bool TryConvertViewPointToNoteCoord(Vector3 viewPoint, out NoteCoord coord)
        {
            Ray ray = ViewCamera.ViewportPointToRay(viewPoint);
            if (_stage.NotePanelPlane.Raycast(ray, out var distance)) {
                var hitp = ray.GetPoint(distance);
                coord = new(MainSystem.Args.ZToOffsetTime(hitp.z) + _stage.CurrentMusicTime, MainSystem.Args.XToPosition(hitp.x));
                return true;
            }
            coord = default;
            return false;
        }

        private enum MouseActionState
        {
            None,
            NoteSelecting = 1,
            NotePlacing = 2,
        }
    }
}