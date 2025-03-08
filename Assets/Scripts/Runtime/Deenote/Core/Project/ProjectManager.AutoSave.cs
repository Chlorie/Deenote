#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities;
using Deenote.Library;
using System;
using System.IO;
using System.Threading;

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

        public event Action<ProjectAutoSaveEventArgs>? ProjectAutoSaving;
        public event Action<ProjectAutoSaveEventArgs>? ProjectAutoSaved;

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
                ProjectAutoSaving?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
                await SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
                ProjectAutoSaving?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
            }

            async UniTaskVoid SaveProjectAndJsonAsync()
            {
                _saveCts.Reset();
                ProjectAutoSaving?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
                var projSaveTask = SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
                var chartSaveTask = SaveCurrentProjectChartJsonsAsync(_saveCts.Token);

                await projSaveTask;
                await chartSaveTask;

                ProjectAutoSaved?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
            }
        }

        public readonly record struct ProjectAutoSaveEventArgs(
            ProjectAutoSaveOption Option);
    }
}