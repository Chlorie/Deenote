#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.Core.Editing
{
    partial class StageNotePlacer
    {
        private StateFlag _currentState;
        private NoteCoord _updateNoteCoord;
        private Vector2 _updateMousePosition;

        public bool IsPlacing => _currentState is StateFlag.PlacingSingleNote or StateFlag.PlacingSlides or StateFlag.PlacingPastedNotes;

        public void BeginPlaceNote(NoteCoord coord, Vector2 mousePosition)
        {
            if (!IsInPlacementArea(coord)) {
                SetIndicatorVisibility(false);
                return;
            }

            switch (_currentState) {
                case StateFlag.Idle:
                    _currentState = BeginPlaceSingleNote(coord, mousePosition);
                    break;
                case StateFlag.IdlePlacingSlides:
                    _currentState = BeginDragPlaceSlides(coord, mousePosition);
                    break;
                case StateFlag.IdlePastingNotes:
                    _currentState = BeginPasteNotes(coord, mousePosition);
                    break;
            }
            RefreshIndicatorVisibility();
        }

        public void UpdatePlaceNote(NoteCoord coord, Vector2 mousePosition)
        {
            _updateNoteCoord = coord;
            _updateMousePosition = mousePosition;

            switch (_currentState) {
                case StateFlag.Idle or StateFlag.IdlePlacingSlides:
                    UpdateMoveIndicator(coord, mousePosition);
                    break;
                case StateFlag.IdlePastingNotes or StateFlag.PlacingPastedNotes:
                    UpdatePasteNotes(coord, mousePosition);
                    break;
                case StateFlag.PlacingSingleNote:
                    UpdatePlaceSingleNote(coord, mousePosition);
                    break;
                case StateFlag.PlacingSlides:
                    UpdateDragPlaceSlides(coord, mousePosition);
                    break;
            }
            RefreshIndicatorVisibility();
        }

        public void EndPlaceNote(NoteCoord coord, Vector2 mousePosition)
        {
            switch (_currentState) {
                case StateFlag.PlacingSingleNote:
                    _currentState = EndPlaceSingleNote(coord, mousePosition);
                    break;
                case StateFlag.PlacingSlides:
                    _currentState = EndDragPlaceSlides(coord, mousePosition);
                    break;
                case StateFlag.PlacingPastedNotes:
                    _currentState = EndPasteNotes(coord, mousePosition);
                    ResetNotePrototypesToIdle();
                    break;
            }
            RefreshIndicatorVisibility();
        }

        public void CancelPlaceNote()
        {
            if (PlaceSlideModifier)
                _currentState = StateFlag.IdlePlacingSlides;
            else
                _currentState = StateFlag.Idle;
            ResetNotePrototypesToIdle();
        }

        private void SwitchPlaceSlideModifier(bool flag)
        {
            switch (_currentState, flag) {
                case (StateFlag.Idle, true):
                    _currentState = StateFlag.IdlePlacingSlides;
                    SyncPrototypes();
                    break;
                case (StateFlag.IdlePlacingSlides, false):
                    _currentState = StateFlag.Idle;
                    SyncPrototypes();
                    break;
                case (StateFlag.PlacingSingleNote, _):
                    UpdatePlaceSingleNote(_updateNoteCoord, _updateMousePosition);
                    break;
            }

            void SyncPrototypes()
            {
                _prototypes[0].Kind = _metaPrototype.Kind;
                _indicators[0].Refresh();
            }
        }

        private void ResetNotePrototypesToIdle()
        {
            var note = _prototypes[0];
            _metaPrototype.CloneDataTo(note, true);
            note.UnlinkWithoutCutChain(keepNoteKind: true);
            _prototypes.SetCount(1);
            RefreshIndicators();
            SetPlacingNoteSpeed(null, false);
        }

        public void DisablePlaceNote()
        {
            SetIndicatorVisibility(false);
        }

        internal void PreparePasteClipBoard()
        {
            if (_editor.ClipBoard.Notes.IsEmpty)
                return;

            if (_currentState is StateFlag.Idle or StateFlag.IdlePlacingSlides) {
                _currentState = StateFlag.IdlePastingNotes;
                using (var resetter = _prototypes.Resetting(_editor.ClipBoard.Notes.Length)) {
                    var baseCoord = _editor.ClipBoard.BaseCoord;
                    foreach (var cnote in _editor.ClipBoard.Notes) {
                        resetter.Add(out var note);
                        cnote.CloneDataTo(note, cloneSounds: true);
                    }
                }
                RefreshIndicators();
                SetPlacingNoteSpeed(_prototypes[0].Speed, false);
            }
        }

        private partial void UpdateMoveIndicator(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginPlaceSingleNote(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdatePlaceSingleNote(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndPlaceSingleNote(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginDragPlaceSlides(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdateDragPlaceSlides(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndDragPlaceSlides(NoteCoord coord, Vector2 mousePosition);

        private partial StateFlag BeginPasteNotes(NoteCoord coord, Vector2 mousePosition);
        private partial void UpdatePasteNotes(NoteCoord coord, Vector2 mousePosition);
        private partial StateFlag EndPasteNotes(NoteCoord coord, Vector2 mousePosition);

        private bool IsForceShowIndicator()
        {
            return _currentState is not (StateFlag.Disabled or StateFlag.Idle);
        }

        private bool IsFreezeNotePrototypeProperties()
        {
            return _currentState is StateFlag.IdlePastingNotes or StateFlag.PlacingPastedNotes;
        }

        public enum StateFlag
        {
            /// <summary>
            /// When the indicator is forced to be hidden, and all placement will be disabled
            /// </summary>
            Disabled,
            Idle,
            IdlePlacingSlides,
            IdlePastingNotes,
            PlacingSingleNote,
            PlacingSlides,
            PlacingPastedNotes,
        }
    }
}