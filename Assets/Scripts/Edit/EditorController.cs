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
            SnapToPositionGrid = SnapToTimeGrid = true;
            IsNoteIndicatorOn = true;
        }

        private void OnNoteSelectionChanging()
        {
            _pianoSoundEditWindow.NotifySelectedNotesChanging(SelectedNotes);
        }

        private void OnNotesChanged(bool notesOrderChanged, bool selectionChanged, bool noteDataChangedExceptTime = false)
        {
            // NoteTime maybe changed, so we always force update everytime
            _stage.ForceUpdateStageNotes(notesOrderChanged, noteDataChangedExceptTime);

            //if (noteOrderChanged)
            //    _stage.UpdateStageNotes(false);
            //else
            //    _stage.ForceUpdateNotesDisplay();

            if (selectionChanged) {
                _editorPropertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
                _propertiesWindow.NotifyNoteSelectionChanged(SelectedNotes);
                _pianoSoundEditWindow.NotifySelectedNotesChanged(SelectedNotes);
            }
        }

        #region Undo

        public void Undo() => _operationHistory.Undo();

        public void Redo() => _operationHistory.Redo();

        #endregion
    }
}
