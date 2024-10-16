using Cysharp.Threading.Tasks;
using Deenote.ApplicationManaging;
using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class MenuPageView : MonoBehaviour
    {
        private const int MaxRecentFilesCount = 5;

        [SerializeField] Button _newButton;
        [SerializeField] Button _openButton;
        [SerializeField] Button _saveButton;
        [SerializeField] Button _saveAsButton;
        [SerializeField] Transform _recentFilesParentTransform;

        [SerializeField] Button _preferenceButton;

        [SerializeField] Button _aboutButton;
        [SerializeField] Button _updatesButton;
        [SerializeField] Button _turorialsButton;

        [SerializeField] Button _checkUpdateButton;

        [Header("Prefabs")]
        [SerializeField] RecentFileItem _recentFileItemPrefab;
        private PooledObjectListView<RecentFileItem> _recentFiles;

        private static readonly MessageBoxArgs _newProjectOnOpenMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_Content"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("NewProjectOnOpen_MsgBox_N"));

        private static readonly MessageBoxArgs _openProjOnOpenMsgBoxArgs = new(
            LocalizableText.Localized("OpenProject_MsgBox_Title"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Content"),
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

        private void Awake()
        {
            _recentFiles = new PooledObjectListView<RecentFileItem>(
                UnityUtils.CreateObjectPool(() =>
                {
                    var item = Instantiate(_recentFileItemPrefab, _recentFilesParentTransform);
                    item.Parent = this;
                    return item;
                }, maxSize: MaxRecentFilesCount));
        }

        private void Start()
        {
            _newButton.OnClick.AddListener(async () =>
            {
                if (MainSystem.ProjectManager.CurrentProject is not null) {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_newProjectOnOpenMsgBoxArgs);
                    if (res != 0)
                        return;
                }

                var result = await MainSystem.NewProjectDialog.OpenCreateNewAsync();
                MainSystem.ProjectManager.LoadProject(result);
                MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("NewProject_Status_Created"));
            });
            _openButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                if (MainSystem.ProjectManager.CurrentProject is not null) {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_openProjOnOpenMsgBoxArgs);
                    if (res != 0)
                        return;
                }

            SelectFile:
                var feRes = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(
                    MainSystem.Args.SupportLoadProjectFileExtensions);
                if (feRes.IsCancelled)
                    return;

                bool isLoaded = await MainSystem.ProjectManager.OpenLoadProjectFileAsync(feRes.Path);
                if (isLoaded) {
                    AddToRecentFile(feRes.Path);
                    MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("OpenProject_Status_Loaded"));
                }
                else {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_loadProjFailedMsgBoxArgs);
                    if (res == 0)
                        goto SelectFile;
                    else
                        return;
                }
            });
            _saveButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                await MainSystem.ProjectManager.SaveCurrentProjectAsync();
                AddToRecentFile(MainSystem.ProjectManager.CurrentProject.ProjectFilePath);
                MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("SaveProject_Status_Saved"));
            });
            _saveAsButton.OnClick.AddListener(async UniTaskVoid () =>
            {
            SelectFile:
                var feRes = await MainSystem.FileExplorerDialog.OpenInputFileAsync(
                    MainSystem.Args.DeenotePreferFileExtension);
                if (feRes.IsCancelled)
                    return;

                if (Directory.Exists(feRes.Path)) {
                    await MainSystem.MessageBoxDialog.OpenAsync(_saveProjDirExistsMsgBoxArgs);
                    goto SelectFile;
                }
                if (File.Exists(feRes.Path)) {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_saveProjFileExistsMsgBoxArgs);
                    if (res != 0)
                        return;
                }

                MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("SaveProject_Status_Saving"));

                await MainSystem.ProjectManager.SaveCurrentProjectToAsync(feRes.Path);
                AddToRecentFile(feRes.Path);
                MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("SaveProject_Status_Saved"));
            });

            _preferenceButton.OnClick.AddListener(MainSystem.PreferencesDialog.Open);

            _aboutButton.OnClick.AddListener(MainSystem.AboutDialog.Open);
            //_updatesButton.OnClick.AddListener(() => MainSystem.AboutWindow.OpenWindow(Windows.AboutWindow.AboutPage.UpdateHistory));
            //_turorialsButton.OnClick.AddListener(() => MainSystem.AboutWindow.OpenWindow(Windows.AboutWindow.AboutPage.Tutorials));

            _checkUpdateButton.OnClick.AddListener(() => VersionChecker.CheckUpdateAsync(true, true).Forget());

            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                Project.ProjectManager.NotifyProperty.CurrentProject,
                projm =>
                {
                    bool active = projm.CurrentProject is not null;
                    _saveButton.IsInteractable = active;
                    _saveAsButton.IsInteractable = active;
                });
        }

        internal void AddToRecentFile(string filePath)
        {
            int findIndex = _recentFiles.Find(filePath, static (item, fp) => item.FilePath == fp);
            if (findIndex < 0) {
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
            else {
                _recentFiles.MoveTo(findIndex, 0);
            }
        }

        internal void RemoveRecentFile(RecentFileItem item)
        {
            _recentFiles.Remove(item);
        }

        internal void TouchRecentFile(RecentFileItem item)
        {
            var index = _recentFiles.IndexOf(item);
            Debug.Assert(index >= 0);
            _recentFiles.MoveTo(index, 0);
            item.transform.SetAsFirstSibling();
        }
    }
}