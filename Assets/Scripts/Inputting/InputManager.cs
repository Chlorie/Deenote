#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.InputSystem.InputActions;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.UI;
using Deenote.UIFramework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deenote.Inputting
{
    public sealed class InputManager : MonoBehaviour
    {
        private KeyboardShortcutInputActions _inputActions = default!;

        private GamePlayManager _game = default!;
        private StageChartEditor _editor = default!;

        private float? _musicResetTime;

        private bool _isEnabled;
        private bool _isGamePlayEnabled;

        private void Awake()
        {
            _game = MainSystem.GamePlayManager;
            _editor = MainSystem.StageChartEditor;
            _inputActions = new();

            RegisterStageGamePlay();
            RegisterStageSettings();
            RegisterNoteEdit();
            RegisterEditorSettings();
            RegisterProjectManagement();

            UISystem.FocusedControlChanged += ctrl =>
            {
                SetGeneralsEnable(ctrl is null);
            };

            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager => SetGamePlayEnabled(manager.IsChartLoaded()));
        }

        private void OnEnable()
        {
            SetGeneralsEnable(true);
        }

        private void OnDisable()
        {
            SetGeneralsEnable(false);
        }

        private void Update()
        {
            Update_MouseAction();
        }


        private void SetGeneralsEnable(bool enable)
        {
            if (Utils.SetField(ref _isEnabled, enable)) {
                NotifyActionsEnable();
            }
        }

        private void SetGamePlayEnabled(bool enable)
        {
            if (Utils.SetField(ref _isGamePlayEnabled, enable)) {
                NotifyActionsEnable();
            }
        }

        private void NotifyActionsEnable()
        {
            if (_isEnabled) {
                _inputActions.StageSettings.Enable();
                _inputActions.EditorSettings.Enable();
                _inputActions.ProjectManagement.Enable();
                if (_isGamePlayEnabled) {
                    _inputActions.StageGamePlay.Enable();
                    _inputActions.NoteEdit.Enable();
                }
            }
            else {
                _inputActions.Disable();
            }
        }

        #region GamePlay

        private bool _manualPlaySpeedUp;
        private float _manualPlay;

        private void RegisterStageGamePlay()
        {
            var actions = _inputActions.StageGamePlay;
            actions.PauseResume.started += _ => _game.MusicPlayer.TogglePlayingState();
            actions.AutoResetPlay.started += _ =>
            {
                _musicResetTime = _game.MusicPlayer.Time;
                _game.MusicPlayer.Play();
            };
            actions.AutoResetPlay.canceled += _ =>
            {
                if (_musicResetTime is { } mrt) {
                    _game.MusicPlayer.Stop();
                    _game.MusicPlayer.Time = mrt;
                }
            };
            actions.ToMusicStart.started += _ => _game.MusicPlayer.Time = 0f;
            actions.ToMusicEnd.started += _ => _game.MusicPlayer.Time = _game.MusicPlayer.ClipLength;
            actions.ManualPlay.started += context =>
            {
                _manualPlay = context.ReadValue<float>();
                SetManualPlay();
            };
            actions.ManualPlay.canceled += context =>
            {
                _manualPlay = 0f;
                _game.SetManualPlaySpeed(null);
            };
            actions.ManualPlaySpeedUp.started += _ =>
            {
                _manualPlaySpeedUp = true;
                if (_manualPlay != 0f) SetManualPlay();
            };
            actions.ManualPlaySpeedUp.canceled += _ =>
            {
                _manualPlaySpeedUp = false;
                if (_manualPlay != 0f) SetManualPlay();
            };
            actions.ScrollPlay.performed += context =>
            {
                if (!MainWindow.Views.PerspectiveViewPanelView.IsHovering)
                    return;

                var delta = context.ReadValue<Vector2>().y;
                if (delta != 0f) {
                    var deltaTime = delta * 0.001f * MainSystem.GlobalSettings.GameViewScrollSensitivity;
                    _game.MusicPlayer.Nudge(-deltaTime);
                }
            };

            void SetManualPlay()
            {
                if (_manualPlaySpeedUp)
                    _game.SetManualPlaySpeed(5.0f * _manualPlay);
                else
                    _game.SetManualPlaySpeed(2.5f * _manualPlay);
            }
        }

        #endregion

        #region StageSettings

        private const int NoteFallSpeedDelta = 5;
        private const int MusicSpeedDelta = 1;

        private void RegisterStageSettings()
        {
            var actions = _inputActions.StageSettings;
            actions.EscapeFullScreen.started += _ => { MainWindow.Views.PerspectiveViewPanelView.SetIsFullScreen(false); };
            actions.NoteFallSpeedUp.started += _ => _game.NoteFallSpeed += NoteFallSpeedDelta;
            actions.NoteFallSpeedDown.started += _ => _game.NoteFallSpeed -= NoteFallSpeedDelta;
            actions.MusicSpeedUp.started += _ => _game.MusicSpeed += MusicSpeedDelta;
            actions.MusicSpeedDown.started += _ => _game.MusicSpeed -= MusicSpeedDelta;
        }

        #endregion

        #region NoteEdit

        private const float TimeDelta = 0.001f;
        private const float TimeDeltaLarge = 0.01f;
        private const float PositionDelta = 0.01f;
        private const float PositionDeltaLarge = 0.1f;
        private const float SizeDelta = 0.01f;
        private const float SizeDeltaLarge = 0.1f;
        private const float SpeedDelta = 0.01f;
        private const float SpeedDeltaLarge = 0.1f;
        private const float DurationDelta = 0.001f;
        private const float DurationDeltaLarge = 0.01f;

        private void RegisterNoteEdit()
        {
            var actions = _inputActions.NoteEdit;
            actions.SelectAllNotes.started += _ => _editor.Selector.SelectAll();
            actions.RemoveSelectedNotes.started += _ => _editor.RemoveSelectedNotes();
            actions.Copy.started += _ => _editor.CopySelectedNotes();
            actions.Cut.started += _ => _editor.CutSelectedNotes();
            actions.Paste.started += _ => _editor.PasteNotes();
            actions.Redo.started += _ => _editor.OperationMemento.Redo();
            actions.Undo.started += _ => _editor.OperationMemento.Undo();
            actions.TimeDec.started += _ => _editor.EditSelectedNotesTime(t => t - TimeDelta);
            actions.TimeInc.started += _ => _editor.EditSelectedNotesTime(t => t + TimeDelta);
            actions.TimeDecLarge.started += _ => _editor.EditSelectedNotesTime(t => t - TimeDeltaLarge);
            actions.TimeIncLarge.started += _ => _editor.EditSelectedNotesTime(t => t + TimeDeltaLarge);
            actions.TimeDecByGrid.started += _ => _editor.EditSelectedNotesTime(t => _game.Grids.FloorToNearestNextTimeGridTime(t) ?? t);
            actions.TimeIncByGrid.started += _ => _editor.EditSelectedNotesTime(t => _game.Grids.CeilToNearestNextTimeGridTime(t) ?? t);
            actions.PositionLeft.started += _ => _editor.EditSelectedNotesPosition(p => p - PositionDelta);
            actions.PositionRight.started += _ => _editor.EditSelectedNotesPosition(p => p + PositionDelta);
            actions.PositionLeftLarge.started += _ => _editor.EditSelectedNotesPosition(p => p - PositionDeltaLarge);
            actions.PositionRightLarge.started += _ => _editor.EditSelectedNotesPosition(p => p + PositionDeltaLarge);
            actions.PositionLeftByGrid.started += _ => _editor.EditSelectedNotesPosition(p => _game.Grids.FloorToNearestNextPositionGridPosition(p) ?? p);
            actions.PositionRightByGrid.started += _ => _editor.EditSelectedNotesPosition(p => _game.Grids.CeilToNearestNextPositionGridPosition(p) ?? p);
            actions.PositionMirror.started += _ => _editor.EditSelectedNotesPosition(p => -p);
            actions.CoordQuantize.started += _ => _editor.EditSelectedNotesPositionCoord(c => _game.Grids.Quantize(c, true, true));
            actions.SizeDec.started += _ => _editor.EditSelectedNotesSize(s => s - SizeDelta);
            actions.SizeInc.started += _ => _editor.EditSelectedNotesSize(s => s + SizeDelta);
            actions.SizeDecLarge.started += _ => _editor.EditSelectedNotesSize(s => s - SizeDeltaLarge);
            actions.SizeIncLarge.started += _ => _editor.EditSelectedNotesSize(s => s + SizeDeltaLarge);
            actions.SpeedDec.started += _ => _editor.EditSelectedNotesSpeed(s => s -= SpeedDelta);
            actions.SpeedInc.started += _ => _editor.EditSelectedNotesSpeed(s => s += SpeedDelta);
            actions.SpeedDecLarge.started += _ => _editor.EditSelectedNotesSpeed(s => s -= SpeedDeltaLarge);
            actions.SpeedIncLarge.started += _ => _editor.EditSelectedNotesSpeed(s => s += SpeedDeltaLarge);
            actions.KindClick.started += _ => _editor.EditSelectedNotesKind(NoteModel.NoteKind.Click);
            actions.KindSlide.started += _ => _editor.EditSelectedNotesKind(NoteModel.NoteKind.Slide);
            actions.KindSwipe.started += _ => _editor.EditSelectedNotesKind(NoteModel.NoteKind.Swipe);
            actions.SoundAdd.started += _ => _editor.EditSelectedNoteSounds(true);
            actions.SoundRemove.started += _ => _editor.EditSelectedNoteSounds(false);
            actions.DurationDec.started += _ => _editor.EditSelectedNotesDuration(d => d - DurationDelta);
            actions.DurationInc.started += _ => _editor.EditSelectedNotesDuration(d => d + DurationDelta);
            actions.DurationDecLarge.started += _ => _editor.EditSelectedNotesDuration(d => d - DurationDeltaLarge);
            actions.DurationIncLarge.started += _ => _editor.EditSelectedNotesDuration(d => d + DurationDeltaLarge);
            actions.DurationDecByGrid.started += _ => _editor.EditSelectedNotesEndTime(t => _game.Grids.FloorToNearestNextTimeGridTime(t) ?? t);
            actions.DurationIncByGrid.started += _ => _editor.EditSelectedNotesEndTime(t => _game.Grids.CeilToNearestNextTimeGridTime(t) ?? t);
            actions.CreateHoldBetween.started += _ =>
            {
                if (_editor.Selector.SelectedNotes.Length != 2)
                    return;

                var prev = _editor.Selector.SelectedNotes[0];
                var next = _editor.Selector.SelectedNotes[1];

                _editor.CreateHoldBetween(prev, next);
            };
        }

        #endregion

        #region EditorSettings

        private void RegisterEditorSettings()
        {
            var actions = _inputActions.EditorSettings;
            actions.SnapToGrids.started += _ =>
            {
                var placer = _editor.Placer;
                var val = !(placer.SnapToPositionGrid && placer.SnapToTimeGrid);
                placer.SnapToPositionGrid = placer.SnapToTimeGrid = val;
            };
            actions.PasteRememberPosition.started += _ => _editor.Placer.PasteRememberPositionModifier = true;
            actions.PasteRememberPosition.canceled += _ => _editor.Placer.PasteRememberPositionModifier = false;
            actions.PlaceNoteSlideFlag.started += _ => _editor.Placer.PlaceSlideModifier = true;
            actions.PlaceNoteSlideFlag.canceled += _ => _editor.Placer.PlaceSlideModifier = false;
            actions.PlaceSoundNote.started += _ => _editor.Placer.PlaceSoundNoteByDefault = !_editor.Placer.PlaceSoundNoteByDefault;
        }

        #endregion

        #region Mouse Action / NotePlacement

        private void Update_MouseAction()
        {
            if (_game.IsChartLoaded() && _game.IsStageLoaded()) {
                var mouse = Mouse.current;
                var pos = mouse.position.ReadValue();

                if (MainWindow.Views.PerspectiveViewPanelView.IsHovering) {
                    if (mouse.leftButton.wasPressedThisFrame)
                        OnLeftMouseDown(pos);
                    if (mouse.rightButton.wasPressedThisFrame)
                        OnRightMouseDown(pos);
                    if (mouse.leftButton.wasReleasedThisFrame)
                        OnLeftMouseUp(pos);
                    if (mouse.rightButton.wasReleasedThisFrame)
                        OnRightMouseUp(pos);
                }
                OnMouseMove(pos);
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
                    _editor.Placer.UpdatePlaceNote(coord, mousePosition);
                }
                else {
                    _editor.Placer.DisablePlaceNote();
                }
            }
        }

        private void OnLeftMouseUp(Vector2 mousePosition)
        {
            if (_editor.Selector.IsDragSelecting) {
                if (MainWindow.Views.PerspectiveViewPanelView.TryConvertScreenPointToViewportPoint(mousePosition, out var vp))
                    _editor.Selector.EndDragSelect(vp);
            }
        }

        private void OnRightMouseUp(Vector2 mousePosition)
        {
            if (TryConvertScreenPointToNoteCoord(mousePosition, true, out var coord)) {
                _editor.Placer.EndPlaceNote(coord, mousePosition);
            }
            else {
                _editor.Placer.CancelPlaceNote();
            }
        }

        private bool TryConvertScreenPointToNoteCoord(Vector2 screenPoint, bool applyHighlightNoteSpeed, out NoteCoord coord)
        {
            MainSystem.GamePlayManager.AssertStageLoaded();

            if (!MainWindow.Views.PerspectiveViewPanelView.TryConvertScreenPointToViewportPoint(screenPoint, out var viewPoint)) {
                coord = default;
                return false;
            }

            var res = MainSystem.GamePlayManager.TryConvertPerspectiveViewportPointToNoteCoord(viewPoint,
                applyHighlightNoteSpeed ? MainSystem.StageChartEditor.Placer.PlacingNoteSpeed : 1f, out coord);

            return res;
        }

        #endregion

        #region ProjectManage

        private void RegisterProjectManagement()
        {
            var actions = _inputActions.ProjectManagement;
            actions.NewProject.started += _ => MainWindow.Views.MenuNavigationPageView.MenuCreateNewProjectAsync().Forget();
            actions.OpenProject.started += _ => MainWindow.Views.MenuNavigationPageView.MenuOpenProjectAsync().Forget();
            actions.SaveProject.started += _ => MainWindow.Views.MenuNavigationPageView.MenuSaveProjectAsync().Forget();
            actions.SaveProjectAs.started += _ => MainWindow.Views.MenuNavigationPageView.MenuSaveProjectAsAsync().Forget();
            // TODO: ExportChartJsons Impl

        }

        #endregion
    }
}