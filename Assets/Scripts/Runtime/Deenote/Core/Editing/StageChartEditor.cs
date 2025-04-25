#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using Deenote.Library.Collections.Generic;
using Deenote.Library.Components;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.Core.Editing
{
    public sealed partial class StageChartEditor : FlagNotifiableMonoBehaviour<StageChartEditor, StageChartEditor.NotificationFlag>
    {

        internal ProjectManager _project = default!;
        internal GamePlayManager _game = default!;

        private OperationMemento _operations = default!;

        public StageNotePlacer Placer { get; private set; } = default!;
        public StageNoteSelector Selector { get; private set; } = default!;
        public OperationMemento OperationMemento => _operations;

        private void Awake()
        {
            _operations = new();

            Awake_ClipBoard();

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Set("editor/indicator", Placer.IsIndicatorOn);
                configs.Set("editor/snap_pos", Placer.SnapToPositionGrid);
                configs.Set("editor/snap_time", Placer.SnapToTimeGrid);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                Placer.IsIndicatorOn = configs.GetBoolean("editor/indicator", true);
                Placer.SnapToPositionGrid = configs.GetBoolean("editor/snap_pos", true);
                Placer.SnapToTimeGrid = configs.GetBoolean("editor/snap_time", true);
            };
            MainSystem.ProjectManager.ProjectSaved += args =>
            {
                OperationMemento.SaveAtCurrent();
            };
        }

        internal void OnInstantiate(ProjectManager project, GamePlayManager game)
        {
            _project = project;
            _game = game;

            Placer = new StageNotePlacer(this);
            Selector = new StageNoteSelector(game);

            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager =>
                {
                    _operations.Reset();
                });
        }

        #region Add Remove

        private void OnNoteCollectionChanged()
        {
            _game.AssertChartLoaded();
            NodeTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
            _game.UpdateNotes(true, false);
        }

        public void AddNote(NoteModel notePrototype, NoteCoord noteCoord)
        {
            if (!_game.IsChartLoaded())
                return;

            var note = notePrototype.Clone();
            note.PositionCoord = noteCoord;
            _operations.Do(_game.CurrentChart.AddNote(note)
                .OnRedone(note =>
                {
                    this.Selector.Clear();
                    OnNoteCollectionChanged();
                    NodeTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
                })
                .OnUndone(note => OnNoteCollectionChanged()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notePrototypes"></param>
        /// <param name="baseCoord">The coord that first note will be created</param>
        public void AddMultipleNotes(ReadOnlySpan<NoteModel> notePrototypes, NoteCoord baseCoord)
        {
            if (!_game.IsChartLoaded())
                return;
            if (notePrototypes.IsEmpty)
                return;
            if (notePrototypes.Length == 1) {
                AddNote(notePrototypes[0], baseCoord);
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
                .OnRedone(notes =>
                {
                    this.Selector.Reselect(notes);
                    OnNoteCollectionChanged();
                })
                .OnUndone(notes =>
                {
                    this.Selector.DeselectMultiple(notes);
                    OnNoteCollectionChanged();
                    NodeTimeComparer.AssertInOrder(_game.CurrentChart.NoteNodes);
                }));
        }

        public void RemoveNote(NoteModel note) => RemoveNotes(MemoryMarshal.CreateReadOnlySpan(ref note, 1));

        public void RemoveNotes(ReadOnlySpan<NoteModel> notes)
        {
            if (!_game.IsChartLoaded())
                return;
            if (notes.IsEmpty)
                return;

            _operations.Do(GetRemoveNotesOperation(notes));
        }

        public IUndoableOperation GetRemoveNotesOperation(ReadOnlySpan<NoteModel> notes)
        {
            _game.AssertChartLoaded();

            return _game.CurrentChart.RemoveOrderedNotes(notes.ToImmutableArray())
                .OnRedone(notes =>
                {
                    this.Selector.Clear();
                    OnNoteCollectionChanged();
                })
                .OnUndone(notes =>
                {
                    this.Selector.AddSelectMultiple(notes);
                    OnNoteCollectionChanged();
                });
        }

        public void RemoveSelectedNotes() => RemoveNotes(Selector.SelectedNotes);

        public void AddNotesSnappingToCurve(int count, ReadOnlySpan<GridsManager.CurveApplyProperty> applyProperties = default)
        {
            if (!_game.IsChartLoaded())
                return;
            if (_game.Grids.CurveTimeInterval is not (var start, var end))
                return;

            bool applySize = false, applySpeed = false;
            foreach (var apply in applyProperties) {
                if (apply is GridsManager.CurveApplyProperty.Size)
                    applySize = true;
                if (apply is GridsManager.CurveApplyProperty.Speed)
                    applySpeed = true;
            }

            using var so_notes = SpanOwner<NoteModel>.Allocate(count);
            var notes = so_notes.Span;
            for (int i = 0; i < count; i++) {
                var time = Mathf.Lerp(start, end, (float)(i + 1) / (count + 1));
                var pos = _game.Grids.GetCurveTransformedPosition(time)!.Value; // Wont be null as CurveTimeInterval is not null
                var note = notes[i] = Placer.ClonePlaceNotePrototype();
                note.PositionCoord = new(pos, time);
                if (applySize)
                    note.Size = _game.Grids.GetCurveTransformedValue(time, GridsManager.CurveApplyProperty.Size)!.Value;
                if (applySpeed)
                    note.Speed = _game.Grids.GetCurveTransformedValue(time, GridsManager.CurveApplyProperty.Speed)!.Value;
            }
            AddMultipleNotes(notes, notes[0].PositionCoord);
        }

        #endregion

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