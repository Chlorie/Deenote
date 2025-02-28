#nullable enable

using Deenote.UI.Dialogs;
using Deenote.UI.Theme;
using Deenote.UI.Views;
using Deenote.Library;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Deenote.Core;

namespace Deenote.UI
{
    public sealed partial class MainWindow : MonoBehaviour
    {
        private static MainWindow _instance = default!;

        #region Panels Access

        private UIPianoSoundPlayer _pianoSoundPlayer = default!;

        [SerializeField] MessageBox _messageBox = default!;
        [SerializeField] FileExplorerDialog _fileExplorerDialog = default!;
        [SerializeField] NewProjectDialog _newProjectDialog = default!;
        [SerializeField] PreferencesDialog _preferencesDialog = default!;
        [SerializeField] AboutDialog _aboutDialog = default!;
        [SerializeField] StatusBar _statusBar = default!;
        [SerializeField] ToastManager _toastManager = default!;
        [SerializeField] PerspectiveViewPanelView _perspectiveViewPanelView = default!;

        public static UIPianoSoundPlayer PianoSoundPlayer => _instance._pianoSoundPlayer;

        public static MessageBox MessageBox => _instance._messageBox;
        public static FileExplorerDialog FileExplorer => _instance._fileExplorerDialog;
        public static NewProjectDialog NewProjectDialog => _instance._newProjectDialog;
        public static PreferencesDialog PreferencesDialog => _instance._preferencesDialog;
        public static AboutDialog AboutDialog => _instance._aboutDialog;
        public static StatusBar StatusBar => _instance._statusBar;
        public static ToastManager ToastManager => _instance._toastManager;
        public static PerspectiveViewPanelView PerspectiveViewPanelView => _instance._perspectiveViewPanelView;

        #endregion

        [Header("Helpers")]
        [SerializeField] GameObject _raycastBlocker = default!;

        private readonly Stack<ModalDialog> _activeDialogs = new();

        #region Localization Keys

        private const string UnhandledExceptionToastKey = "UnhandledException_Toast";

        private const string AutoSaveSavingStatusKey = "AutoSaveProject_Status_Saving";
        private const string AutoSaveSavedStatusKey = "AutoSaveProject_Status_Saved";

        #endregion

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _quitUnsavedMsgBoxArgs = new(
            LocalizableText.Localized("Quit_MsgBox_Title"),
            LocalizableText.Localized("QuitUnsaved_MsgBox_Content"),
            LocalizableText.Localized("Quit_MsgBox_Y"),
            LocalizableText.Localized("Quit_MsgBox_N"));

        #endregion

        private void Awake()
        {
            MainWindow instance = this;
#if DEBUG
            if (_instance is null)
                _instance = instance;
            else {
                Destroy(instance);
                Debug.LogError($"Unexpected multiple instances of {typeof(MainWindow).Name}.");
            }
#else
            _instance = instance;
#endif
            _pianoSoundPlayer = new(MainSystem.PianoSoundSource);

            ApplicationManager.Quitting += args =>
            {
                if (!MainSystem.StageChartEditor.HasUnsavedChange) {
                    return;
                }
                var res = MessageBox.OpenAsync(_quitUnsavedMsgBoxArgs).GetAwaiter().GetResult();
                args.Cancel = res != 0;
            };
            UnhandledExceptionHandler.UnhandledExceptionOccurred += args =>
            {
                ToastManager.ShowLocalizedToastAsync(UnhandledExceptionToastKey, 3f);
            };
            MainSystem.ProjectManager.ProjectAutoSaving += args =>
            {
                StatusBar.SetLocalizedStatusMessage(AutoSaveSavingStatusKey);
            };
            MainSystem.ProjectManager.ProjectAutoSaved += args =>
            {
                StatusBar.SetLocalizedStatusMessage(AutoSaveSavedStatusKey);
            };
        }

        public static void RegisterModalDialog(ModalDialog dialog) => _instance.RegisterModalDialogImpl(dialog);
        private void RegisterModalDialogImpl(ModalDialog dialog)
        {
            dialog.IsActiveChanged += (dlg, active) =>
            {
                if (active) {
                    _activeDialogs.Push(dlg);
                    _raycastBlocker.SetActive(true);
                    _raycastBlocker.transform.SetAsLastSibling();
                    dlg.transform.SetAsLastSibling();
                }
                else {
                    var popped = _activeDialogs.Pop();
                    Debug.Assert(popped == dlg);
                    if (_activeDialogs.TryPeek(out var top)) {
                        top.transform.SetAsLastSibling();
                    }
                    else {
                        _raycastBlocker.SetActive(false);
                    }
                }
            };
        }

        public static class Args
        {
#if UNITY_EDITOR
            private static T GetScriptableObject<T>() where T : ScriptableObject => UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/ScriptableObjects/UI/{typeof(T).Name}.asset");
#endif

            public static UIIcons UIIcons
#if UNITY_EDITOR
                => GetScriptableObject<UIIcons>();
#else
                => Instance._uiIcons;
#endif

            public static UIPrefabs UIPrefabs
#if UNITY_EDITOR
            { get; } = GetScriptableObject<UIPrefabs>();
#else
                => Instance._uiPrefabs;
#endif
        }
    }
}