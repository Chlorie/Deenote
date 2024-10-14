using Deenote.Edit.Operations;
using Deenote.GameStage;
using Deenote.Project;
using Deenote.UI.ComponentModel;
using Deenote.UI.Windows;
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
        [SerializeField] private ProjectManager _projectManager = null!;
        private GameStageController Stage => GameStageController.Instance;

        [Header("Notify")]
        [SerializeField] private PropertiesWindow _propertiesWindow = null!;
        [SerializeField] private EditorPropertiesWindow _editorPropertiesWindow = null!;
        [SerializeField] private PianoSoundEditWindow _pianoSoundEditWindow = null!;
        [SerializeField] private PerspectiveViewWindow _perspectiveViewWindow = null!;

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
        }

        private void OnNoteSelectionChanging()
        {
            _pianoSoundEditWindow.NotifySelectedNotesChanging(SelectedNotes);
        }

        private void OnNotesChanged(bool notesOrderChanged, bool selectionChanged,
            bool noteDataChangedExceptTime = false)
        {
            Stage.ForceUpdateStageNotes(notesOrderChanged, noteDataChangedExceptTime);

            if (selectionChanged) {
                // Keep sync with NotifyProjectChanged()
                // Stage is required to update when selection changed unless project changed
                _propertyChangedNotifier.Invoke(this, NotifyProperty.SelectedNotes);
                _editorPropertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
                _propertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
                _pianoSoundEditWindow.NotifySelectedNotesChanged(SelectedNotes);
            }
        }

        public void NotifyProjectChanged()
        {
            _operationHistory.Clear();

            // _clipBoardNotes.Clear();
            _placeState = PlacementState.Idle;
            RefreshNoteIndicator();

            _noteSelectionController.ClearSelection();
            // Keep sync with OnNoteChanged()
            _propertyChangedNotifier.Invoke(this, NotifyProperty.SelectedNotes);
            _editorPropertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
            _propertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
            _pianoSoundEditWindow.NotifySelectedNotesChanged(SelectedNotes);
        }

        #region Undo

        public void Undo() => _operationHistory.Undo();

        public void Redo() => _operationHistory.Redo();

        #endregion

        private PropertyChangeNotifier<EditorController, NotifyProperty> _propertyChangedNotifier;

        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<EditorController> action)
            => _propertyChangedNotifier.AddListener(flag, action);

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