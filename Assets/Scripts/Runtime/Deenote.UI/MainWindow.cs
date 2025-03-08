#nullable enable

using Deenote.Core;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views;
using System;
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

        private const string AutoSaveSavingStatusKey = "AutoSaveProject_Status_Saving";
        private const string AutoSaveSavedStatusKey = "AutoSaveProject_Status_Saved";

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

        public static class Args
        {
            public static UIIcons UIIcons => Resources.Load<UIIcons>("UI/UIIcons");
        }
    }
}