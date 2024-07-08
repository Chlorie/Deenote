using Deenote.Project;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Deenote
{
    public static class Fake
    {
        public static ProjectModel Project;

        static Fake()
        {
            using var fs = File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            //var bytes = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            //var fs = new MemoryStream(bytes);
            if (!AudioUtils.TryLoad(fs, ".mp3", out var clip)) {
                Debug.Log("Load audio failed");
            }

            Project = new ProjectModel {
                AudioClip = clip ?? Resources.Load<AudioClip>("Test/12.Magnolia"),
                MusicName = "tsuki-",
            };
            Project.Charts.Add(new ChartModel(ChartData.Load(Resources.Load<TextAsset>("Test/12.Magnolia.hard").text)) {
                Level = "10",
                Difficulty = Difficulty.Hard,
            });
            ProjectModel.InitializeHelper.SetTempoList(Project, new List<Tempo> { new(160f, 1.5f) });
        }
    }
}