using Deenote.Audio;
using Deenote.Inputting;
using Deenote.Utilities;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.GameStage
{
    partial class PerspectiveViewController :
        IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler, IScrollHandler, IPointerExitHandler
    {
        [Inject] private MusicController _musicController = null!;
        [Inject] private KeyBindingManager _keyBindingManager = null!;

        private void RegisterKeyBindings()
        {
            // TODO: convert to contextual bindings
            const string togglePlayingStateName = "Deenote.TogglePlayingState";
            _keyBindingManager.RegisterAction(togglePlayingStateName, _musicController.TogglePlayingState);
            var list = _keyBindingManager.GetBindings(togglePlayingStateName);
            list.AddGlobalBinding(new KeyBinding(KeyCode.Return));
            list.AddGlobalBinding(new KeyBinding(KeyCode.KeypadEnter));
        }


        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (!_stage.IsActive)
                return;

            // Note selection
            if (_editor.IsSelecting) {
                if (TryConvertScreenPointToNoteCoord(eventData.position, out var coord)) {
                    _editor.UpdateNoteSelection(coord);
                }
            }
            else {
                Vector2 mousePosition = eventData.position;
                // Move indicator
                if (TryConvertScreenPointToNoteCoord(mousePosition, out var coord)) {
                    _editor.MoveNoteIndicator(mousePosition, coord);
                }
                else {
                    _editor.HideNoteIndicator();
                }
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!_stage.IsActive)
                return;

            switch (eventData.button) {
                case PointerEventData.InputButton.Left: {
                    if (_editor.IsPlacing) {
                        _editor.CancelPlaceNote();
                    }
                    else {
                        _editor.HideNoteIndicator();
                        if (TryConvertScreenPointToNoteCoord(eventData.position, out var coord)) {
                            _editor.BeginSelectNote(coord, toggleMode: UnityUtils.IsFunctionalKeyHolding(ctrl: true));
                        }
                    }
                    break;
                }
                case PointerEventData.InputButton.Right: {
                    if (!_editor.IsSelecting) {
                        Vector2 mousepos = eventData.position;
                        if (TryConvertScreenPointToNoteCoord(mousepos, out var coord)) {
                            _editor.BeginPlaceNote(mousepos, coord);
                        }
                    }
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
                    _editor.EndSelectNote();
                    break;
                }
                case PointerEventData.InputButton.Right: {
                    if (TryConvertScreenPointToNoteCoord(eventData.position, out var coord)) {
                        _editor.EndPlaceNote(coord);
                    }
                    else {
                        _editor.CancelPlaceNote();
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
                _musicController.NudgePlaybackPosition(-deltaTime);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _editor.CancelPlaceNote();
        }

        public bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, out NoteCoord coord)
        {
            var transform = _cameraViewRawImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(transform, screenPoint, null,
                    out var localPoint)) {
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
            return true;
        }

        private bool TryConvertViewPointToNoteCoord(Vector3 viewPoint, out NoteCoord coord)
        {
            Ray ray = ViewCamera.ViewportPointToRay(viewPoint);
            if (_stage.NotePanelPlane.Raycast(ray, out var distance)) {
                var hitp = ray.GetPoint(distance);
                coord = new(MainSystem.Args.ZToOffsetTime(hitp.z) + _stage.CurrentMusicTime,
                    MainSystem.Args.XToPosition(hitp.x));
                return true;
            }
            coord = default;
            return false;
        }
    }
}