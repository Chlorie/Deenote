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
        }

        private void OnNoteSelectionChanging()
        {
            _pianoSoundEditWindow.NotifySelectedNotesChanging(SelectedNotes);
        }

        private void OnNotesChanged(bool notesOrderChanged, bool selectionChanged, bool noteDataChangedExceptTime = false)
        {
            GameStageController.Instance.ForceUpdateStageNotes(notesOrderChanged, noteDataChangedExceptTime);

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
