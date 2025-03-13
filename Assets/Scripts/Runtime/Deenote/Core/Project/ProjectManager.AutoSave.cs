#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library;
using System;

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

        public event Action<ProjectAutoSaveEventArgs>? ProjectSaving;
        public event Action<ProjectAutoSaveEventArgs>? ProjectSaved;

        private void AutoSaveHandler()
        {
            if (IsProjectLoaded()) {
                switch (AutoSave) {
                    case ProjectAutoSaveOption.On:
                        _ = SaveProjectAsync();
                        break;
                    case ProjectAutoSaveOption.OnAndSaveJson:
                        _ = SaveProjectAndJsonAsync();
                        break;
                    default:
                        break;
                }
            }

            async UniTaskVoid SaveProjectAsync()
            {
                _saveCts.Reset();
                ProjectSaving?.Invoke(new ProjectAutoSaveEventArgs(true));
                await SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
                ProjectSaving?.Invoke(new ProjectAutoSaveEventArgs(true));
            }

            async UniTaskVoid SaveProjectAndJsonAsync()
            {
                _saveCts.Reset();
                ProjectSaving?.Invoke(new ProjectAutoSaveEventArgs(true));
                var projSaveTask = SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
                var chartSaveTask = SaveCurrentProjectChartJsonsAsync(_saveCts.Token);

                await projSaveTask;
                await chartSaveTask;

                ProjectSaved?.Invoke(new ProjectAutoSaveEventArgs(true));
            }
        }

        public readonly record struct ProjectAutoSaveEventArgs(
            bool IsAutoSave);
    }
}