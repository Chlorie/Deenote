#nullable enable

using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Deenote
{
    public static class Fake
    {
        private static ProjectModel? _project;

        private const string TestMusic = "finale";

        public static ProjectModel GetProject()
        {
            if (_project is not null) return _project;

            //await using var fs = File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            //var clip = await AudioUtils.LoadAsync(fs, ".mp3");
            var clip = Resources.Load<AudioClip>($"Test/{TestMusic}");
            if (clip is null) {
                Debug.LogError("Load audio failed");
            }

            _project = new ProjectModel { AudioClip = clip!, ProjectFilePath = "D:/" };
            var chartData = ChartData.Load(Resources.Load<TextAsset>($"Test/{TestMusic}.hard").text);
            chartData.Notes.First(n => n.IsVisible).Duration = 0.375f;
            _project.Charts.Add(
                new ChartModel(chartData) {
                    // Name = "<Cht> name",
                    Level = "10",
                    Difficulty = Difficulty.Hard,
                });
            ProjectModel.InitializationHelper.SetTempoList(_project, new List<Tempo> { new(160f, 1.5f) });
            return _project;
        }
    }
}