#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core.Project;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using Deenote.UI;
using System;
using System.IO;
using UnityEngine;

namespace Deenote
{
    public sealed class AutoSaveTrigger : MonoBehaviour
    {
        private const string AutoSaveJsonDirName = $"Deenote_AutoSave";

        private const string AutoSaveSavingStatusKey = "AutoSaveProject_Status_Saving";
        private const string AutoSaveSavedStatusKey = "AutoSaveProject_Status_Saved";

        private float _timer;

        public bool IsEnabled
        {
            get => enabled;
            set {
                enabled = value;
                if (value)
                    _ = AutoSaveProjectAsync();
            }
        }

        private void Start()
        {
            MainSystem.ProjectManager.RegisterNotificationAndInvoke(
                ProjectManager.NotificationFlag.AutoSave,
                manager =>
                {
                    IsEnabled = manager.AutoSave is not ProjectAutoSaveOption.Off;
                });
        }

        private void OnDisable()
        {
            _timer = 0f;
        }

        private void Update()
        {
            if (MathUtils.IncAndTryWrap(ref _timer, Time.unscaledDeltaTime, MainSystem.ProjectManager.AutoSaveIntervalTime)) {
                _ = AutoSaveProjectAsync();
            }
        }

        private async UniTask AutoSaveProjectAsync()
        {
            if (!MainSystem.ProjectManager.IsProjectLoaded())
                return;
            if (MainSystem.ProjectManager.IsSaving)
                return;
            if (!MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange)
                return;

            switch (MainSystem.ProjectManager.AutoSave) {
                case Core.Project.ProjectAutoSaveOption.On:
                    MainWindow.StatusBar.SetLocalizedStatusMessage(AutoSaveSavingStatusKey);
                    await MainSystem.ProjectManager.SaveCurrentProjectAsync();
                    MainWindow.StatusBar.SetLocalizedStatusMessage(AutoSaveSavedStatusKey, DateTime.Now.ToString("HH:mm"), duration: 10f);
                    return;
                case Core.Project.ProjectAutoSaveOption.OnAndSaveJson:
                    MainWindow.StatusBar.SetLocalizedStatusMessage(AutoSaveSavingStatusKey);
                    var proj = MainSystem.ProjectManager.SaveCurrentProjectAsync();
                    var dir = Path.Combine(Path.GetDirectoryName(MainSystem.ProjectManager.CurrentProject.ProjectFilePath), AutoSaveJsonDirName);
                    var charts = MainSystem.ProjectManager.SaveCurrentProjectChartJsonsToAsync(dir);
                    await proj;
                    await charts;
                    MainWindow.StatusBar.SetLocalizedStatusMessage(AutoSaveSavedStatusKey, DateTime.Now.ToString("HH:mm"), duration: 10f);
                    return;
                default:
                    break;
            }
        }
    }
}