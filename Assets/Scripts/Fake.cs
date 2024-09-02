using Cysharp.Threading.Tasks;
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
        private static ProjectModel? _project;

        public static async UniTask<ProjectModel> GetProject()
        {
            if (_project is not null) return _project;

            await using var fs = File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            var clip = await AudioUtils.LoadAsync(fs, ".mp3");
            if (clip is null) {
                Debug.LogError("Load audio failed");
            }

            _project = new ProjectModel { AudioClip = clip! };
            _project.Charts.Add(
                new ChartModel(ChartData.Load(Resources.Load<TextAsset>("Test/12.Magnolia.hard").text)) {
                    // Name = "<Cht> name",
                    Level = "10", Difficulty = Difficulty.Hard,
                });
            ProjectModel.InitializeHelper.SetTempoList(_project, new List<Tempo> { new(160f, 1.5f) });
            return _project;
        }
    }
}