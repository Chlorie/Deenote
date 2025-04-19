#nullable enable

using Deenote.Entities.Comparisons;
using Deenote.Library.Collections;
using Deenote.Library.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
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
            public static void DeserializeNotesOld(ReadOnlySpan<NoteModel> notes,
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
                            tailsBuffer.GetSortedModifier(NodeTimeComparer.Instance)
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

                NodeTimeComparer.AssertInOrder(visibleNotes);
                NodeTimeComparer.AssertInOrder(backgroundNotes);
            }

            public static void DeserializeNotes(ReadOnlySpan<NoteModel> notes,
                out int holdCount,
                out SortedList<IStageNoteNode>? visibleNotes,
                out SortedList<SoundNoteModel>? soundNotes,
                out SortedList<SpeedChangeWarningModel>? speedChangeWarnings)
            {
                if (notes.IsEmpty) {
                    holdCount = 0;
                    visibleNotes = null;
                    soundNotes = null;
                    speedChangeWarnings = null;
                    return;
                }

                holdCount = 0;
                visibleNotes = new SortedList<IStageNoteNode>(NodeTimeUniqueComparer.Instance);
                soundNotes = new SortedList<SoundNoteModel>(NodeTimeComparer.Instance);
                speedChangeWarnings = new SortedList<SpeedChangeWarningModel>(NodeTimeComparer.Instance);

                foreach (var note in notes) {
                    if (note.IsVisibleOnStage()) {
                        visibleNotes.AddFromEnd(note);
                        if (note.IsHold) {
                            holdCount++;
                            visibleNotes.AddFromEnd(new NoteTailNode(note));
                        }
                    }
                    else if (note.WarningType is WarningType.SpeedChange) {
                        speedChangeWarnings.AddFromEnd(new SpeedChangeWarningModel(note));
                    }
                    else {
                        soundNotes.Add(new SoundNoteModel(note));
                    }
                }
            }

            public static void SetNoteModels(ChartModel chart, ReadOnlySpan<NoteModel> notes)
            {
                if (notes.IsEmpty) {
                    chart._holdCount = 0;
                    chart.NoteNodes = new(NodeTimeUniqueComparer.Instance);
                    return;
                }

                var holdCount = 0;

                foreach (var note in notes) {
                    Debug.Assert(note.IsVisibleOnStage());
                    chart.NoteNodes.AddFromEnd(note);
                    if (note.IsHold) {
                        holdCount++;
                        chart.NoteNodes.AddFromEnd(new NoteTailNode(note));
                    }
                }

                chart._holdCount = holdCount;
            }

            public static void ResetNotes(ChartModel chart, ReadOnlySpan<NoteModel> notes)
            {
                DeserializeNotes(notes, out var holdCount, out var stageNotes, out var backgroundNotes, out var speedChangeWarnings);
                chart._holdCount = holdCount;
                if (stageNotes is null) {
                    if (chart.NoteNodes.Count > 0)
                        chart.NoteNodes.Clear();
                }
                else {
                    chart.NoteNodes = stageNotes;
                }
                if (backgroundNotes is null) {
                    if (chart.BackgroundSoundNotes.Count > 0)
                        chart.BackgroundSoundNotes.Clear();
                }
                else {
                    chart.BackgroundSoundNotes = backgroundNotes;
                }

                if (speedChangeWarnings is null) {
                    if (chart.SpeedChangeWarnings.Count > 0)
                        chart.SpeedChangeWarnings.Clear();
                }
                else {
                    chart.SpeedChangeWarnings = speedChangeWarnings;
                }
            }
        }
    }
}