#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using Trarizon.Library.Collections;
using Trarizon.Library.Collections.Generic;
using UnityEngine;

namespace Deenote.Entities.Models
{
    partial class ChartModel
    {
        internal static class Marshal
        {
            /// <remarks>
            /// Return collection will be null if <paramref name="notes"/> is empty, to reduce allocation
            /// </remarks>
            public static void DeserializeNotes(ReadOnlySpan<NoteModel> notes,
                out int holdCount,
                out List<IStageNoteNode>? visibleNotes,
                out List<SoundNoteModel>? backgroundNotes,
                out List<SpeedChangeWarningModel>? speedChangeWarnings)
            {
                if (notes.IsEmpty) {
                    holdCount = 0;
                    visibleNotes = null;
                    backgroundNotes = null;
                    speedChangeWarnings = null;
                    return;
                }

                holdCount = 0;
                visibleNotes = new List<IStageNoteNode>();
                backgroundNotes = new List<SoundNoteModel>();
                speedChangeWarnings = new List<SpeedChangeWarningModel>();
                var tailsBuffer = new List<NoteTailNode>();

                foreach (var note in notes) {
                    if (note.IsVisibleOnStage()) {
                        while (tailsBuffer.TryAt(0, out var tail) && tail.Time < note.Time) {
                            visibleNotes.Add(tail);
                            holdCount++;
                            tailsBuffer.RemoveAt(0);
                        }
                        visibleNotes.Add(note);
                        if (note.IsHold) {
                            tailsBuffer.GetSortedModifier(NoteTimeComparer.Instance)
                                .Add(new NoteTailNode(note));
                        }
                    }
                    else if (note.WarningType is WarningType.SpeedChange) {
                        speedChangeWarnings.Add(new SpeedChangeWarningModel(note));
                    }
                    else {
                        backgroundNotes.Add(new SoundNoteModel(note));
                    }
                }

                if (tailsBuffer.Count > 0) {
                    foreach (var tail in tailsBuffer) {
                        visibleNotes.Add(tail);
                    }
                }

                NoteTimeComparer.AssertInOrder(visibleNotes);
                NoteTimeComparer.AssertInOrder(backgroundNotes);
            }

            public static void SetNoteModels(ChartModel chart, ReadOnlySpan<NoteModel> notes)
            {
                if (notes.IsEmpty) {
                    chart._holdCount = 0;
                    chart._visibleNoteNodes = new();
                    return;
                }

                var holdCount = 0;
                var noteNodes = new SortedList<IStageNoteNode>(NoteTimeComparer.Instance);

                foreach (var note in notes) {
                    Debug.Assert(note.IsVisibleOnStage());
                    noteNodes.AddFromEnd(note);
                    if (note.IsHold) {
                        holdCount++;
                        noteNodes.AddFromEnd(new NoteTailNode(note));
                    }
                }

                chart._holdCount = holdCount;
                chart._visibleNoteNodes = new List<IStageNoteNode>(noteNodes);

                NoteTimeComparer.AssertInOrder(chart._visibleNoteNodes);
            }

            public static void ResetNotes(ChartModel chart, ReadOnlySpan<NoteModel> notes)
            {
                DeserializeNotes(notes, out var holdCount, out var visibleNotes, out var backgroundNotes, out var speedChangeWarnings);
                chart._holdCount = holdCount;
                if (visibleNotes is null) {
                    if (chart._visibleNoteNodes.Count > 0)
                        chart._visibleNoteNodes = new();
                }
                else {
                    chart._visibleNoteNodes = visibleNotes;
                }
                if (backgroundNotes is null) {
                    if (chart._backgroundNotes.Count > 0)
                        chart._backgroundNotes = new();
                }
                else {
                    chart._backgroundNotes = backgroundNotes;
                }

                if (speedChangeWarnings is null) {
                    if (chart._speedChangeWarnings.Count > 0)
                        chart._speedChangeWarnings = new();
                }
                else {
                    chart._speedChangeWarnings = speedChangeWarnings;
                }
            }
        }
    }
}