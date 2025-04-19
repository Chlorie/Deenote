#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core.Project;
using Deenote.Localization;
using Deenote.Plugin;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using UnityEngine;

namespace Deenote.Runtime.Plugins
{
    public sealed class OldVersionCompatibility : IDeenotePluginGroup
    {
        private Dictionary<string, Dictionary<string, string>> _texts;

        public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; }

        public string? GetGroupName(string LanguageCode) => LanguageCode switch {
            "zh" => "旧版本兼容",
            "en" or _ => "Old Version Compatibility",
        };

        public OldVersionCompatibility()
        {
            _texts = new Dictionary<string, Dictionary<string, string>> {
                ["en"] = new Dictionary<string, string> {
                    ["nochartload"] = "No chart Loaded",
                    ["exportas"] = "Export As...",
                    ["exporting"] = "Exporting...",
                    ["exported"] = "Chart exported as Deemo Chart",
                },
                ["zh"] = new Dictionary<string, string> {
                    ["nochartload"] = "谱面未加载",
                    ["exportas"] = "导出为...",
                    ["exporting"] = "导出中...",
                    ["exported"] = "已导出为Deemo格式谱面",
                }
            };

            Plugins = ImmutableArray.Create(
                ImmutableArray.Create<IDeenotePlugin>(new LoadLegacyDeenoteConfigurations()),
                ImmutableArray.Create<IDeenotePlugin>(new DelegatePlugin("Export As Deemo I Chart", new[] { ("zh", "导出为Deemo1谱面") }, async (context, args) =>
                {
                    var texts = _texts[args.CurrentLanguage.LanguageCode];
                    if (!context.GameManager.IsChartLoaded()) {
                        context.UI.ToastManager.ShowRawTextToastAsync(texts["nochartload"], 2f).Forget();
                        return;
                    }
                    context.ProjectManager.AssertProjectLoaded();

                    var res = await context.UI.DialogManager.OpenFileExplorerInputFileAsync(
                        LocalizableText.Raw(texts["exportas"]),
                        context.ProjectManager.CurrentProject.MusicName,
                        ".json");
                    if (res.IsCancelled)
                        return;

                    context.UI.StatusBar.SetRawTextStatusMessage(texts["exporting"]);
                    await File.WriteAllTextAsync(res.Path,
                        context.GameManager.CurrentChart.ToJsonString(Entities.ChartSerializationVersion.DeemoV2));
                    context.UI.StatusBar.SetRawTextStatusMessage(texts["exported"]);
                })));
        }
    }
}