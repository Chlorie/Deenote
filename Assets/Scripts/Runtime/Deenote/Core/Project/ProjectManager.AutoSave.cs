#nullable enable

using Deenote.Library;
using System;
using UnityEngine.Profiling;

namespace Deenote.Core.Project
{
    partial class ProjectManager
    {
        private const string AutoSaveJsonDirName = "$Deenote_AutoSave";

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