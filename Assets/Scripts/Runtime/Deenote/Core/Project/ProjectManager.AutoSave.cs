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

        private CancellationTokenSource? _autoSaveCts;

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
            switch (AutoSave) {
                case ProjectAutoSaveOption.On:
                    if (IsProjectLoaded()) {
                        _ = SaveProjectAsync();
                    }
                    break;
                case ProjectAutoSaveOption.OnAndSaveJson:
                    if (IsProjectLoaded()) {
                        _ = SaveProjectAndJsonAsync();
                    }
                    break;
                default:
                    break;
            }

            async UniTaskVoid SaveProjectAsync()
            {
                _autoSaveCts?.Cancel();
                _autoSaveCts?.Dispose();
                var cts = new CancellationTokenSource();
                _autoSaveCts = cts;
                try {
                    ProjectAutoSaving?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
                    await SaveCurrentProjectAsync(_autoSaveCts.Token);
                    ProjectAutoSaving?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
                    if (cts == _autoSaveCts)
                        _autoSaveCts = null;
                } finally {
                    cts.Dispose();
                }
            }

            async UniTaskVoid SaveProjectAndJsonAsync()
            {
                _autoSaveCts?.Cancel();
                _autoSaveCts?.Dispose();
                var cts = new CancellationTokenSource();
                _autoSaveCts = cts;
                try {
                    ProjectAutoSaving?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
                    var projSaveTask = SaveCurrentProjectAsync();

                    var time = DateTime.Now;
                    string dir = Path.Combine(Path.GetDirectoryName(CurrentProject.ProjectFilePath), AutoSaveJsonDirName);
                    string filename = Path.GetFileNameWithoutExtension(CurrentProject.ProjectFilePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    foreach (var chart in CurrentProject.Charts) {
                        var chartname = string.IsNullOrEmpty(chart.Name) ? chart.Difficulty.ToLowerCaseString() : chart.Name;
                        File.WriteAllText(
                            Path.Combine(dir, $"{filename}.{chartname}.{time:yyMMddHHmmss}.json"),
                            chart.ToJsonString());
                    }
                    await projSaveTask;
                    ProjectAutoSaved?.Invoke(new ProjectAutoSaveEventArgs(AutoSave));
                    if (cts == _autoSaveCts)
                        _autoSaveCts = null;
                } finally {
                    cts.Dispose();
                }
            }
        }

        public readonly record struct ProjectAutoSaveEventArgs(
            ProjectAutoSaveOption Option);
    }
}