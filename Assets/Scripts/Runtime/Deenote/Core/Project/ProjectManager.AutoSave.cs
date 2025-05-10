#nullable enable

using Deenote.Library;
using System;
using UnityEngine.Profiling;

namespace Deenote.Core.Project
{
    partial class ProjectManager
    {
        private const string AutoSaveJsonDirName = "$Deenote_AutoSave";
        private const int DefaultAutoSaveIntervalTime = 5 * 60;

        private int _autoSaveIntervalTime_bf;
        /// <remarks>
        /// Unit second
        /// </remarks>
        public int AutoSaveIntervalTime
        {
            get => _autoSaveIntervalTime_bf;
            set {
                if (Utils.SetField(ref _autoSaveIntervalTime_bf, value)) {
                    NotifyFlag(NotificationFlag.AutoSaveInterval);
                }
            }
        }

        private ProjectAutoSaveOption _autoSave_bf;
        public ProjectAutoSaveOption AutoSave
        {
            get => _autoSave_bf;
            set {
                if (Utils.SetField(ref _autoSave_bf, value)) {
                    NotifyFlag(NotificationFlag.AutoSave);
                }
            }
        }

        public event Action<ProjectSaveEventArgs>? ProjectSaved;

        private void RegisterAutoSaveConfigurations()
        {
            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Set("project/autosave", (int)AutoSave);
                configs.Set("project/autosave_interval", AutoSaveIntervalTime);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                AutoSave = (ProjectAutoSaveOption)configs.GetInt32("project/autosave", (int)ProjectAutoSaveOption.Off);
                var autosaveintervaltime = configs.GetInt32("project/autosave_interval", -1);
                AutoSaveIntervalTime = autosaveintervaltime <= 0 ? DefaultAutoSaveIntervalTime : autosaveintervaltime;
            };
        }

        public readonly record struct ProjectSaveEventArgs(ProjectSaveContents Contents)
        {
            public bool IsProjectSaved => Contents.HasFlag(ProjectSaveContents.Project);
            public bool IsChartJsonsSaved => Contents.HasFlag(ProjectSaveContents.ChartJsons);
        }

        [Flags]
        public enum ProjectSaveContents
        {
            None,
            Project,
            ChartJsons,
        }
    }
}