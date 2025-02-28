#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.CoreApp.Project;
using Deenote.Library.Components;
using Deenote.Localization;
using Deenote.UIFramework.Controls;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed class PreferencesDialog : ModalDialog
    {
        [SerializeField] Dialog _dialog = default!;

        [Header("Properties")]
        [SerializeField] ToggleSwitch _stageEffectToggle = default!;
        [SerializeField] TextBox _mouseSensitivityInput = default!;
        [SerializeField] Button _mouseSensitivityInvertButton = default!;
        [SerializeField] ToggleSwitch _vSyncToggle = default!;
        [SerializeField] Dropdown _languageDropdown = default!;
        [SerializeField] Dropdown _autoSaveDropdown = default!;
        [SerializeField] ToggleSwitch _embedAudioDataToggle = default!;
        [SerializeField] ToggleSwitch _showFpsToggle = default!;
        [SerializeField] ToggleSwitch _showIneffectivePropertiesToggle = default!;
        [SerializeField] ToggleSwitch _distinguishPianoNotesToggle = default!;

        protected override void Awake()
        {
            base.Awake();

            _dialog.CloseButton.Clicked += base.CloseSelfModalDialog;

            _stageEffectToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsStageEffectOn = val;
            MainSystem.GamePlayManager.RegisterNotification(
                GamePlay.GamePlayManager.NotificationFlag.StageEffectOn,
                manager => _stageEffectToggle.SetIsCheckedWithoutNotify(manager.IsStageEffectOn));

            _mouseSensitivityInput.EditSubmitted += val =>
            {
                if (float.TryParse(val, out var fval))
                    MainWindow.Settings.GameViewScrollSensitivity = fval;
                _mouseSensitivityInput.SetValueWithoutNotify(fval.ToString("F1"));
            };
            _mouseSensitivityInvertButton.Clicked += () => MainWindow.Settings.GameViewScrollSensitivity = -MainWindow.Settings.GameViewScrollSensitivity;
            MainWindow.Settings.RegisterNotification(
                MainWindow.SettingsData.NotificationFlag.GameViewScrollSensitivity,
                settings => _mouseSensitivityInput.SetValueWithoutNotify(settings.GameViewScrollSensitivity.ToString("F1")));

            // System

            _vSyncToggle.IsCheckedChanged += val => MainSystem.GlobalSettings.IsVSyncOn = val;
            MainSystem.GlobalSettings.RegisterNotification(
                GlobalSettings.NotificationFlag.VSync,
                settings => _vSyncToggle.IsChecked = settings.IsVSyncOn);

            _languageDropdown.ResetOptions(LocalizationSystem.Languages, pack => LocalizableText.Raw(pack.LanguageDisplayName));
            _languageDropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(text => text == LocalizationSystem.CurrentLanguage.LanguageDisplayName));
            _languageDropdown.SelectedIndexChanged += val => LocalizationSystem.CurrentLanguage = (LanguagePack)_languageDropdown.Options[val].Item!;
            LocalizationSystem.LanguageChanged += val => _languageDropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(text => text == val.LanguageDisplayName));

            _autoSaveDropdown.ResetOptions(_autoSaveDropdownOptions.AsSpan());
            _autoSaveDropdown.SelectedIndexChanged += val => MainSystem.ProjectManager.AutoSave = GetAutoSaveDropdownOption(val);
            MainSystem.ProjectManager.RegisterNotificationAndInvoke(
                ProjectManager.NotificationFlag.AutoSave,
                manager => _autoSaveDropdown.SetValueWithoutNotify(GetAutoSaveDropdownIndex(manager.AutoSave)));

            //_embedAudioDataToggle.IsCheckedChanged+=MainSystem.ProjectManager.

            // Display

            _showFpsToggle.IsCheckedChanged += val => MainSystem.GlobalSettings.IsFpsShown = val;
            MainSystem.GlobalSettings.RegisterNotification(
                GlobalSettings.NotificationFlag.FpsShown,
                settings => _showFpsToggle.IsChecked = settings.IsFpsShown);

            _showIneffectivePropertiesToggle.IsCheckedChanged += val => MainSystem.GlobalSettings.IsIneffectivePropertiesVisible = val;
            MainSystem.GlobalSettings.RegisterNotification(
                GlobalSettings.NotificationFlag.IneffectivePropertiesVisible,
                settings => _showIneffectivePropertiesToggle.IsChecked = settings.IsIneffectivePropertiesVisible);

            _distinguishPianoNotesToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsPianoNotesDistinguished = val;
            MainSystem.GamePlayManager.RegisterNotification(
                GamePlay.GamePlayManager.NotificationFlag.DistinguishPianoNotes,
                manager => _distinguishPianoNotesToggle.IsChecked = manager.IsPianoNotesDistinguished);
        }

        public void Open() => OpenSelfModalDialog();

        private void OnValidate()
        {
            _dialog ??= GetComponent<Dialog>();
        }

        #region AutoSaveOptions

        private static readonly ImmutableArray<LocalizableText> _autoSaveDropdownOptions = ImmutableArray.Create(
            LocalizableText.Localized("Dialog_PreferencesAutoSaveOff_Option"),
            LocalizableText.Localized("Dialog_PreferencesAutoSaveOn_Option"),
            LocalizableText.Localized("Dialog_PreferencesAutoSaveOnAndSaveJson_Option"));

        private static ProjectAutoSaveOption GetAutoSaveDropdownOption(int optionIndex)
            => optionIndex switch {
                0 => ProjectAutoSaveOption.Off,
                1 => ProjectAutoSaveOption.On,
                2 => ProjectAutoSaveOption.OnAndSaveJson,
                _ => ThrowHelper.ThrowInvalidOperationException<ProjectAutoSaveOption>(),
            };

        private static int GetAutoSaveDropdownIndex(ProjectAutoSaveOption option)
            => option switch {
                ProjectAutoSaveOption.Off => 0,
                ProjectAutoSaveOption.On => 1,
                ProjectAutoSaveOption.OnAndSaveJson => 2,
                _ => ThrowHelper.ThrowInvalidOperationException<int>(),
            };

        #endregion
    }
}