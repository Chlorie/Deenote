using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.Utilities;
using System;
using System.IO;
using UnityEngine;

namespace Deenote.Project
{
    partial class ProjectManager
    {
        private const float AutoSaveIntervalTime_s = 5f * 60f;

        [Header("Args")]
        [SerializeField]
        private AutoSaveOption _autoSave;
        public AutoSaveOption AutoSave
        {
            get => _autoSave;
            set {
                if (_autoSave == value)
                    return;
                _autoSave = value;
                MainSystem.PreferenceWindow.NotifyAutoSaveChanged(_autoSave);
            }
        }

        [SerializeField]
        private bool __isAudioDataSaveInProject;
        public bool IsAudioDataSaveInProject
        {
            get => __isAudioDataSaveInProject;
            set {
                if (__isAudioDataSaveInProject == value)
                    return;
                __isAudioDataSaveInProject = value;
                MainSystem.PreferenceWindow.NotifyIsAudioDataSaveInProjectChanged(value);
            }
        }

        private float _lastAutoSaveTime;

        private void UpdateAutoSave()
        {
            switch (AutoSave) {
                case AutoSaveOption.Off:
                    _lastAutoSaveTime = 0f;
                    return;
                case AutoSaveOption.On:
                    if (CurrentProject == null)
                        return;
                    if (_lastAutoSaveTime.IncAndTryWrap(Time.deltaTime, AutoSaveIntervalTime_s)) {
                        _ = SaveProjectAsync();
                    }
                    break;
                case AutoSaveOption.OnAndSaveJson:
                    if (CurrentProject == null)
                        return;
                    if (_lastAutoSaveTime.IncAndTryWrap(Time.deltaTime, AutoSaveIntervalTime_s)) {
                        _ = SaveProjectAsync();
                     
                        string timeStr = DateTime.Now.ToString("yyMMddHHmmss");
                        string dir = Path.Combine(CurrentProjectSaveDirectory, $"{_currentProjectFileName}_$autosave");
                        var filename = Path.GetFileNameWithoutExtension(_currentProjectFileName);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        foreach (var chart in CurrentProject.Charts) {
                            File.WriteAllText(
                                Path.Combine(dir, $"{filename}.{chart.Difficulty.ToLowerCaseString()}.{timeStr}.json"),
                                chart.Data.ToJsonString());
                            chart.Data.ToJsonString();
                        }
                    }
                    break;
            }
        }

        public enum AutoSaveOption
        {
            Off,
            On,
            OnAndSaveJson,
        }

        public static class EnumExts
        {
            public static LocalizableText[] AutoSaveDropDownOptions = new[] {
                LocalizableText.Localized("Window_Preferences_AutoSave_Off"),
                LocalizableText.Localized("Window_Preferences_AutoSave_On"),
                LocalizableText.Localized("Window_Preferences_AutoSave_OnAndSaveJson"),
            };

            public static AutoSaveOption AudoSaveOptionFromDropdownIndex(int index) => (AutoSaveOption)index;

            public static int ToDropdownIndex(AutoSaveOption option) => (int)option;
        }
    }
}