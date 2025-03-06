#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Deenote.Entities.Storage
{
    internal static class DsprojLoader
    {
        public static bool TryLoadDsproj(string filePath, [NotNullWhen(true)] out ProjectModel? project)
        {
            try {
                using var fs = File.OpenRead(filePath);
                var obj = LegacyProjectSerialization.Formatter.Deserialize(fs);
                project = obj switch {
                    SerializableProjectData v1 => LegacyProjectSerialization.ToVer3(v1, filePath),
                    FullProjectDataV2 v2 => LegacyProjectSerialization.ToVer3(v2, filePath),
                    _ => null,
                };
                if (project is not null) {
                    project.ProjectFilePath = filePath;
                    return true;
                }
                else {
                    return false;
                }
            } catch (Exception ex) {
                Debug.LogError(ex.Message);
                project = null;
                return false;
            }
        }

        private sealed class LegacyProjectSerialization : SerializationBinder
        {
            public static readonly BinaryFormatter Formatter = new() { Binder = new LegacyProjectSerialization(), };

            private LegacyProjectSerialization() { }

            public override Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName.StartsWith("Assembly-CSharp")) {
                    return typeName switch {
                        "FullProjectDataV2" => typeof(FullProjectDataV2),
                        "SerializableProjectData" => typeof(SerializableProjectData),
                        "Project" => typeof(Project),
                        "Chart" => typeof(Chart),
                        "Note" => typeof(Note),
                        "PianoSound" => typeof(PianoSound),
                        _ => throw new InvalidOperationException("Unknown type"),
                    };
                }

                if (typeName.StartsWith("System.Collections.Generic.List`1")) {
                    var typeFullName = typeName.AsSpan("System.Collections.Generic.List`1".Length + 2);
                    var comma = typeFullName.IndexOf(',');
                    var assName = typeFullName[(comma + 2)..];
                    if (assName.StartsWith("Assembly-CSharp")) {
                        var tyName = typeFullName[..comma];
                        return tyName.ToString() switch {
                            "Note" => typeof(List<Note>),
                            "PianoSound" => typeof(List<PianoSound>),
                            _ => throw new InvalidOperationException("Unknown type"),
                        };
                    }
                }

                return Type.GetType($"{typeName}, {assemblyName}")!;
            }

            // Copy from Chlorie
            public static FullProjectDataV2 ToVersion2(SerializableProjectData dataV1)
            {
                var dataV2 = new FullProjectDataV2 { project = dataV1.project, audioType = ".wav" };
                AudioUtils.EncodeToWav(dataV1.channel, dataV1.frequency, dataV1.length, dataV1.sampleData,
                    out dataV2.audio);
                dataV2.project.songName = "converted audio.wav";
                return dataV2;
            }

            public static ProjectModel ToVer3(SerializableProjectData dataV1, string projectFilePath)
            {
                var proj = ToVer3(dataV1.project, projectFilePath, $"Embeded Audio.wav");
                proj.Composer = "Unknown";

                AudioUtils.EncodeToWav(dataV1.channel, dataV1.frequency, dataV1.length, dataV1.sampleData,
                    out var audioFileData);
                proj.AudioFileData = audioFileData;

                return proj;
            }

            public static ProjectModel ToVer3(FullProjectDataV2 dataV2, string projectFilePath)
            {
                var proj = ToVer3(dataV2.project, projectFilePath, $"Embedded Audio{dataV2.audioType}");
                proj.Composer = "Unknown";
                proj.AudioFileData = dataV2.audio;

                return proj;
            }

            private static ProjectModel ToVer3(Project oldProject, string projectFilePath, string audioFileRelativePath)
            {
                var project = new ProjectModel(projectFilePath, null!, audioFileRelativePath) {
                    MusicName = oldProject.name,
                    ChartDesigner = oldProject.chartMaker,
                };

                Debug.Assert(oldProject.charts.Length == 4);
                for (int i = 0; i < oldProject.charts.Length; i++) {
                    Chart? oldChart = oldProject.charts[i];
                    if (oldChart.notes.Count == 0)
                        continue;
                    var chartModel = ToVer3(oldChart);
                    chartModel.Difficulty = ToDifficulty(i);
                    chartModel.Level = oldChart.level;
                    project.Charts.Add(chartModel);
                }

                project._tempos = oldProject.charts[0].beats
                    .Adjacent()
                    .Select(beat => new Tempo(60f / (beat.Item2 - beat.Item1), beat.Item1))
                    .ToList();

                return project;
            }

            private static Difficulty ToDifficulty(int value)
                => DifficultyExt.FromInt32Index(value);

            private static ChartModel ToVer3(Chart oldChart)
            {
                var chart = new ChartModel(speed: oldChart.speed);

                using (var notes_so = SpanOwner<NoteModel>.Allocate(oldChart.notes.Count)) {
                    var notes = notes_so.Span;
                    for (int i = 0; i < oldChart.notes.Count; i++) {
                        notes[i] = ToVer3(oldChart.notes[i]);
                    }

                    for (int i = 0; i < notes.Length; i++) {
                        var old = oldChart.notes[i];
                        var note = notes[i];
                        if (old.prevLink != -1)
                            note._prevLink = notes[old.prevLink];
                        if (old.nextLink != -1)
                            note._nextLink = notes[old.nextLink];
                    }

                    ChartModel.Marshal.ResetNotes(chart, notes);
                }

                return chart;
            }

            private static NoteModel ToVer3(Note oldNote)
            {
                var note = new NoteModel {
                    Position = oldNote.position,
                    Time = oldNote.time,
                    Size = oldNote.size,
                    Shift = oldNote.shift,
                    Kind = oldNote.isLink ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click,
                };

                note.Sounds.Capacity = oldNote.sounds.Capacity;
                foreach (var s in oldNote.sounds) {
                    note.Sounds.Add(ToVer3(s));
                }
                return note;
            }

            private static PianoSoundValueModel ToVer3(PianoSound oldSound)
            {
                return new PianoSoundValueModel(
                    oldSound.delay,
                    oldSound.duration,
                    oldSound.pitch,
                    oldSound.volume);
            }

        }

        #region Types
