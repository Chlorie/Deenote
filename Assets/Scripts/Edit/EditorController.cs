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

        [Header("Note Edit")]
        private UndoableOperationHistory _operationHistory = new(100);

        public bool HasUnsavedChange => _operationHistory.CanUndo;

        private void Awake()
        {
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
            GameStageController.Instance.ForceUpdateStageNotes(notesOrderChanged, noteDataChangedExceptTime);

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
