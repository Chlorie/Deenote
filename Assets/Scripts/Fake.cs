using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote
{
    public static class Fake
    {
        public static ProjectModel Project;

        static Fake()
        {
            Project = new ProjectModel {
                AudioClip = Resources.Load<AudioClip>("Test/12.Magnolia"),
                MusicName = "tsuki-",
                Tempos = new List<Tempo>() {
                    new(160f, 1.5f),
                }
            };
            Project.Charts.Add(new ChartModel(ChartData.Load(Resources.Load<TextAsset>("Test/12.Magnolia.hard").text)) {
                Level = "10",
                Difficulty = Difficulty.Hard,
            });
        }
    }
}