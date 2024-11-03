#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class RecentFileItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;
        private string _filePath = default!;

        public MenuPageView Parent { get; internal set; } = default!;

        public string FilePath => _filePath;

        private static readonly MessageBoxArgs _openProjOnOpenMsgBoxArgs = new(
            LocalizableText.Localized("OpenRecentProject_MsgBox_Title"),
            LocalizableText.Localized("OpenProjectOnOpen_MsgBox_Content"),
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

        private void Start()
        {
            _button.OnClick.AddListener(async UniTaskVoid () =>
            {
                if (MainSystem.ProjectManager.CurrentProject is not null) {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_openProjOnOpenMsgBoxArgs);
                    if (res != 0)
                        return;
                }

                // Load project
                if (File.Exists(_filePath)) {
                    bool res = await MainSystem.ProjectManager.OpenLoadProjectFileAsync(_filePath);
                    if (!res) {
                        MainSystem.MessageBoxDialog.OpenAsync(_loadProjFailedMsgBoxArgs)
                            .Forget();
                    }
                    Parent.TouchRecentFile(this);
                    return;
                }

                // File not found
                var click = await MainSystem.MessageBoxDialog.OpenAsync(_fileNotFoundMsgBoxArgs);
                switch (click) {
                    case 0: // Remove
                        Parent.RemoveRecentFile(this);
                        return;
                    case 1: { // Reselect
                        var res = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(
                            LocalizableText.Localized("OpenProject_FileExplorer_Title"),
                            MainSystem.Args.SupportLoadProjectFileExtensions,
                            Path.GetDirectoryName(FilePath));
                        if (res.IsCancelled)
                            return;
                        bool openRes = await MainSystem.ProjectManager.OpenLoadProjectFileAsync(res.Path);
                        if (!openRes) {
                            MainSystem.MessageBoxDialog.OpenAsync(_loadProjFailedMsgBoxArgs).Forget();
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

        public void Initialize(string filePath)
        {
            _filePath = filePath;
            _button.Text.SetRawText(Path.GetFileName(filePath));
        }
    }
}