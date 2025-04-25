#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core;
using Deenote.Localization;
using Deenote.UI;
using Deenote.UI.Dialogs.Elements;
using System;
using UnityEngine;

namespace Deenote
{
    public sealed class StartupController : MonoBehaviour
    {
        private static readonly MessageBoxArgs _verUpdMsgBoxArgs = new(
            LocalizableText.Localized("NewVersion_MsgBox_Title"),
            LocalizableText.Localized("NewVersion_MsgBox_Content"),
            LocalizableText.Localized("NewVersion_MsgBox_2"),
            LocalizableText.Localized("NewVersion_MsgBox_1"),
            LocalizableText.Localized("NewVersion_MsgBox_N"));

        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("Ignored update check in editor mode");
#else
            if (MainSystem.GlobalSettings.CheckUpdateOnStartup) {
                _ = CheckUpdateAsync();
            }
#endif

#if UNITY_EDITOR
            Debug.Log("Ignored command line args in editor mode");
#else
            if (GetCommandLineArg0() is { } clfile) {
                _ = MainWindow.Views.MenuNavigationPageView.MenuOpenProjectAsync();
            }
#endif
        }

        private async UniTaskVoid CheckUpdateAsync()
        {
            var res = await VersionManager.CheckUpdateAsync();
            if (res.Kind is VersionManager.UpdateCheckResultKind.UpdateAvailable) {
                var clicked = await MainWindow.DialogManager.OpenMessageBoxAsync(_verUpdMsgBoxArgs,
                    VersionManager.CurrentVersion.ToString(),
                    res.LatestVersion.ToString());
                switch (clicked) {
                    case 0: VersionManager.OpenReleasePage(); break;
                    case 1: VersionManager.OpenDownloadPage(res.LatestVersion); break;
                }
            }
            // We do not pop up any fail message on startup-check
        }

        private void OpenProjectFromCommandLine()
        {
        }

        private string? GetCommandLineArg0()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1) {
                return args[1];
            }
            return null;
        }
    }
}