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
    partial class PerspectiveViewPanelView
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

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, Camera? camera, bool applyHighlightNoteSpeed, out NoteCoord coord)
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

            return MainSystem.GamePlayManager.TryConvertPerspectiveViewportPointToNoteCoord(viewPoint,
                applyHighlightNoteSpeed ? MainSystem.StageChartEditor.Placer.PlacingNoteSpeed : 1f, out coord);
        }
    }
}