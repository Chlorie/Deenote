#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core;
using Deenote.Core.Project;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Localization;
using Deenote.UI.Dialogs;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views.Elements;
using Deenote.UIFramework.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class MenuNavigationPageView : MonoBehaviour
    {
        private const int MaxRecentFilesCount = 5;

        [SerializeField] Button _newButton = default!;
        [SerializeField] Button _openButton = default!;
        [SerializeField] Button _saveButton = default!;
        [SerializeField] Button _saveAsButton = default!;
        [SerializeField] Collapsable _recentFilesCollapsable = default!;

        [SerializeField] Button _preferenceButton = default!;

        [SerializeField] Button _aboutDevelopersButton = default!;
        [SerializeField] Button _turorialsButton = default!;
        [SerializeField] Button _updateHistoryButton = default!;

        [SerializeField] Button _checkUpdateButton = default!;

        [Header("Prefabs")]
        [SerializeField] RecentFileListItem _recentFilePrefab = default!;
        private PooledObjectListView<RecentFileListItem> _recentFiles = default!;

        internal IEnumerable<string>? GetRecentFiles()
            => _recentFiles?.Select(item => item.FilePath);

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _newProjectOnOpenMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_Content"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_N"));
        private static readonly MessageBoxArgs _newProjectOnUnsavedOpenMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectOnUnsavedOpen_MsgBox_Content"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_N"));

        private static readonly MessageBoxArgs _openProjOnOpenMsgBoxArgs = new(
            LocalizableText.Localized("OpenProject_MsgBox_Title"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Content"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_N"));
        private static readonly MessageBoxArgs _openProjOnUnsavedOpenMsgBoxArgs = new(
            LocalizableText.Localized("OpenProject_MsgBox_Title"),
            LocalizableText.Localized("OpenProjectOnUnsavedOpen_MsgBox_Content"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_N"));

        private static readonly MessageBoxArgs _loadProjFailedMsgBoxArgs = new(
            LocalizableText.Localized("OpenProject_MsgBox_Title"),
            LocalizableText.Localized("LoadProjectFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadProjectFailed_MsgBox_Y"),
            LocalizableText.Localized("LoadProjectFailed_MsgBox_N"));

        private static readonly MessageBoxArgs _saveProjDirExistsMsgBoxArgs = new(
            LocalizableText.Localized("SaveProject_MsgBox_Title"),
            LocalizableText.Localized("DirExists_MsgBox_Content"),
            LocalizableText.Localized("DirExists_MsgBox_Y"));

        private static readonly MessageBoxArgs _saveProjFileExistsMsgBoxArgs = new(
            LocalizableText.Localized("SaveProject_MsgBox_Title"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Content"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Y"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_N"));

        private static readonly MessageBoxArgs _verUpdMsgBoxArgs = new(
            LocalizableText.Localized("NewVersion_MsgBox_Title"),
            LocalizableText.Localized("NewVersion_MsgBox_Content"),
            LocalizableText.Localized("NewVersion_MsgBox_2"),
            LocalizableText.Localized("NewVersion_MsgBox_1"),
            LocalizableText.Localized("NewVersion_MsgBox_N"));

        #endregion

        #region LocalizedTextKeys

        private const string OpenProjectFileExplorerTitleKey = "OpenProject_FileExplorer_Title";
        private const string OpenProjectLoadingStatusKey = "OpenProject_Status_Loading";
        private const string OpenProjectLoadedStatusKey = "OpenProject_Status_Loaded";
        private const string OpenProjectFailedStatusKey = "OpenProject_Status_LoadFailed";
        private const string SaveAsFileExplorerTitleKey = "SaveAsProject_FileExplorer_Title";
        private const string SaveProjectSavingStatusKey = "SaveProject_Status_Saving";
        private const string SaveProjectSavedStatusKey = "SaveProject_Status_Saved";

        private const string VersionCheckNoInternetToastKey = "Version_NoInternet_Toast";
        private const string VersionCheckUpdateToDateToastKey = "Version_UpToDate_Toast";
        private const string VersionCheckingStatusKey = "NewVersion_Status_Checking";

        #endregion

        private void Awake()
        {
            _recentFiles = new(UnityUtils.CreateObjectPool(
                _recentFilePrefab, _recentFilesCollapsable.Content,
                item => item.OnInstantiate(this), defaultCapacity: 0, maxSize: MaxRecentFilesCount));
        }

        private void Start()
        {
            if (MainWindow._configtmpRecentFiles is { } inits) {
                foreach (var file in inits) {
                    _recentFiles.Add(out var item);
                    item.Initialize(file);
                }
            }
            // Register
            {
                _newButton.Clicked += UnityUtils.Action(MenuCreateNewProjectAsync);
                _openButton.Clicked += UnityUtils.Action(MenuOpenProjectAsync);
                _saveButton.Clicked += () =>
                {
                    _ = MenuSaveProjectAsync();
                    MainSystem.SaveSystem.SaveConfigurations();
                };
                _saveAsButton.Clicked += () =>
                {
                    _ = MenuSaveProjectAsAsync();
                    MainSystem.SaveSystem.SaveConfigurations();
                };
                _preferenceButton.Clicked += () => MainWindow.DialogManager.PreferencesDialog.Open();

                // Abouts

                _aboutDevelopersButton.Clicked += () => MainWindow.DialogManager.AboutDialog.Open(AboutDialog.Page.Developers);
                _turorialsButton.Clicked += () => MainWindow.DialogManager.AboutDialog.Open(AboutDialog.Page.Tutorials);
                _updateHistoryButton.Clicked += () => MainWindow.DialogManager.AboutDialog.Open(AboutDialog.Page.UpdateHistory);

                _checkUpdateButton.Clicked += UniTask.Action(async () =>
                {
                    MainWindow.StatusBar.SetLocalizedStatusMessage(VersionCheckingStatusKey);

                    try {
                        var res = await VersionManager.CheckUpdateAsync();
                        switch (res.Kind) {
                            case VersionManager.UpdateCheckResultKind.NoInternet:
                                _ = MainWindow.ToastManager.ShowLocalizedToastAsync(VersionCheckNoInternetToastKey, 3f);
                                break;
                            case VersionManager.UpdateCheckResultKind.UpToDate:
                                _ = MainWindow.ToastManager.ShowLocalizedToastAsync(VersionCheckUpdateToDateToastKey, 3f);
                                break;
                            case VersionManager.UpdateCheckResultKind.UpdateAvailable:
                                var clicked = await MainWindow.DialogManager.OpenMessageBoxAsync(_verUpdMsgBoxArgs, VersionManager.CurrentVersion.ToString(), res.LatestVersion.ToString());
                                switch (clicked) {
                                    case 0: VersionManager.OpenReleasePage(); break;
                                    case 1: VersionManager.OpenDownloadPage(res.LatestVersion); break;
                                }
                                break;
                        }
                    } finally {
                        MainWindow.StatusBar.SetReadyStatusMessage();
                    }
                });

                MainSystem.ProjectManager.RegisterNotificationAndInvoke(
                    ProjectManager.NotificationFlag.CurrentProject,
                    manager =>
                    {
                        bool active = manager.IsProjectLoaded();
                        _saveButton.IsInteractable = active;
                        _saveAsButton.IsInteractable = active;
                    });
                MainSystem.ProjectManager.RegisterNotificationAndInvoke(
                    ProjectManager.NotificationFlag.IsLoading,
                    manager =>
                    {
                        bool active = !manager.IsLoading;
                        _newButton.IsInteractable = active;
                        _openButton.IsInteractable = active;
                    });
            }
        }

        #region Notification Listener

        public async UniTask MenuCreateNewProjectAsync()
        {
            if (MainSystem.ProjectManager.IsProjectLoaded()) {
                var res = await MainWindow.DialogManager.OpenMessageBoxAsync(
                    MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange
                        ? _newProjectOnUnsavedOpenMsgBoxArgs
                        : _newProjectOnOpenMsgBoxArgs);
                if (res != 0)
                    return;
            }
            var nresult = await MainWindow.DialogManager.NewProjectDialog.OpenCreateNewAsync();
            if (nresult is { } result) {
                MainSystem.ProjectManager.SetCurrentProject(result.Project, result.AudioClip);
            }
        }

        public async UniTask MenuOpenProjectAsync()
        {
            if (MainSystem.ProjectManager.IsProjectLoaded()) {
                var res = await MainWindow.DialogManager.OpenMessageBoxAsync(
                    MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange
                        ? _openProjOnUnsavedOpenMsgBoxArgs
                        : _openProjOnOpenMsgBoxArgs);
                if (res != 0) return;
            }

        SelectFile:
            var feRes = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                LocalizableText.Localized(OpenProjectFileExplorerTitleKey),
                MainSystem.Args.SupportLoadProjectFileExtensions);
            if (feRes.IsCancelled)
                return;

            MainWindow.StatusBar.SetLocalizedStatusMessage(OpenProjectLoadingStatusKey);
            MainSystem.ProjectManager.UnloadCurrentProject();
            bool isLoaded = await MainSystem.ProjectManager.OpenLoadProjectFileAsync(feRes.Path);
            if (isLoaded) {
                AddOrTouchRecentFiles(feRes.Path);
                MainWindow.StatusBar.SetLocalizedStatusMessage(OpenProjectLoadedStatusKey);
            }
            else {
                MainWindow.StatusBar.SetReadyStatusMessage();
                var res = await MainWindow.DialogManager.OpenMessageBoxAsync(_loadProjFailedMsgBoxArgs);
                if (res == 0)
                    goto SelectFile;
                else
                    return;
            }
        }

        public async UniTask MenuSaveProjectAsync()
        {
            MainSystem.ProjectManager.AssertProjectLoaded("Unexpected interactable save button when current project is null");
            var proj = MainSystem.ProjectManager.CurrentProject;

            MainWindow.StatusBar.SetLocalizedStatusMessage(SaveProjectSavingStatusKey);
            await MainSystem.ProjectManager.SaveCurrentProjectAsync();
            MainWindow.StatusBar.SetLocalizedStatusMessage(SaveProjectSavedStatusKey);
            AddOrTouchRecentFiles(proj.ProjectFilePath);
        }

        public async UniTask MenuSaveProjectAsAsync()
        {
            MainSystem.ProjectManager.AssertProjectLoaded("Unexpected interactable save button when current project is not loaded");
        SelectFile:
            var feRes = await MainWindow.DialogManager.OpenFileExplorerInputFileAsync(
                LocalizableText.Localized(SaveAsFileExplorerTitleKey),
                MainSystem.ProjectManager.CurrentProject.MusicName,
                MainSystem.Args.DeenotePreferFileExtension);
            if (feRes.IsCancelled)
                return;

            if (Directory.Exists(feRes.Path)) {
                await MainWindow.DialogManager.OpenMessageBoxAsync(_saveProjDirExistsMsgBoxArgs);
                goto SelectFile;
            }
            if (File.Exists(feRes.Path)) {
                var res = await MainWindow.DialogManager.OpenMessageBoxAsync(_saveProjFileExistsMsgBoxArgs);
                if (res != 0) return;
            }

            MainWindow.StatusBar.SetLocalizedStatusMessage(SaveProjectSavingStatusKey);
            await MainSystem.ProjectManager.SaveCurrentProjectToAsync(feRes.Path);
            MainWindow.StatusBar.SetLocalizedStatusMessage(SaveProjectSavedStatusKey);
            AddOrTouchRecentFiles(MainSystem.ProjectManager.CurrentProject.ProjectFilePath);
        }

        #endregion

        #region Recent Files

        public void AddOrTouchRecentFiles(string filePath)
        {
            int findIndex = _recentFiles.FindIndex(filePath, static (item, fp) => item.FilePath == fp);
            if (findIndex >= 0) {
                _recentFiles.MoveTo(findIndex, 0);
                return;
            }

            if (_recentFiles.Count >= MaxRecentFilesCount) {
                _recentFiles.MoveTo(^1, 0);
                var item = _recentFiles[0];
                item.Initialize(filePath);
                item.transform.SetAsFirstSibling();
            }
            else {
                _recentFiles.Insert(0, out var item);
                item.Initialize(filePath);
                item.transform.SetAsFirstSibling();
            }
        }

        internal void RemoveRecentFile(RecentFileListItem item)
        {
            _recentFiles.Remove(item);
        }

        /// <summary>
        /// Move the item to the top of the list.
        /// </summary>
        /// <param name="item"></param>
        internal void TouchRecentFile(RecentFileListItem item)
        {
            var index = _recentFiles.IndexOf(item);
            Debug.Assert(index >= 0);
            _recentFiles.MoveTo(index, 0);
            item.transform.SetAsFirstSibling();
        }

        #endregion
    }
}