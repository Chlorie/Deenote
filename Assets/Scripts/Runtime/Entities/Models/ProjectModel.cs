#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Entities.Models
{
    public sealed partial class ProjectModel
    {
        public string MusicName { get; set; } = "";
        public string Composer { get; set; } = "";
        public string ChartDesigner { get; set; } = "";

        public byte[] AudioFileData { get; set; }

        /// <summary>
        /// Relative to ProjectFilePath, is not used if not <see cref="SaveAsRefPath"/>, but
        /// still requires this to get the audio type (file extension)
        /// </summary>
        public string AudioFileRelativePath { get; set; }

        public List<ChartModel> Charts { get; } = new();
        internal List<Tempo> _tempos = new();

        public ReadOnlySpan<Tempo> Tempos => _tempos.AsSpan();

        // Non Serialize

        public string ProjectFilePath { get; internal set; }

        public float? AudioLength { get; set; }

        public ProjectModel(string projectFilePath, byte[] audioFileData, string audioFileRelativePath)
        {
            AudioFileData = audioFileData;
            AudioFileRelativePath = audioFileRelativePath;
            ProjectFilePath = projectFilePath;
        }

        public ProjectModel CloneForSave()
        {
            var proj = new ProjectModel(ProjectFilePath, AudioFileData, AudioFileRelativePath) {
                MusicName = MusicName,
                Composer = Composer,
                ChartDesigner = ChartDesigner,
            };
            proj.Charts.Capacity = Charts.Count;
            foreach (var chart in Charts) {
                proj.Charts.Add(chart.Clone());
            }
            proj._tempos.Capacity = _tempos.Count;
            foreach (var tempo in _tempos) {
                proj._tempos.Add(new Tempo(tempo.Bpm, tempo.StartTime));
            }
            return proj;
        }
    }
}