#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using Deenote.Library;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote
{
#if UNITY_EDITOR

    public static class Fake
    {
        private static ProjectModel? _project;
        private static AudioClip? _audio;

        private const string TestMusic = "finale";

        private static string GetDevPath(string relativePathToAssets)
            => Path.Combine(Application.dataPath, "Dev", relativePathToAssets);

        public static async Task<(ProjectModel, AudioClip)> GetProject()
        {
            if (_project is not null) return (_project, _audio!);

            //await using var fs = File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            //var clip = await AudioUtils.LoadAsync(fs, ".mp3");
            using var fs = File.OpenRead(GetDevPath($"TestCharts/{TestMusic}.mp3"));
            var clip = await AudioUtils.TryLoadAsync(fs, ".mp3");
            if (clip is null) {
                Debug.LogError("Load audio failed");
            }
            _audio = clip;

            _project = new ProjectModel("D:/", null!, "Fake.mp3");
            //Resources.Load<TextAsset>($"Test/{TestMusic}.hard").text
            if (ChartModel.TryParse(File.ReadAllText(GetDevPath($"TestCharts/{TestMusic}.hard.json")), out var chart)) {
                // chart.Name = "<cht> name";
                chart.Level = "10";
                chart.Difficulty = Difficulty.Hard;
                _project.Charts.Add(chart);
                ((IUndoableOperation)(_project.InsertTempo(new TempoRange(160f, 1.5f, 11111f)))).Redo();
            }
            else {
                Debug.LogError("Load test chart failed");
            }
            return (_project, _audio!);
        }
    }

#endif
}