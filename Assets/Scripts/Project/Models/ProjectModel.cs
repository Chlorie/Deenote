using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Project.Models
{
    //[JsonObject(MemberSerialization.OptIn)]
    public sealed partial class ProjectModel
    {
        public string MusicName;
        public string Composer;
        public string ChartDesigner;

        // SaveAsRefPath indicates whether AudioFileData has value
        // but AudioFileRelativePath always has value, we need this
        // to get audio file type(by extension)
        public bool SaveAsRefPath;
        public byte[] AudioFileData;
        public string AudioFileRelativePath;

        public List<ChartModel> Charts { get; } = new();
        // string AudioType

        private List<Tempo> _tempos = new();
        public TempoListProxy Tempos => new(this);

        // NonSerialize
        private string _projectFilePath;
        public string ProjectFilePath
        {
            get => _projectFilePath;
            init => _projectFilePath = value;
            // private set in InitializationHelper
        }

        public AudioClip AudioClip { get; set; }

        public ProjectModel CloneForSave()
        {
            var proj = new ProjectModel() {
                MusicName = MusicName,
                Composer = Composer,
                ChartDesigner = ChartDesigner,
                SaveAsRefPath = SaveAsRefPath,
                AudioFileData = AudioFileData,
                AudioFileRelativePath = AudioFileRelativePath,
            };
            proj.Charts.Capacity = Charts.Count;
            foreach (var chart in Charts) {
                proj.Charts.Add(chart.CloneForSave());
            }
            proj._tempos.Capacity = _tempos.Count;
            foreach (var tempo in _tempos) {
                proj._tempos.Add(new Tempo(tempo.Bpm, tempo.StartTime));
            }
            return proj;
        }

        public readonly partial struct TempoListProxy : IReadOnlyList<Tempo>
        {
            private readonly ProjectModel _projectModel;

            public TempoListProxy(ProjectModel project) => _projectModel = project;

            public int Count => _projectModel._tempos.Count;

            public Tempo this[int index] => _projectModel._tempos[index];

            public float GetNonOverflowTempoTime(int index)
            {
                if (index >= _projectModel._tempos.Count)
                    return _projectModel.AudioClip.length;
                if (index < 0)
                    return 0f;
                return _projectModel._tempos[index].StartTime;
            }

            /// <returns>Range: [-1, Count)</returns>
            public int GetTempoIndex(float time)
            {
                int i;
                for (i = 0; i < _projectModel._tempos.Count; i++) {
                    if (time < _projectModel._tempos[i].StartTime)
                        break;
                }
                return i - 1;
            }

            /// <returns>Range: [0, Count]</returns>
            public int GetCeilingTempoIndex(float time)
            {
                int i;
                for (i = 0; i < _projectModel._tempos.Count; i++) {
                    if (time <= _projectModel._tempos[i].StartTime)
                        break;
                }
                return i;
            }

            public List<Tempo>.Enumerator GetEnumerator() => _projectModel._tempos.GetEnumerator();

            IEnumerator<Tempo> IEnumerable<Tempo>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static class InitializationHelper
        {
            public static void SetTempoList(ProjectModel model, List<Tempo> tempos)
            {
                model._tempos = tempos;
            }

            public static void SetProjectFilePath(ProjectModel model, string filePath)
            {
                model._projectFilePath = filePath;
            }
        }
    }
}