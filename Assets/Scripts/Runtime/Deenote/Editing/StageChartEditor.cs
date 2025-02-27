#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using Deenote.GamePlay;
using Deenote.Library.Components;
using Deenote.Project;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.Editing
{
    public sealed partial class StageChartEditor : FlagNotifiableMonoBehaviour<StageChartEditor, StageChartEditor.NotificationFlag>
    {
        private const int MaxOperationUndoCount = 100;

        private ProjectManager _project = default!;
        internal GamePlayManager _game = default!;

        [SerializeField] StageNotePlacer _placer = default!;
        [SerializeField] StageNoteSelector _selector = default!;
        private UndoableOperationHistory _operations = default!;

        public StageNotePlacer Placer => _placer;
        public StageNoteSelector Selector => _selector;

        private void Awake()
        {
            _operations = new(MaxOperationUndoCount);

            Awake_ClipBoard();
        }

        private void Start()
        {
            _placer.Unity_Start();
        }

        internal void OnInstantiate(ProjectManager project, GamePlayManager game)
        {
            _project = project;
            _game = game;

            _placer = new StageNotePlacer(this);
            _selector = new StageNoteSelector(game);

            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager =>
                {
                    _operations.Clear();
                });
        }

        #region Add Remove

        private void OnNoteCollectionChanged()
        {
            _game.AssertChartLoaded();
            NoteTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
            _game.UpdateNotes(true, false);
        }

        public void AddNote(NoteCoord noteCoord, NoteModel notePrototype)
        {
            if (!_game.IsChartLoaded())
                return;

            var note = notePrototype.Clone();
            note.PositionCoord = noteCoord;
            _operations.Do(_game.CurrentChart.AddNote(note)
                .OnRedone(note =>
                {
                    _selector.Clear();
                    OnNoteCollectionChanged();
                    NoteTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
                })
                .OnUndone(note => OnNoteCollectionChanged()));
        }

        public void AddMultipleNotes(NoteCoord baseCoord, ReadOnlySpan<NoteModel> notePrototypes)
        {
            if (!_game.IsChartLoaded())
                return;
            if (notePrototypes.IsEmpty)
                return;
            if (notePrototypes.Length == 1) {
                AddNote(baseCoord, notePrototypes[0]);
                return;
            }

            var notes = new NoteModel[notePrototypes.Length];
            baseCoord -= notePrototypes[0].PositionCoord;

            for (int i = 0; i < notes.Length; i++) {
                var prototype = notePrototypes[i];
                var note = notes[i] = prototype.Clone();
                note.PositionCoord = NoteCoord.ClampPosition(baseCoord + note.PositionCoord);
            }

            NoteModel.CloneLinkDatas(notePrototypes, notes);

            _operations.Do(_game.CurrentChart.AddMultipleNotes(ImmutableCollectionsMarshal.AsImmutableArray(notes))
                .OnRedone(nodes =>
                {
                    _selector.Reselect(nodes.OfType<NoteModel>());
                    OnNoteCollectionChanged();
                })
                .OnUndone(nodes =>
                {
                    _selector.DeselectMultiple(nodes.OfType<NoteModel>());

                    NoteTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
                }));
        }

        public void RemoveNotes(ReadOnlySpan<NoteModel> notes)
        {
            if (!_game.IsChartLoaded())
                return;
            if (notes.IsEmpty)
                return;

            _operations.Do(_game.CurrentChart.RemoveOrderedNotes(notes.ToImmutableArray())
                .OnRedone(nodes =>
                {
                    _selector.Clear();
                    OnNoteCollectionChanged();
                })
                .OnUndone(nodes =>
                {
                    _selector.AddSelectMultiple(nodes.OfType<NoteModel>());
                    OnNoteCollectionChanged();
                }));
        }

        public void RemoveSelectedNotes() => RemoveNotes(_selector.SelectedNotes);

        public void AddNotesSnappingToCurve(int count, ReadOnlySpan<GridsManager.CurveApplyProperty> applyProperties = default)
        {
            if (!_game.IsChartLoaded())
                return;
            if (_game.Grids.CurveTimeInterval is not (var start, var end))
                return;

            using var so_notes = SpanOwner<NoteModel>.Allocate(count);
            var notes = so_notes.Span;
            for (int i = 0; i < count; i++) {
                var time = Mathf.Lerp(start, end, (float)(i + 1) / (count + 1));
                var pos = _game.Grids.GetCurveTransformedPosition(time)!.Value; // Wont be null as CurveTimeInterval is not null
                var note = notes[i] = _placer.ClonePlaceNotePrototype();
                note.PositionCoord = new(pos, time);
            }
            AddMultipleNotes(new NoteCoord(0f, start), notes);

            // TODO: Apply applyproperties
        }

        #endregion

        public bool HasUnsavedChange => _operations.CanUndo;

        public void UndoOperation() => _operations.Undo();

        public void RedoOperation() => _operations.Redo();

        /*
        #region Notify

        public void NotifyCurveGeneratedWithSelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.Length < 2)
                return;

            NoteModel first = SelectedNotes[0];
            NoteModel last = SelectedNotes[^1];

            _operationHistory.Do(Stage.Chart.Notes.RemoveNotes(SelectedNotes[1..^1])
                .WithRedoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.ClearSelection();
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(removedNotes =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.SelectedNotes_Changing);
                    _noteSelectionController.SelectNote(first);
                    _noteSelectionController.SelectNotes(removedNotes.OfType<NoteModel>());
                    _noteSelectionController.SelectNote(last);
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(true, true, noteDataChangedExceptTime: true);
                }));
            NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
        }

        #endregion
        */

        public enum NotificationFlag
        {
            NoteTime,
            NotePosition,
            NotePositionCoord,
            NoteSize,
            NoteShift,
            NoteSpeed,
            NoteDuration,
            NoteKind,
            NoteVibrate,
            NoteWarningType,
            NoteEventId,
            NoteSounds,

            ProjectTempo,
        }
    }
}