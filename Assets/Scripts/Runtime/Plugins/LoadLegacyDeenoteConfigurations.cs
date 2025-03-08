#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core.Project;
using Deenote.Localization;
using Deenote.Plugin;
using UnityEngine;

namespace Deenote.Runtime.Plugins
{
    public sealed class LoadLegacyDeenoteConfigurations : IDeenotePlugin
    {
        public string GetName(string languageCode) => languageCode switch {
            "zh" => "载入旧版Deenote配置",
            "en" or _ => "Load Legacy Deenote Configs",
        };

        public string? GetDescription(string languageCode) => null;

        public UniTask ExecuteAsync(DeenotePluginContext context, in DeenotePluginArgs args)
        {
            var autoSave = (AutoSaveState)GetInt("Autosave", 0);
            var lightEffect = GetBool("Light Effect", false);
            var showFps = GetBool("Show FPS");
            var vsync = GetBool("VSync On", context.GlobalSettings.IsVSyncOn);
            var mouseSensi = GetInt("Mouse Wheel Sensitivity");
            var noteSpeed = GetInt("Note Speed");
            var musicSpeed = GetInt("Music Speed");
            var effectVolume = GetInt("Effect Volume");
            var musicVolume = GetInt("Music Volume");
            var pianoVolume = GetInt("Piano Volume");
            var showLinkLines = GetBool("Show Link Line");
            var xGridCount = GetInt("XGrid Count");
            var xGridOffset = GetFloat("XGrid Offset");
            var tGridCount = GetInt("TGrid Count");
            var language = (Language)GetInt("Language", 0);
            var snapToGrid = GetBool("Snap To Grid");
            var showIndicator = GetBool("Show Indicator");
            var showBorder = GetBool("Show Border");

            context.ProjectManager.AutoSave = autoSave switch {
                AutoSaveState.Off => ProjectAutoSaveOption.Off,
                AutoSaveState.On => ProjectAutoSaveOption.On,
                AutoSaveState.OnAndSaveJson => ProjectAutoSaveOption.OnAndSaveJson,
                _ => ProjectAutoSaveOption.Off,
            };
            context.GameManager.IsStageEffectOn = lightEffect;
            context.GlobalSettings.IsFpsShown = showFps;
            context.GlobalSettings.IsVSyncOn = vsync;
            context.GlobalSettings.GameViewScrollSensitivity = mouseSensi / 10f;
            context.GameManager.NoteFallSpeed = noteSpeed * 5;
            context.GameManager.MusicSpeed = musicSpeed;
            context.GameManager.HitSoundVolume = effectVolume / 100f;
            context.GameManager.MusicVolume = musicVolume / 100f;
            context.GameManager.PianoVolume = pianoVolume / 100f;
            context.GameManager.IsShowLinkLines = showLinkLines;
            // TODO: 强制转换为旧版？
            context.GameManager.Grids.PositionGridCount = xGridCount + 1;
            //context.GameManager.Grids.PositionGridOffset_Legacy
            context.GameManager.Grids.TimeGridSubBeatCount = tGridCount;
            LocalizationSystem.TrySetLanguage(language switch {
                Language.EN => "en",
                Language.ZH => "zh",
                _ => null,
            });
            context.Editor.Placer.SnapToPositionGrid = snapToGrid;
            context.Editor.Placer.SnapToTimeGrid = snapToGrid;
            context.Editor.Placer.IsIndicatorOn = showIndicator;
            //context.GameManager.Grids.IsPositionGridBorderVisible_Legacy

            return UniTask.CompletedTask;
        }

        private enum AutoSaveState { Off, On, OnAndSaveJson }
        private enum Language { EN = 0, ZH = 1 }

        private static int GetInt(string key, int defaultValue = default)
            => PlayerPrefs.GetInt(key);
        private static bool GetBool(string key, bool defaultValue = default)
            => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        private static float GetFloat(string key, float defaultValue = default)
            => PlayerPrefs.GetFloat(key, defaultValue);
    }
}
