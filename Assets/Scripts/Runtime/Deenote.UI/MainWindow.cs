#nullable enable

using Deenote.Core;
using Deenote.Localization;
using Deenote.UI.Dialogs;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.UI
{
    public interface IMainWindow
    {
        DialogManager DialogManager { get; }
        StatusBar StatusBar { get; }
        ToastManager ToastManager { get; }
        ref readonly MainWindow.ViewsWrapper Views { get; }
    }

    public sealed partial class MainWindow : MonoBehaviour, IMainWindow
    {
        private static MainWindow _instance = default!;

        public static MainWindow Instance => _instance;

        #region Panels Access

        private UIPianoSoundPlayer _pianoSoundPlayer = default!;

        [SerializeField] DialogManager _dialogManager = default!;
        [SerializeField] StatusBar _statusBar = default!;
        [SerializeField] ToastManager _toastManager = default!;
        [SerializeField] ViewsWrapper _views;

        internal static UIPianoSoundPlayer PianoSoundPlayer => _instance._pianoSoundPlayer;

        public static DialogManager DialogManager => _instance._dialogManager;
        public static StatusBar StatusBar => _instance._statusBar;
        public static ToastManager ToastManager => _instance._toastManager;
        public static ref readonly ViewsWrapper Views => ref _instance._views;

        #region Interface

        DialogManager IMainWindow.DialogManager => _dialogManager;
        StatusBar IMainWindow.StatusBar => _statusBar;
        ToastManager IMainWindow.ToastManager => _toastManager;
        ref readonly ViewsWrapper IMainWindow.Views => ref _views;

        #endregion

        [Serializable]
        public struct ViewsWrapper
        {
            [SerializeField] PerspectiveViewPanelView _perspectiveViewPanelView;
            [SerializeField] MenuNavigationPageView _menuNavigationPageView;

            public readonly PerspectiveViewPanelView PerspectiveViewPanelView => _perspectiveViewPanelView;
            public readonly MenuNavigationPageView MenuNavigationPageView => _menuNavigationPageView;
        }

        #endregion

        #region Localization Keys

        private const string UnhandledExceptionToastKey = "UnhandledException_Toast";

        private const string SaveProjectSavingStatusKey = "SaveProject_Status_Saving";
        private const string SaveProjectSavedStatusKey = "SaveProject_Status_Saved";
        private const string AutoSaveSavingStatusKey = "AutoSaveProject_Status_Saving";
        private const string AutoSaveSavedStatusKey = "AutoSaveProject_Status_Saved";

        #endregion

        #region Configurations temp

        internal static List<string>? _configtmpRecentFiles;

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

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Add("ui/perspective_aspect_ratio", Views.PerspectiveViewPanelView.AspectRatio);
                configs.AddList("ui/recent_files", Views.MenuNavigationPageView.GetRecentFiles() ?? _configtmpRecentFiles);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                Views.PerspectiveViewPanelView.AspectRatio = configs.GetSingle("ui/perspective_aspect_ratio", 4f / 3f);
                _configtmpRecentFiles = configs.GetStringList("ui/recent_files");
            };

            UnhandledExceptionHandler.UnhandledExceptionOccurred += args =>
            {
                ToastManager.ShowLocalizedToastAsync(UnhandledExceptionToastKey, 3f);
            };
            MainSystem.ProjectManager.ProjectSaving += args =>
            {
                if (args.IsAutoSave)
                    StatusBar.SetLocalizedStatusMessage(AutoSaveSavingStatusKey);
                else
                    StatusBar.SetLocalizedStatusMessage(SaveProjectSavingStatusKey);
            };
            MainSystem.ProjectManager.ProjectSaved += args =>
            {
                if (args.IsAutoSave)
                    StatusBar.SetLocalizedStatusMessage(AutoSaveSavedStatusKey);
                else
                    StatusBar.SetLocalizedStatusMessage(SaveProjectSavedStatusKey);
            };
        }

        public static class Args
        {
            public static UIIcons UIIcons => Resources.Load<UIIcons>("UI/UIIcons");
        }
    }
}