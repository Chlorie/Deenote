using Cysharp.Threading.Tasks;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine.Pool;

namespace Deenote.Project
{
    partial class ProjectManager
    {
        private static UniTask<ProjectModel?> LoadAsync(string projectFilePath)
            => UniTask.RunOnThreadPool(() => Load(projectFilePath));

        private static ProjectModel? Load(string projectFilePath)
        {
            if (projectFilePath.EndsWith(".dsproj")) {
                if (TryLoadFromDsproj(projectFilePath, out var dsprojProject))
                    return dsprojProject;
            }

            using var fs = File.OpenRead(projectFilePath);
            using var br = new BinaryReader(fs);

            var header = br.ReadUInt16();
            if (header != 0xDEE0)
                return null;

            var version = br.ReadByte();
            if (version != 1)
                return null;

            return IOHelper.ReadProject(br);
        }

        private static UniTask SaveAsync(ProjectModel project, string saveFilePath)
        {
            var saveProj = project.CloneForSave();
            return UniTask.RunOnThreadPool(() => Save(saveProj, saveFilePath));
        }

        private static void Save(ProjectModel project, string saveFilePath)
        {
            using var fs = File.OpenWrite(saveFilePath);
            using var bw = new BinaryWriter(fs);

            bw.Write(MainSystem.Args.DeenoteProjectFileHeader);
            bw.Write(MainSystem.Args.DeenoteProjectFileVersionMark);
            IOHelper.WriteProject(bw, project);
        }

        private static class IOHelper
        {
            public static void WriteProject(BinaryWriter writer, ProjectModel project)
            {
                writer.Write(project.MusicName);
                writer.Write(project.Composer);
                writer.Write(project.ChartDesigner);
                writer.Write(project.SaveAsRefPath);
                if (project.SaveAsRefPath) {
                    writer.Write(project.AudioFileRelativePath);
                }
                else {
                    writer.Write(project.AudioFileData.Length);
                    writer.Write(project.AudioFileData);
                }
                writer.Write(project.Charts.Count);
                foreach (var chart in project.Charts) {
                    WriteChart(writer, chart);
                }
                writer.Write(project.Tempos.Count);
                foreach (var tempo in project.Tempos) {
                    writer.Write(tempo.Bpm);
                    writer.Write(tempo.StartTime);
                }
            }

            public static ProjectModel ReadProject(BinaryReader reader)
            {
                var project = new ProjectModel {
                    MusicName = reader.ReadString(),
                    Composer = reader.ReadString(),
                    ChartDesigner = reader.ReadString(),
                    SaveAsRefPath = reader.ReadBoolean(),
                };
                if (project.SaveAsRefPath) {
                    project.AudioFileRelativePath = reader.ReadString();
                }
                else {
                    var len = reader.ReadInt32();
                    project.AudioFileData = reader.ReadBytes(len);
                }
                var chartLen = reader.ReadInt32();
                project.Charts.Capacity = chartLen;
                for (int i = 0; i < chartLen; i++) {
                    project.Charts.Add(ReadChart(reader));
                }
                var tempoLen = reader.ReadInt32();
                var tempos = new List<Tempo>(tempoLen);
                for (int i = 0; i < tempoLen; i++) {
                    var bpm = reader.ReadSingle();
                    var startTime = reader.ReadSingle();
                    tempos.Add(new Tempo(bpm, startTime));
                }
                ProjectModel.InitializationHelper.SetTempoList(project, tempos);

                return project;
            }

            private static void WriteChart(BinaryWriter writer, ChartModel chart)
            {
                writer.Write(chart.Difficulty.ToInt32());
                writer.Write(chart.Level);
                WriteChartData(writer, chart.Data);
            }

            private static ChartModel ReadChart(BinaryReader reader)
            {
                var diff = (Difficulty)reader.ReadInt32();
                var level = reader.ReadString();
                var data = ReadChartData(reader);

                return new ChartModel(data) { Difficulty = diff, Level = level, };
            }

            private static void WriteChartData(BinaryWriter writer, ChartData chart)
            {
                writer.Write(chart.Speed);
                writer.Write(chart.MinVelocity);
                writer.Write(chart.MaxVelocity);
                writer.Write(chart.RemapMinVelocity);
                writer.Write(chart.RemapMaxVelocity);

                using var nDict = DictionaryPool<NoteData, int>.Get(out var linkNotes);
                writer.Write(chart.Notes.Count);
                for (int i = 0; i < chart.Notes.Count; i++) {
                    NoteData note = chart.Notes[i];
                    if (note.IsSlide && note.NextLink is not null)
                        linkNotes.Add(note, i);
                }
                foreach (var note in chart.Notes) {
                    WriteNoteData(writer, note, linkNotes);
                }

                writer.Write(chart.SpeedLines.Count);
                foreach (var line in chart.SpeedLines)
                    WriteSpeedLineData(writer, line);
            }

