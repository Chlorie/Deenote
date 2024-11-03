#nullable enable

using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Deenote.Project
{
    partial class ProjectManager
    {
        private static bool TryLoadFromDsproj(string filePath, [NotNullWhen(true)] out ProjectModel? project)
        {
            try {
                using var fs = File.OpenRead(filePath);
                var obj = LegacyProjectSerialization.Formatter.Deserialize(fs);
                project = obj switch {
                    LegacyProjectSerialization.SerializableProjectData v1 => LegacyProjectSerialization.ToVer3(v1, filePath),
                    LegacyProjectSerialization.FullProjectDataV2 v2 => LegacyProjectSerialization.ToVer3(v2, filePath),
                    _ => null,
                };
                if (project is not null) {
                    ProjectModel.InitializationHelper.SetProjectFilePath(project, filePath);
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
                var project = new ProjectModel {
                    MusicName = oldProject.name,
                    ChartDesigner = oldProject.chartMaker,

                    AudioFileRelativePath = audioFileRelativePath,
                    ProjectFilePath = projectFilePath, // Late init
                };

                Debug.Assert(oldProject.charts.Length == 4);
                foreach (var oldChart in oldProject.charts) {
                    if (oldChart.notes.Count == 0)
                        continue;
                    var chartModel = new ChartModel(ToVer3(oldChart)) {
                        Difficulty = DifficultyExt.FromInt32(oldChart.difficulty),
                        Level = oldChart.level,
                    };
                    project.Charts.Add(chartModel);
                }

                List<Tempo> tempos = oldProject.charts[0].beats
                    .Select(time => new Tempo(bpm: 0f, time))
                    .ToList();
                ProjectModel.InitializationHelper.SetTempoList(project, tempos);

                return project;
            }

            private static ChartData ToVer3(Chart oldChart)
            {
                var chart = new ChartData { Speed = oldChart.speed, RemapMinVelocity = 10, RemapMaxVelocity = 70, };
                chart.Notes.AddRange(oldChart.notes.Select(ToVer3));
                for (int i = 0; i < chart.Notes.Count; i++) {
                    var oldNote = oldChart.notes[i];
                    var note = chart.Notes[i];
                    if (oldNote.prevLink != -1)
                        note.PrevLink = chart.Notes[oldNote.prevLink];
                    if (oldNote.nextLink != -1)
                        note.NextLink = chart.Notes[oldNote.nextLink];
                }
                return chart;
            }

            private static NoteData ToVer3(Note oldNote)
            {
                var note = new NoteData {
                    Position = oldNote.position,
                    Size = oldNote.size,
                    Time = oldNote.time,
                    Shift = oldNote.shift,
                    IsSlide = oldNote.isLink,
                    Sounds = { Capacity = oldNote.sounds.Capacity }
                };
                foreach (var sound in oldNote.sounds) {
                    note.Sounds.Add(ToVer3(sound));
                }
                return note;
            }

            private static PianoSoundData ToVer3(PianoSound oldSound)
            {
                return new PianoSoundData(
                    oldSound.delay,
                    oldSound.duration,
                    oldSound.pitch,
                    oldSound.volume);
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
}