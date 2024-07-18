using Deenote.Project.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Deenote.Project
{
    partial class ProjectManager
    {
        private static bool TryLoadFromDsproj(string filePath, out ProjectModel project)
        {
            try {
                using var fs = File.OpenRead(filePath);
                var obj = LegacyProjectSerialization.Formatter.Deserialize(fs);
                project = obj switch {
                    LegacyProjectSerialization.SerializableProjectData v1 => ToVer3(v1),
                    LegacyProjectSerialization.FullProjectDataV2 v2 => ToVer3(v2),
                    _ => null,
                };
                return project is not null;
            } catch (Exception ex) {
                Debug.LogError(ex.Message);
                project = null;
                return false; ;
            }

        }

        // Copy from Chlorie
        private static LegacyProjectSerialization.FullProjectDataV2 ToVersion2(LegacyProjectSerialization.SerializableProjectData dataV1)
        {
            var dataV2 = new LegacyProjectSerialization.FullProjectDataV2 {
                project = dataV1.project,
                audioType = ".wav"
            };
            var wavEncoder = new WavEncoder {
                channel = dataV1.channel,
                frequency = dataV1.frequency,
                length = dataV1.length,
                sampleData = dataV1.sampleData
            };
            wavEncoder.EncodeToWav(out dataV2.audio);
            dataV2.project.songName = "converted audio.wav";
            return dataV2;
        }

        private static ProjectModel ToVer3(LegacyProjectSerialization.SerializableProjectData dataV1)
        {
            var proj = new ProjectModel {
                //Name = dataV1.project.name,
                MusicName = dataV1.project.songName,
                ChartDesigner = dataV1.project.songName,
                SaveAsRefPath = false,
            };

            // TODO
            return proj;
        }

        private static ProjectModel ToVer3(LegacyProjectSerialization.FullProjectDataV2 dataV2)
        {
            var proj = new ProjectModel { };
            // TODO:
            return proj;
        }

        private sealed class LegacyProjectSerialization : SerializationBinder
        {
            public static readonly BinaryFormatter Formatter = new() {
                Binder = new LegacyProjectSerialization(),
            };

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

                return Type.GetType($"{typeName}, {assemblyName}");
            }

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
        }
    }
}
