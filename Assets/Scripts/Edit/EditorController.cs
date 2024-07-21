using Deenote.Edit.Operations;
using Deenote.GameStage;
using Deenote.Project;
using Deenote.UI.Windows;
using UnityEngine;

namespace Deenote.Edit
{
    /// <summary>
    /// Containing all chart editing logic, 
    /// DO NOT bypass this class to edit chart and notes, 'cause it contains undo/notify actions etc.
    /// </summary>
    public sealed partial class EditorController : MonoBehaviour
    {
        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _stage;

        [Header("Notify")]
        [SerializeField] PropertiesWindow _propertiesWindow;
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;
        [SerializeField] PianoSoundEditWindow _pianoSoundEditWindow;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;

        [Header("Note Edit")]
        private UndoableOperationHistory _operationHistory;

        public bool HasUnsavedChange => _operationHistory.CanUndo;

        private void Awake()
        {
            _operationHistory = new(100);
            AwakeNotePlacement();
        }

        private void Start()
        {
            // TODO: Temp
            SnapToPositionGrid = SnapToTimeGrid = true;
            IsNoteIndicatorOn = true;
        }

        private void OnNoteSelectionChanging()
        {
            _pianoSoundEditWindow.NotifySelectedNotesChanging(SelectedNotes);
        }

        private void OnNotesChanged(bool notesOrderChanged, bool selectionChanged, bool noteDataChangedExceptTime = false)
        {
            _stage.ForceUpdateStageNotes(notesOrderChanged, noteDataChangedExceptTime);

            if (selectionChanged) {
                // Keep sync with NotifyProjectChanged()
                // Stage is required to update when selection changed unless project changed
                _editorPropertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
                _propertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
                _pianoSoundEditWindow.NotifySelectedNotesChanged(SelectedNotes);
            }
        }

        public void NotifyProjectChanged()
        {
            _operationHistory.Clear();

            // _clipBoardNotes.Clear();
            _isPasting = false;
            RefreshNoteIndicator();
            
            _noteSelectionController.ClearSelection();
            // Keep sync with OnNoteChanged()
            _editorPropertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
            _propertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
            _pianoSoundEditWindow.NotifySelectedNotesChanged(SelectedNotes);
        }

        #region Undo

        public void Undo() => _operationHistory.Undo();

        public void Redo() => _operationHistory.Redo();

        #endregion
    }
}
