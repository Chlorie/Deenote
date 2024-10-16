using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.Utilities;
using System;
using System.Collections.Immutable;
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
                _propertyChangedNotifier.Invoke(this, NotifyProperty.AutoSave);
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
                _propertyChangedNotifier.Invoke(this, NotifyProperty.SaveAudioDataInProject);
                MainSystem.PreferenceWindow.NotifyIsAudioDataSaveInProjectChanged(value);
            }
        }

        private float _lastAutoSaveTime;
        private const string AutoSaveDirName = $"$Deenote_AutoSave";

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
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saving"));
                        SaveProjectAsync().Forget();
                        SaveCurrentProjectAsync();
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saved"));
                    }
                    break;
                case AutoSaveOption.OnAndSaveJson:
                    if (CurrentProject == null)
                        return;
                    if (_lastAutoSaveTime.IncAndTryWrap(Time.deltaTime, AutoSaveIntervalTime_s)) {
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saving"));
                        SaveProjectAsync().Forget();
                        SaveCurrentProjectAsync();

                        DateTime time = DateTime.Now; 
                        string dir = Path.Combine(Path.GetDirectoryName(CurrentProject.ProjectFilePath), AutoSaveDirName);
                        string filename = Path.GetFileNameWithoutExtension(CurrentProject.ProjectFilePath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);

                        foreach (var chart in CurrentProject.Charts) {
                            File.WriteAllText(
                                Path.Combine(dir, $"{filename}.{chart.Name ?? chart.Difficulty.ToLowerCaseString()}.{time:yyMMddHHmmss}.json"),
                                chart.Data.ToJsonString());
                        }
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saved"));
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
            public static ImmutableArray<LocalizableText> AutoSaveDropDownOptions = ImmutableArray.Create(
                LocalizableText.Localized("Window_Preferences_AutoSave_Off"),
                LocalizableText.Localized("Window_Preferences_AutoSave_On"),
                LocalizableText.Localized("Window_Preferences_AutoSave_OnAndSaveJson"));

            public static AutoSaveOption AutoSaveOptionFromDropdownIndex(int index) => (AutoSaveOption)index;

            public static int ToDropdownIndex(AutoSaveOption option) => (int)option;
        }
    }
}