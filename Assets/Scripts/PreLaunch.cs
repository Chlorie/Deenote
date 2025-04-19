#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core;
using Deenote.Localization;
using Deenote.Plugin;
using Deenote.Runtime.Plugins;
using Deenote.UI;
using Deenote.UI.Dialogs.Elements;
using System.ComponentModel;
using UnityEngine;

namespace Deenote
{
    public static class PreLaunch
    {
        #region MessageBoxArgs

        private static readonly MessageBoxArgs _quitUnsavedMsgBoxArgs = new(
            LocalizableText.Localized("Quit_MsgBox_Title"),
            LocalizableText.Localized("QuitUnsaved_MsgBox_Content"),
            LocalizableText.Localized("Quit_MsgBox_Y"),
            LocalizableText.Localized("Quit_MsgBox_N"));

        #endregion

        [RuntimeInitializeOnLoadMethod]
        private static void RegisterBuiltinPlugins()
        {
            DeenotePluginManager.RegisterPluginGroup(new OldVersionCompatibility());
            DeenotePluginManager.RegisterPluginGroup(new CommandShortcutButtons());
        }

        [RuntimeInitializeOnLoadMethod]
        private static void QuitRegisteration()
        {
            ApplicationManager.Quitting += QuitRegistration;

            static void QuitRegistration(CancelEventArgs args)
            {
                if (!MainSystem.StageChartEditor.OperationMemento.HasUnsavedChange) {
                    return;
                }
                var res = MainWindow.DialogManager.OpenMessageBoxAsync(_quitUnsavedMsgBoxArgs)
                    .ContinueWith(val =>
                    {
                        if (val == 0) {
                            ApplicationManager.Quitting -= QuitRegistration;
                            ApplicationManager.Quit();
                        }
                    });
                args.Cancel = true;
            }
        }
    }
}