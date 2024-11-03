#nullable enable

using Deenote.Edit.Operations;
using Deenote.GameStage;
using Deenote.UI.ComponentModel;
using System;
using UnityEngine;

namespace Deenote.Edit
{
    /// <summary>
    /// Containing all chart editing logic, 
    /// DO NOT bypass this class to edit chart and notes, 'cause it contains undo/notify actions etc.
    /// </summary>
    public sealed partial class EditorController : MonoBehaviour, INotifyPropertyChange<EditorController, EditorController.NotifyProperty>
    {
        private GameStageController Stage => GameStageController.Instance;

        [Header("Note Edit")]
        private UndoableOperationHistory _operationHistory = new(100);

        public bool HasUnsavedChange => _operationHistory.CanUndo;

        private void Awake()
        {
            AwakeNotePlacement();
        }

        private void Start()
        {
            // TODO: Temp
            SnapToPositionGrid = SnapToTimeGrid = true;
            IsNoteIndicatorOn = true;
            Start_NotePlacement();

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStageController.NotifyProperty.CurrentChart,
                stage => ResetEditorStatus());
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStageController.NotifyProperty.IsShowLinkLines,
                stage =>
                {
                    bool show = stage.IsShowLinkLines;
                    foreach (var note in _noteIndicatorList) {
                        note.UpdateLinkLineVisibility(show);
                    }
                });
        }

        private void OnNotesChanged(bool notesOrderChanged, bool selectionChanged,
            bool noteDataChangedExceptTime = false)
        {
            Stage.ForceUpdateStageNotes(notesOrderChanged, noteDataChangedExceptTime);

            if (selectionChanged) {
                // Keep sync with NotifyProjectChanged()
                // Stage is required to update when selection changed unless project changed
                _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes);
            }
        }

        private void ResetEditorStatus()
        {
            _operationHistory.Clear();

            // _clipBoardNotes.Clear();
            _placeState = PlacementState.Idle;
            RefreshNoteIndicator();

            _noteSelectionController.ClearSelection();
            _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes);
        }

        #region Undo

        public void Undo() => _operationHistory.Undo();

        public void Redo() => _operationHistory.Redo();

        #endregion

        private PropertyChangeNotifier<EditorController, NotifyProperty> _propertyChangeNotifier;

        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<EditorController> action) 
            => _propertyChangeNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            SelectedNotes_Changing,
            SelectedNotes,
            IsIndicatorOn,
            SnapToTimeGrid,
            SnapToPositionGrid,

            NoteTime,
            NotePosition,
            NoteSize,
            NoteShift,
            NoteSpeed,
            NoteDuration,
            NoteKind,
            NoteVibrate,
            NoteWarningType,
            NoteEventId,
            NoteSounds,
        }
    }
}