#nullable disable
#pragma warning disable CS0649

        [Serializable]
        public class SerializableProjectData //Saves all the project data including the audio clip data
        {
            public Project project; //Other project data
                                    //Audio clip data
            public float[] sampleData;
            public int frequency;
            public int channel;
            public int length; //Length is in samples
        }

        [Serializable]
        public class FullProjectDataV2 // Version 2, saves audio in a byte array
        {
            public Project project;
            public byte[] audio;
            public string audioType;
        }

        [Serializable]
        public class Project //Saves the project info and settings, audio clip not included
        {
            //Project info
            public string name; //The name of the project
            public string chartMaker; //The name of the chart maker
            public string songName; //The name of the song (the file)(includes extension) - cannot be changed after creating the project
            public Chart[] charts; //Charts in the project
        }

        [Serializable]
        public class Chart // Saves info of a single chart
        {
            public float speed;
            public int difficulty;
            public string level;
            public List<float> beats; // For quantizing the note
            public List<Note> notes;
        }

        [Serializable]
        public class Note //A class that saves info of a note
        {
            public float position; //pos
            public float size; //size
            public float time; //_time
            public float shift; //shift
            public List<PianoSound> sounds; //sounds
            public bool isLink; //Whether the note is a link note
                                //The two variables below are not used when isLink=false
            public int prevLink; //Index of previous link note in the same link, -1 means current note is the first
            public int nextLink; //Index of next link note in the same link, -1 means current note is the last
        }

        [Serializable]
        public class PianoSound //Saves info of a piano sound
        {
            public float delay; //w
            public float duration; //d
            public int pitch; //p
            public int volume; //v
        }
#nullable restore
        #endregion
    }
}