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

        public bool SaveAsRefPath;
        public byte[] AudioData;
        public string AudioFileRelativePath;

        public List<ChartModel> Charts { get; } = new();
        // string AudioType

        private List<Tempo> _tempos = new();
        public TempoListProxy Tempos => new(this);

        // NonSerialize
        public AudioClip AudioClip { get; set; }

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

            public int GetTempoIndex(float time)
            {
                int i;
                for (i = 0; i < _projectModel._tempos.Count; i++) {
                    if (time < _projectModel._tempos[i].StartTime)
                        break;
                }
                return i - 1;
            }

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

        public static class InitializeHelper
        {
            public static void SetTempoList(ProjectModel model, List<Tempo> tempos)
            {
                model._tempos = tempos;
            }
        }
    }
}