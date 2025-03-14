#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Entities.Models;
using Deenote.Library.Collections;
using Deenote.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Entities.Storage
{
    partial class ProjectIO
    {
        private static void WriteProject(BinaryWriter writer, ProjectModel project)
        {
            writer.Write(project.MusicName);
            writer.Write(project.Composer);
            writer.Write(project.ChartDesigner);
            writer.Write(project.AudioFileRelativePath);
            writer.WriteArrayWithLengthPrefix(project.AudioFileData);

            writer.Write(project.Charts.Count);
            foreach (var chart in project.Charts)
                WriteChart(writer, chart);

            writer.Write(project._tempos.Count);
            foreach (var tempo in project._tempos)
                writer.Write(tempo);
        }

        private static ProjectModel ReadProject(BinaryReader reader, string projectFilePath)
        {
            var musicName = reader.ReadString();
            var composer = reader.ReadString();
            var chartDesigner = reader.ReadString();
            var audioFileRelativePath = reader.ReadString();
            var audioFileData = reader.ReadArrayWithLengthPrefix();

            var proj = new ProjectModel(projectFilePath, audioFileData, audioFileRelativePath) {
                MusicName = musicName,
                Composer = composer,
                ChartDesigner = chartDesigner,
            };

            var chartCount = reader.ReadInt32();
            proj.Charts.EnsureCapacity(chartCount);
            for (int i = 0; i < chartCount; i++)
                proj.Charts.Add(ReadChart(reader));

            var tempoCount = reader.ReadInt32();
            proj._tempos.EnsureCapacity(tempoCount);
            for (int i = 0; i < tempoCount; i++)
                proj._tempos.Add(reader.Read<Tempo>());

            return proj;
        }

        private static void WriteChart(BinaryWriter writer, ChartModel chart)
        {
            writer.Write(chart.Name);
            writer.Write(chart.Difficulty);
            writer.Write(chart.Level);

            writer.Write(chart.Speed);
            writer.Write(chart.RemapMinVolume);
            writer.Write(chart.RemapMaxVolume);

            //writer.Write(chart._holdCount); // Not required

            using var dp_linkNotes = DictionaryPool<NoteModel, int>.Get(out var linkNotesLookup);
            int noteIndex = 0;
            foreach (var note in chart.EnumerateNoteModels()) {
                if (note.NextLink is not null)
                    linkNotesLookup.Add(note, noteIndex);
                noteIndex++;
            }
            writer.Write(noteIndex); // noteModelCount
            foreach (var note in chart.EnumerateNoteModels()) {
                WriteNote(writer, note, linkNotesLookup);
            }

            writer.Write(chart.BackgroundSoundNotes.Count);
            foreach (var note in chart.BackgroundSoundNotes) {
                WriteSoundNote(writer, note);
            }

            writer.Write(chart.SpeedChangeWarnings.Count);
            foreach (var note in chart.SpeedChangeWarnings) {
                WriteSpeedChangeWarning(writer, note);
            }

            writer.Write(chart.SpeedLines.Count);
            foreach (ref readonly var line in chart.SpeedLines.AsSpan()) {
                WriteSpeedLine(writer, line);
            }
        }

        private static ChartModel ReadChart(BinaryReader reader)
        {
            var name = reader.ReadString();
            var difficulty = reader.Read<Difficulty>();
            var level = reader.ReadString();

            var speed = reader.ReadSingle();
            var remapvmin = reader.ReadInt32();
            var remapvmax = reader.ReadInt32();

            var chart = new ChartModel(speed, remapvmin, remapvmax) {
                Name = name,
                Difficulty = difficulty,
                Level = level,
            };


            var noteCount = reader.ReadInt32();
            using var so_notes = SpanOwner<NoteModel>.Allocate(noteCount);
            using var so_links = SpanOwner<int>.Allocate(noteCount);
            var notes = so_notes.Span;
            var links = so_links.Span;
            for (int i = 0; i < notes.Length; i++) {
                notes[i] = ReadNote(reader, out var prev);
                links[i] = prev;
            }
            for (int i = 0; i < notes.Length; i++) {
                var prevLink = links[i];
                if (prevLink != -1) {
                    var note = notes[i];
                    var prevNote = notes[prevLink];
                    prevNote._nextLink = note;
                    note._prevLink = prevNote;
                }
            }
            ChartModel.Marshal.SetNoteModels(chart, notes);

            var soundCount = reader.ReadInt32();
            chart.BackgroundSoundNotes.EnsureCapacity(soundCount);
            for (int i = 0; i < soundCount; i++) {
                chart.BackgroundSoundNotes.AddFromEnd(ReadSoundNote(reader));
            }

            var speedChangeCount = reader.ReadInt32();
            chart.SpeedChangeWarnings.EnsureCapacity(speedChangeCount);
            for (int i = 0; i < speedChangeCount; i++) {
                chart.SpeedChangeWarnings.AddFromEnd(ReadSpeedChangeWarning(reader));
            }

            var lineCount = reader.ReadInt32();
            chart.SpeedLines.EnsureCapacity(lineCount);
            for (int i = 0; i < lineCount; i++) {
                chart.SpeedLines.AddFromEnd(ReadSpeedLine(reader));
            }

            return chart;
        }

        private static void WriteNote(BinaryWriter writer, NoteModel note, Dictionary<NoteModel, int> linkLookup)
        {
            writer.Write(note.Position);
            writer.Write(note.Time);
            writer.Write(note.Size);
            writer.Write(note.Duration);
            writer.Write(note.Kind);
            writer.Write(note.Speed);
            writer.Write(note.Sounds.Count);
            foreach (var sound in note.Sounds)
                WriteSound(writer, sound);
            writer.Write(note.Shift);
            writer.Write(note.EventId);
            writer.Write(note.WarningType);
            writer.Write(note.Vibrate);
            if (note.PrevLink != null)
                writer.Write(linkLookup[note.PrevLink]);
            else
                writer.Write(-1);
        }

        private static NoteModel ReadNote(BinaryReader reader, out int prevLinkIndex)
        {
            var note = new NoteModel();
            note.Position = reader.ReadSingle();
            note.Time = reader.ReadSingle();
            note.Size = reader.ReadSingle();
            note.Duration = reader.ReadSingle();
            note.Kind = reader.Read<NoteModel.NoteKind>();
            note.Speed = reader.ReadSingle();
            var soundsLen = reader.ReadInt32();
            note.Sounds.Capacity = soundsLen;
            for (int i = 0; i < soundsLen; i++)
                note.Sounds.Add(ReadSound(reader));
            note.Shift = reader.ReadSingle();
            note.EventId = reader.ReadString();
            note.WarningType = reader.Read<WarningType>();
            note.Vibrate = reader.ReadBoolean();
            prevLinkIndex = reader.ReadInt32();
            return note;
        }

        private static void WriteSoundNote(BinaryWriter writer, SoundNoteModel note)
        {
            writer.Write(note.Time);
            writer.Write(note.Sounds.Length);
            foreach (var sound in note.Sounds) {
                WriteSound(writer, sound);
            }
        }

        private static SoundNoteModel ReadSoundNote(BinaryReader reader)
        {
            var note = new NoteModel() {
                Position = 12,
            };
            var time = reader.ReadSingle();
            var len = reader.ReadInt32();
            note.Time = time;
            note.Sounds.Capacity = len;
            for (var i = 0; i < len; i++)
                note.Sounds.Add(ReadSound(reader));
            return new SoundNoteModel(note);
        }

        private static void WriteSound(BinaryWriter writer, in PianoSoundValueModel sound)
        {
            writer.Write(sound.Delay);
            writer.Write(sound.Duration);
            writer.Write(sound.Pitch);
            writer.Write(sound.Velocity);
        }

        private static PianoSoundValueModel ReadSound(BinaryReader reader)
        {
            var delay = reader.ReadSingle();
            var duration = reader.ReadSingle();
            var pitch = reader.ReadInt32();
            var velocity = reader.ReadInt32();
            return new PianoSoundValueModel(delay, duration, pitch, velocity);
        }

        private static void WriteSpeedChangeWarning(BinaryWriter writer, in SpeedChangeWarningModel warning)
        {
            writer.Write(warning.Time);
        }

        private static SpeedChangeWarningModel ReadSpeedChangeWarning(BinaryReader reader)
        {
            var time = reader.ReadSingle();
            return new SpeedChangeWarningModel(new NoteModel() {
                Position = 4f,
                Time = time,
            });
        }

        private static void WriteSpeedLine(BinaryWriter writer, in SpeedLineValueModel line)
        {
            writer.Write(line.StartTime);
            writer.Write(line.Speed);
            writer.Write(line.WarningType);
        }

        private static SpeedLineValueModel ReadSpeedLine(BinaryReader reader)
        {
            var start = reader.ReadSingle();
            var speed = reader.ReadSingle();
            var warningType = reader.Read<WarningType>();
            return new SpeedLineValueModel(start, speed, warningType);
        }
    }
}