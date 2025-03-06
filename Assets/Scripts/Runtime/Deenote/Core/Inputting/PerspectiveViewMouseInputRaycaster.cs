#nullable enable

using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.Inputting
{
    public sealed class PerspectiveViewMouseInputRaycaster : MonoBehaviour
    {
        [SerializeField] RectTransform _viewRectTransform = default!;

        private GamePlayManager _game = default!;
        private StageChartEditor _editor = default!;

        private void Awake()
        {
            _game = MainSystem.GamePlayManager;
            _editor = MainSystem.StageChartEditor;
        }

        private void Update()
        {
            if (_game.IsChartLoaded() && _game.IsStageLoaded()) {
                var pos = Input.mousePosition;
                if (Input.GetMouseButtonDown(0))
                    OnLeftMouseDown(pos);
                if (Input.GetMouseButtonDown(1))
                    OnRightMouseDown(pos);
                if (Input.GetMouseButtonUp(0))
                    OnLeftMouseUp(pos);
                if (Input.GetMouseButtonUp(1))
                    OnRightMouseUp(pos);
                OnMouseMove(pos);
                OnMouseScroll(Input.mouseScrollDelta.y);
            }
        }

        private void OnLeftMouseDown(Vector2 mousePosition)
        {
            if (_editor.Placer.IsPlacing) {
                _editor.Placer.CancelPlaceNote();
            }
            else {
                if (TryConvertScreenPointToNoteCoord(mousePosition, false, out var coord)) {
                    _editor.Selector.BeginDragSelect(coord, toggleMode: UnityUtils.IsFunctionalKeyHolding(ctrl: true));
                }
            }
        }

        private void OnRightMouseDown(Vector2 mousePosition)
        {
            if (_editor.Selector.IsDragSelecting)
                return;
            else {
                if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
                    _editor.Placer.BeginPlaceNote(coord, mousePosition);
                }
            }
        }

        private void OnMouseMove(Vector2 mousePosition)
        {
            if (_editor.Selector.IsDragSelecting) {
                if (TryConvertScreenPointToNoteCoord(mousePosition, false, out var coord)) {
                    _editor.Selector.UpdateDragSelect(coord);
                }
            }
            else {
                if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
                    _editor.Placer.UpdateMovePlace(coord, mousePosition);
                }
                else {
                    _editor.Placer.HideIndicators();
                }
            }
        }

        private void OnLeftMouseUp(Vector2 mousePosition)
        {
            if (_editor.Selector.IsDragSelecting) {
                _editor.Selector.EndDragSelect();
            }
        }

        private void OnRightMouseUp(Vector2 mousePosition)
        {
            if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
                _editor.Placer.EndPlaceNote(coord);
            }
            else {
                _editor.Placer.CancelPlaceNote();
            }
        }

        private void OnMouseScroll(float scrollDeltaY)
        {
            if (scrollDeltaY != 0f) {
                float deltaTime = scrollDeltaY * 0.1f * MainSystem.GlobalSettings.GameViewScrollSensitivity;
                _game.MusicPlayer.Nudge(-deltaTime);
            }
        }

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, bool applyHighlightNoteSpeed, out NoteCoord coord)
        {
            MainSystem.GamePlayManager.AssertStageLoaded();

            var tsfm = _viewRectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(tsfm, screenPoint, null, out var localPoint)) {
                coord = default;
                return false;
            }

            var tsfmrect = tsfm.rect;
            var viewPoint = new Vector2(
                localPoint.x / tsfmrect.width,
                localPoint.y / tsfmrect.height);

            return MainSystem.GamePlayManager.TryConvertPerspectiveViewportPointToNoteCoord(viewPoint,
                applyHighlightNoteSpeed ? MainSystem.StageChartEditor.Placer.PlacingNoteSpeed : 1f, out coord);
        }

        private void OnValidate()
        {
            _viewRectTransform ??= GetComponent<RectTransform>();
        }
    }
}