#nullable enable

using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Operations;
using UnityEngine;

namespace Deenote
{
    public static class Fake
    {
        private static ProjectModel? _project;
        private static AudioClip? _audio;

        private const string TestMusic = "finale";

        public static (ProjectModel, AudioClip) GetProject()
        {
            if (_project is not null) return (_project, _audio!);

            //await using var fs = File.OpenRead(Path.Combine(Application.streamingAssetsPath, "Magnolia.mp3"));
            //var clip = await AudioUtils.LoadAsync(fs, ".mp3");
            var clip = Resources.Load<AudioClip>($"Test/{TestMusic}");
            if (clip is null) {
                Debug.LogError("Load audio failed");
            }
            _audio = clip;

            _project = new ProjectModel("D:/", null!, "Fake.mp3");
            if (ChartModel.TryParse(Resources.Load<TextAsset>($"Test/{TestMusic}.hard").text, out var chart)) {
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
}