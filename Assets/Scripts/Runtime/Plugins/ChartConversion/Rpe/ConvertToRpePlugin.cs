#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Plugin;
using System.Collections.Generic;
using System.IO;
using Deenote.Entities;
using UnityEngine;

namespace Deenote.Runtime.Plugins.ChartConversion.Rpe
{
    public class ConvertToRpePlugin : IDeenotePlugin
    {
        private static readonly string _defaultCoverPath =
            Path.Combine(Application.streamingAssetsPath, "ChartConversionPluginGroup", "default_cover.png");

        public string GetName(string languageCode) => languageCode switch {
            "zh" => "转换为.pez文件",
            "en" or _ => "Convert To .pez File"
        };

        public string? GetDescription(string languageCode) => null;

        private readonly Dictionary<string, Dictionary<string, string>> _texts = new() {
            ["en"] = new() { ["exporting"] = "Exporting...", ["exported"] = "Project exported as .pez file" },
            ["zh"] = new() { ["exporting"] = "导出中...", ["exported"] = "已导出为.pez文件" }
        };

        public async UniTask ExecuteAsync(DeenotePluginContext context, DeenotePluginArgs args)
        {
            var texts = _texts[args.CurrentLanguage.LanguageCode];
            var res = await context.UI.DialogManager.PezConvertDialog.OpenAsync();
            if (res.IsCancelled || res.Project is null) return;

            RpeConverter rpeConverter = new() {
                Speed = res.Speed,
                SpeedCoefficient = res.SpeedCoefficient,
                SpeedExponent = res.SpeedExponent,
                WidthCoefficient = res.WidthCoefficient,
                WidthExponent = res.WidthExponent,
                WidthMultiplier = res.WidthMultiplier,
                NeedFlickClick = res.NeedFlickClick,
                HoldAlpha = res.HoldAlpha,
                EarlyDisplaySlowNotes = res.EarlyDisplaySlowNotes,
                HoldDragInterval = res.HoldDragInterval
            };

            context.UI.StatusBar.SetRawTextStatusMessage(texts["exporting"]);
            foreach (var deemoChart in res.Project.Charts) {
                Info info = new() {
                    Name = res.Project.MusicName,
                    Level = $"{deemoChart.Difficulty.ToDisplayString()} Lv {deemoChart.Level}",
                    Difficulty = float.TryParse(deemoChart.Level, out var level) ? level : 10.0f,
                    Charter = res.Project.ChartDesigner,
                    Composer = res.Project.Composer,
                    Illustrator = res.Illustrator ?? "UK"
                };

                var rpeChart = rpeConverter.Convert(deemoChart);
                info.FillMeta(rpeChart.META);
                var audioData = res.Project.AudioFileData;
                string coverPath = string.IsNullOrWhiteSpace(res.CoverPath) ? _defaultCoverPath : res.CoverPath!;
                PezPackage package = new(info, rpeChart, audioData, coverPath);

                var fileName = res.Project.Charts.Count == 1
                    ? res.OutputFileName
                    : $"{res.OutputFileName}_{deemoChart.Difficulty.ToDisplayString()} Lv {deemoChart.Level}";
                await package.SaveToFileAsync(Path.Combine(res.OutputDirectory, $"{fileName}.pez"));
            }

            context.UI.StatusBar.SetRawTextStatusMessage(texts["exported"]);
        }
    }
}