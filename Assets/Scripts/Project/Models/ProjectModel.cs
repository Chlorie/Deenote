using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Project.Models
{
    //[JsonObject(MemberSerialization.OptIn)]
    public sealed class ProjectModel
    {
        public string MusicName;
        public string Composer;
        public string ChartDesigner;

        public bool SaveAsRefPath;
        public byte[] AudioData;
        public string RefPath;

        public List<ChartModel> Charts { get;} = new();
        // string AudioType

        // TODO:这几个init只是给Fake开洞，应该提供Set方法设置这些东西
        public List<Tempo> Tempos { get; init; } = new();

        // NonSerialize
        public AudioClip AudioClip { get; set; }
    }
}