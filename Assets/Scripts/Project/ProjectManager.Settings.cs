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
        private ProjectAutoSaveOption _autoSave;

        public ProjectAutoSaveOption AutoSave
        {
            get => _autoSave;
            set {
                if (_autoSave == value)
                    return;
                _autoSave = value;

                if (_autoSave is ProjectAutoSaveOption.Off)
                    _lastAutoSaveTime = 0f;
                _propertyChangeNotifier.Invoke(this, NotifyProperty.AutoSave);
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
                _propertyChangeNotifier.Invoke(this, NotifyProperty.SaveAudioDataInProject);
            }
        }

        private float _lastAutoSaveTime;
        private const string AutoSaveJsonDirName = $"$Deenote_AutoSave";

        private void Update_AutoSave()
        {
            switch (AutoSave) {
                case ProjectAutoSaveOption.On:
                    if (CurrentProject == null)
                        return;
                    if (_lastAutoSaveTime.IncAndTryWrap(Time.deltaTime, AutoSaveIntervalTime_s)) {
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saving"));
                        SaveCurrentProjectAsync();
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saved"));
                    }
                    break;
                case ProjectAutoSaveOption.OnAndSaveJson:
                    if (CurrentProject == null)
                        return;
                    if (_lastAutoSaveTime.IncAndTryWrap(Time.deltaTime, AutoSaveIntervalTime_s)) {
                        MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("AutoSaveProject_Status_Saving"));
                        SaveCurrentProjectAsync();

                        DateTime time = DateTime.Now; 
                        string dir = Path.Combine(Path.GetDirectoryName(CurrentProject.ProjectFilePath), AutoSaveJsonDirName);
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
    }
}