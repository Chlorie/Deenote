using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Edit
{
    partial class EditorController
    {
        [Header("Note Selection")]
        [SerializeField] NoteSelectionController _noteSelectionController;

        public ReadOnlySpan<NoteModel> SelectedNotes => _noteSelectionController.SelectedNotes;

        public void SelectAllNotes()
        {
            OnNoteSelectionChanging();
            _noteSelectionController.ClearSelection();
            foreach (var note in Stage.Chart.Notes.EnumerateSelectableModels()) {
                _noteSelectionController.SelectNote(note);
            }
            OnNotesChanged(false, true);
        }

        public void StartNoteSelection(NoteCoord startCoord, bool toggleMode)
        {
            OnNoteSelectionChanging();
            _noteSelectionController.StartNoteSelection(startCoord, toggleMode);
            OnNotesChanged(false, true);
        }

        public void UpdateNoteSelection(NoteCoord endCoord)
        {
            OnNoteSelectionChanging();
            _noteSelectionController.UpdateNoteSelection(endCoord);
            OnNotesChanged(false, true);
        }

        public void EndNoteSelection()
        {
            _noteSelectionController.EndNoteSelection();
        }

        [Serializable]
        private sealed class NoteSelectionController
        {
            [SerializeField] EditorController _editor;
            [SerializeField] RectTransform _noteSelectionIndicatorImageTransform;
            private NoteCoord _noteSelectionIndicatorStartCoord;
            private bool _isSelecting;
            private readonly List<NoteModel> _selectedNotes = new();
            private readonly List<NoteModel> _inSelectionNotes = new();

            public ReadOnlySpan<NoteModel> SelectedNotes => _selectedNotes.AsSpan();

            public void SelectNote(NoteModel note)
            {
                if (note.IsSelected)
                    return;

                _selectedNotes.Add(note);
                note.IsSelected = true;
            }

            /// <summary>
            /// Add new notes into selection
            /// </summary>
            public void SelectNotes(IEnumerable<NoteModel> notes)
            {
                foreach (var note in notes) {
                    SelectNote(note);
                }
            }

            public void DeselectNote(NoteModel note)
            {
                if (_selectedNotes.Remove(note))
                    note.IsSelected = false;
            }

            /// <summary>
            /// Deselect notes in selection, if note is not selected, do nothing
            /// </summary>
            /// <param name="notes"></param>
            public void DeselectNotes(IEnumerable<NoteModel> notes)
            {
                foreach (var note in notes) {
                    if (note.IsSelected) {
                        var remove = _selectedNotes.Remove(note);
                        Debug.Assert(remove == true);
                        note.IsSelected = false;
                    }
                }
            }

            public void DeselectNoteAt(Index index)
            {
                var i = index.GetOffset(_selectedNotes.Count);
                var note = _selectedNotes[i];
                _selectedNotes.RemoveAt(i);
                note.IsSelected = false;
            }

            public void StartNoteSelection(NoteCoord startCoord, bool toggleMode)
            {
                _noteSelectionIndicatorImageTransform.gameObject.SetActive(true);

                startCoord.Time = Mathf.Clamp(startCoord.Time, 0f, _editor.Stage.MusicLength);
                _noteSelectionIndicatorStartCoord = startCoord;
                _isSelecting = true;

                UpdateNoteSelectionInternal(startCoord, startCoord,
                    toggleMode ? NoteSelectionUpdateMode.Toggle : NoteSelectionUpdateMode.Reselect);
            }

            public void UpdateNoteSelection(NoteCoord endCoord)
            {
                if (!_isSelecting)
                    return;

                Debug.Assert(_noteSelectionIndicatorImageTransform.gameObject.activeSelf);

                endCoord.Time = Mathf.Clamp(endCoord.Time, 0f, _editor.Stage.MusicLength);

                UpdateNoteSelectionInternal(_noteSelectionIndicatorStartCoord, endCoord,
                    NoteSelectionUpdateMode.Toggle);
            }

            public void EndNoteSelection()
            {
                _noteSelectionIndicatorImageTransform.gameObject.SetActive(false);
                _isSelecting = false;

                foreach (var note in _inSelectionNotes) {
                    // This is to set note._isInSelection to false while
                    // not change the value of note.IsSelected
                    note.IsSelected = note.IsSelected;
                }
                _inSelectionNotes.Clear();
            }

            public void ClearSelection()
            {
                foreach (var note in _selectedNotes) {
                    note.IsSelected = false;
                }
                _selectedNotes.Clear();
            }

            private void UpdateNoteSelectionInternal(NoteCoord startCoord, NoteCoord endCoord,
                NoteSelectionUpdateMode mode)
            {
                Debug.Assert(startCoord.Position is <= MainSystem.Args.NoteSelectionMaxPosition
                    and >= -MainSystem.Args.NoteSelectionMaxPosition);
                Debug.Assert(endCoord.Position is <= MainSystem.Args.NoteSelectionMaxPosition
                    and >= -MainSystem.Args.NoteSelectionMaxPosition);

                if (startCoord.Position > endCoord.Position) {
                    (startCoord.Position, endCoord.Position) = (endCoord.Position, startCoord.Position);
                }
                if (startCoord.Time > endCoord.Time) {
                    (startCoord.Time, endCoord.Time) = (endCoord.Time, startCoord.Time);
                }

                (float xMin, float zMin) =
                    MainSystem.Args.NoteCoordToWorldPosition(startCoord, _editor.Stage.CurrentMusicTime);
                (float xMax, float zMax) =
                    MainSystem.Args.NoteCoordToWorldPosition(endCoord, _editor.Stage.CurrentMusicTime);

                _noteSelectionIndicatorImageTransform.offsetMin = new(xMin, zMin);
                _noteSelectionIndicatorImageTransform.offsetMax = new(xMax, zMax);

                // Optimizable
                _selectedNotes.Clear();

                foreach (var note in _editor.Stage.Chart.Notes.EnumerateSelectableModels()) {
                    float notePos = note.Data.Position;
                    float halfNoteSize = note.Data.Size / 2;
                    float noteTime = note.Data.Time;
                    bool isSelected = noteTime >= startCoord.Time
                                      && noteTime <= endCoord.Time
                                      && notePos + halfNoteSize >= startCoord.Position
                                      && notePos - halfNoteSize <= endCoord.Position;

                    if (mode is NoteSelectionUpdateMode.Reselect)
                        note.IsSelected = false;
                    note.SetIsInSelection(isSelected);
                    if (isSelected)
                        _inSelectionNotes.Add(note);

                    if (note.IsSelected) {
                        _selectedNotes.Add(note);
                    }
                }

                NoteTimeComparer.AssertInOrder(_selectedNotes);
            }

            enum NoteSelectionUpdateMode
            {
                Reselect,
                Toggle,
            }
        }
    }
}