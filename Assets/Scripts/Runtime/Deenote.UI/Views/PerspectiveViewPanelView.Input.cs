#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Library;
using Deenote.Systems.Inputting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    partial class PerspectiveViewPanelView :
        IPointerDownHandler, IPointerUpHandler,
        IPointerMoveHandler, IScrollHandler
    {
        [SerializeField] GraphicRaycaster _raycaster = default!;

        private void _OnStageLoaded_Input()
        {
            _raycaster.enabled = true;
        }

        private void RegisterKeyBindings()
        {
            // TODO: convert to contextual bindings
            const string togglePlayingStateName = "Deenote.TogglePlayingState";
            MainSystem.KeyBindingManager.RegisterAction(togglePlayingStateName, () => MainSystem.GamePlayManager.MusicPlayer.TogglePlayingState());
            var list = MainSystem.KeyBindingManager.GetBindings(togglePlayingStateName);
            list.AddGlobalBinding(new KeyBinding(KeyCode.Return));
            list.AddGlobalBinding(new KeyBinding(KeyCode.KeypadEnter));
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!MainSystem.GamePlayManager.IsChartLoaded())
                return;

            var editor = MainSystem.StageChartEditor;

            switch (eventData.button) {
                case PointerEventData.InputButton.Left: {
                    if (editor.Placer.IsPlacing) {
                        editor.Placer.CancelPlaceNote();
                    }
                    else {
                        editor.Placer.HideIndicators();
                        if (TryConvertScreenPointToNoteCoord(eventData.position, eventData.pressEventCamera, out var coord)) {
                            editor.Selector.BeginDragSelect(coord, toggleMode: UnityUtils.IsFunctionalKeyHolding(ctrl: true));
                        }
                    }
                    break;
                }
                case PointerEventData.InputButton.Right: {
                    if (!editor.Selector.IsDragSelecting) {
                        Vector2 pos = eventData.position;
                        if (TryConvertScreenPointToNoteCoord(pos, eventData.pressEventCamera, out var coord)) {
                            editor.Placer.BeginPlaceNote(coord, pos);
                        }
                    }
                    break;
                }
            }
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (!MainSystem.GamePlayManager.IsChartLoaded())
                return;

            var editor = MainSystem.StageChartEditor;
            if (editor.Selector.IsDragSelecting) {
                if (TryConvertScreenPointToNoteCoord(eventData.position, eventData.pressEventCamera, out var coord)) {
                    editor.Selector.UpdateDragSelect(coord);
                }
            }
            else {
                Vector2 pos = eventData.position;
                if (TryConvertScreenPointToNoteCoord(pos, eventData.pressEventCamera, out var coord)) {
                    editor.Placer.UpdateMovePlace(coord, pos);
                }
                else {
                    editor.Placer.HideIndicators();
                }
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!MainSystem.GamePlayManager.IsChartLoaded())
                return;

            var editor = MainSystem.StageChartEditor;
            switch (eventData.button) {
                case PointerEventData.InputButton.Left:
                    editor.Selector.EndDragSelect();
                    break;
                case PointerEventData.InputButton.Right:
                    if (TryConvertScreenPointToNoteCoord(eventData.position, eventData.pressEventCamera, out var coord)) {
                        editor.Placer.EndPlaceNote(coord);
                    }
                    else {
                        editor.Placer.CancelPlaceNote();
                    }
                    break;
            }
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (!MainSystem.GamePlayManager.IsChartLoaded())
                return;

            float scroll = eventData.scrollDelta.y;
            if (scroll != 0f) {
                float deltaTime = scroll * 0.1f * MainWindow.Settings.GameViewScrollSensitivity;
                MainSystem.GamePlayManager.MusicPlayer.Nudge(-deltaTime);
            }
        }

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, Camera camera, out NoteCoord coord)
        {
            MainSystem.GamePlayManager.AssertStageLoaded();

            var tsfm = _viewRawImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(tsfm, screenPoint, camera, out var localPoint)) {
                coord = default;
                return false;
            }

            var tsfmrect = tsfm.rect;
            var viewPoint = new Vector2(
                localPoint.x / tsfmrect.width,
                localPoint.y / tsfmrect.height);

            return MainSystem.GamePlayManager.TryConvertPerspectiveViewportPointToNoteCoord(viewPoint, out coord);
        }
    }
}