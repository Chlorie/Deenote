#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Library;
using Deenote.UIFramework.Controls;
using Deenote.UI.Dialogs.Elements;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class RecentFileListItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;

        public MenuNavigationPageView Parent { get; private set; } = default!;

        public string FilePath { get; private set; } = default!;

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _openProjOnOpenMsgBoxArgs = new(
            LocalizableText.Localized("OpenRecentProject_MsgBox_Title"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Content"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_N"));
        private static readonly MessageBoxArgs _openProjOnUnsavedOpenMsgBoxArgs = new(
            LocalizableText.Localized("OpenRecentProject_MsgBox_Title"),
            LocalizableText.Localized("OpenProjectOnUnsavedOpen_MsgBox_Content"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Y"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_N"));

        private static readonly MessageBoxArgs _loadProjFailedMsgBoxArgs = new(
            LocalizableText.Localized("OpenRecentProject_MsgBox_Title"),
            LocalizableText.Localized("LoadProjectFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadProjectFailed_MsgBox_Y"),
            LocalizableText.Localized("LoadProjectFailed_MsgBox_N"));

        private static readonly MessageBoxArgs _fileNotFoundMsgBoxArgs = new(
            LocalizableText.Localized("OpenRecentProject_MsgBox_Title"),
            LocalizableText.Localized("OpenRecentProjectFileNotFound_MsgBox_Content"),
            LocalizableText.Localized("OpenRecentProjectFileNotFound_MsgBox_Y"),
            LocalizableText.Localized("OpenRecentProjectFileNotFound_MsgBox_X"),
            LocalizableText.Localized("OpenRecentProjectFileNotFound_MsgBox_N"));

        #endregion

        #region Localized Keys

        private const string OpenProjectLoadingStatusKey = "OpenProject_Status_Loading";
        private const string OpenProjectLoadedStatusKey = "OpenProject_Status_Loaded";
        private const string OpenProjectFailedStatusKey = "OpenProject_Status_LoadFailed";

        #endregion

        private void Awake()
        {
            _button.Clicked += UniTask.Action(async () =>
            {
                if (MainSystem.ProjectManager.IsProjectLoaded()) {
                    var res = await MainWindow.DialogManager.OpenMessageBoxAsync(
                        MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange
                            ? _openProjOnUnsavedOpenMsgBoxArgs
                            : _openProjOnOpenMsgBoxArgs);
                    if (res != 0)
                        return;
                }

                // Load project
                if (File.Exists(FilePath)) {
                    MainWindow.StatusBar.SetLocalizedStatusMessage(OpenProjectLoadingStatusKey);
                    MainSystem.ProjectManager.UnloadCurrentProject();
                    bool res = await MainSystem.ProjectManager.OpenLoadProjectFileAsync(FilePath);
                    if (res) {
                        MainWindow.StatusBar.SetLocalizedStatusMessage(OpenProjectLoadedStatusKey);
                    }
                    else {
                        MainWindow.StatusBar.SetLocalizedStatusMessage(OpenProjectFailedStatusKey);
                        MainWindow.DialogManager.OpenMessageBoxAsync(_loadProjFailedMsgBoxArgs)
                            .Forget();
                    }
                    Parent.TouchRecentFile(this);
                    return;
                }

                // File not found
                var click = await MainWindow.DialogManager.OpenMessageBoxAsync(_fileNotFoundMsgBoxArgs);
                switch (click) {
                    case 0: // Remove
                        Parent.RemoveRecentFile(this);
                        return;
                    case 1: { // Reselect
                        var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                            LocalizableText.Localized("OpenProject_FileExplorer_Title"),
                            MainSystem.Args.SupportLoadProjectFileExtensions,
                            Path.GetDirectoryName(FilePath));
                        if (res.IsCancelled)
                            return;
                        bool openRes = await MainSystem.ProjectManager.OpenLoadProjectFileAsync(res.Path);
                        if (!openRes) {
                            MainWindow.DialogManager.OpenMessageBoxAsync(_loadProjFailedMsgBoxArgs).Forget();
                            Parent.RemoveRecentFile(this);
                            return;
                        }
                        else {
                            Initialize(res.Path);
                            Parent.TouchRecentFile(this);
                            return;
                        }
                    }
                }
            });

        }

        internal void OnInstantiate(MenuNavigationPageView parent)
        {
            Parent = parent;
        }

        internal void Initialize(string filePath)
        {
            FilePath = filePath;
            _button.Text.SetRawText(Path.GetFileName(filePath));
        }
    }
}