            private static ChartData ReadChartData(BinaryReader reader)
            {
                var chart = new ChartData() {
                    Speed = reader.ReadInt32(),
                    MinVelocity = reader.ReadInt32(),
                    MaxVelocity = reader.ReadInt32(),
                    RemapMinVelocity = reader.ReadInt32(),
                    RemapMaxVelocity = reader.ReadInt32(),
                };

                var notesLen = reader.ReadInt32();
                chart.Notes.Capacity = notesLen;
                for (int i = 0; i < notesLen; i++)
                    chart.Notes.Add(ReadNoteData(reader, chart.Notes));

                var linesLen = reader.ReadInt32();
                chart.SpeedLines.Capacity = linesLen;
                for (int i = 0; i < linesLen; i++)
                    chart.SpeedLines.Add(ReadSpeedLineData(reader));

                return chart;
            }

            private static void WriteNoteData(BinaryWriter writer, NoteData note, Dictionary<NoteData, int> linkDict)
            {
                writer.Write(note.Sounds.Count);
                foreach (var sound in note.Sounds)
                    WriteSoundData(writer, sound);
                writer.Write(note.Position);
                writer.Write(note.Size);
                writer.Write(note.Time);
                writer.Write(note.Shift);
                writer.Write(note.Speed);
                writer.Write(note.Duration);
                writer.Write(note.Vibrate);
                writer.Write(note.IsSwipe);
                writer.Write((int)note.WarningType);
                writer.Write(note.EventId);
                writer.Write(note.IsSlide);
                if (note.PrevLink != null)
                    writer.Write(linkDict[note.PrevLink]);
                else
                    writer.Write(-1);
            }

            private static NoteData ReadNoteData(BinaryReader reader, List<NoteData> readedNotes)
            {
                var note = new NoteData();
                var soundsLen = reader.ReadInt32();
                note.Sounds.Capacity = soundsLen;
                for (int i = 0; i < soundsLen; i++)
                    note.Sounds.Add(ReadSoundData(reader));
                note.Position = reader.ReadSingle();
                note.Size = reader.ReadSingle();
                note.Time = reader.ReadSingle();
                note.Shift = reader.ReadSingle();
                note.Speed = reader.ReadSingle();
                note.Duration = reader.ReadSingle();
                note.Vibrate = reader.ReadBoolean();
                note.IsSwipe = reader.ReadBoolean();
                note.WarningType = (WarningType)reader.ReadInt32();
                note.EventId = reader.ReadString();
                note.IsSlide = reader.ReadBoolean();
                var prev = reader.ReadInt32();
                if (prev != -1) {
                    var prevNote = readedNotes[prev];
                    Debug.Assert(prevNote.IsSlide);
                    prevNote.NextLink = note;
                    note.PrevLink = prevNote;
                }
                return note;
            }

            private static void WriteSoundData(BinaryWriter writer, PianoSoundData sound)
            {
                writer.Write(sound.Delay);
                writer.Write(sound.Duration);
                writer.Write(sound.Pitch);
                writer.Write(sound.Velocity);
            }

            private static PianoSoundData ReadSoundData(BinaryReader reader)
            {
                var delay = reader.ReadSingle();
                var duration = reader.ReadSingle();
                var pitch = reader.ReadInt32();
                var velocity = reader.ReadInt32();
                return new PianoSoundData(delay, duration, pitch, velocity);
            }

            private static void WriteSpeedLineData(BinaryWriter writer, SpeedLine line)
            {
                writer.Write(line.Speed);
                writer.Write(line.StartTime);
                writer.Write(line.EndTime);
                writer.Write((int)line.WarningType);
            }

            private static SpeedLine ReadSpeedLineData(BinaryReader reader)
            {
                var speed = reader.ReadSingle();
                var startTime = reader.ReadSingle();
                var endTime = reader.ReadSingle();
                var warningType = (WarningType)reader.ReadInt32();
                return new SpeedLine(speed, startTime, endTime, warningType);
            }
        }
    }
}