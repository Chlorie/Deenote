#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Core;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.Library.Components;
using Deenote.Localization;
using Deenote.UIFramework;
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
        [SerializeField] ToggleSwitch _distinguishPianoNotesToggle = default!;
        [SerializeField] ToggleSwitch _pauseStageWhenLoseFocusToggle = default!;

        [SerializeField] Dropdown _resolutionDropdown = default!;
        [SerializeField] ToggleSwitch _vSyncToggle = default!;
        [SerializeField] Dropdown _languageDropdown = default!;
        [SerializeField] Dropdown _autoSaveDropdown = default!;
        //[SerializeField] ToggleSwitch _embedAudioDataToggle = default!;

        [SerializeField] Dropdown _uiThemeDropdown = default!;
        [SerializeField] ToggleSwitch _showFpsToggle = default!;
        [SerializeField] ToggleSwitch _showIneffectivePropertiesToggle = default!;

        #region LocalizationKeys

        private const string UIThemeLocalizationKeyPrefix = "UITheme_";

        #endregion

        private void Start()
        {
            _dialog.CloseButton.Clicked += base.CloseSelfModalDialog;

            _stageEffectToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsStageEffectOn = val;
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.StageEffectOn,
                manager => _stageEffectToggle.SetIsCheckedWithoutNotify(manager.IsStageEffectOn));

            _pauseStageWhenLoseFocusToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.PauseWhenLoseFocus = val;
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.PauseWhenLoseFocus,
                manager => _pauseStageWhenLoseFocusToggle.SetIsCheckedWithoutNotify(manager.PauseWhenLoseFocus));

            _mouseSensitivityInput.EditSubmitted += val =>
            {
                if (float.TryParse(val, out var fval))
                    MainSystem.GlobalSettings.GameViewScrollSensitivity = fval;
                _mouseSensitivityInput.SetValueWithoutNotify(fval.ToString("F1"));
            };
            _mouseSensitivityInvertButton.Clicked += () => MainSystem.GlobalSettings.GameViewScrollSensitivity = -MainSystem.GlobalSettings.GameViewScrollSensitivity;
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.GameViewScrollSensitivity,
                settings => _mouseSensitivityInput.SetValueWithoutNotify(settings.GameViewScrollSensitivity.ToString("F1")));

            // System

            _resolutionDropdown.ResetOptions(_resolutionDropdownOptions.AsSpan());
            _resolutionDropdown.SelectedIndexChanged += index => ApplicationManager.SetResolution(GetResolutionDropdownOption(index));
            ApplicationManager.ResolutionChanged += vector => _resolutionDropdown.SetValueWithoutNotify(GetResolutionDropdownIndex(vector));
            _resolutionDropdown.SetValueWithoutNotify(GetResolutionDropdownIndex(ApplicationManager.GetResolution()));

            _vSyncToggle.IsCheckedChanged += val => MainSystem.GlobalSettings.IsVSyncOn = val;
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.VSync,
                settings => _vSyncToggle.IsChecked = settings.IsVSyncOn);

            _languageDropdown.ResetOptions(LocalizationSystem.Languages, pack => LocalizableText.Raw(pack.LanguageDisplayName));
            _languageDropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(text => text == LocalizationSystem.CurrentLanguage.LanguageDisplayName));
            _languageDropdown.SelectedIndexChanged += val => LocalizationSystem.CurrentLanguage = (LanguagePack)_languageDropdown.Options[val].Item!;
            LocalizationSystem.LanguageChanged += val => _languageDropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(text => text == val.LanguageDisplayName));
            _languageDropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(text => text == LocalizationSystem.CurrentLanguage.LanguageDisplayName));

            _autoSaveDropdown.ResetOptions(_autoSaveDropdownOptions.AsSpan());
            _autoSaveDropdown.SelectedIndexChanged += val => MainSystem.ProjectManager.AutoSave = GetAutoSaveDropdownOption(val);
            MainSystem.ProjectManager.RegisterNotificationAndInvoke(
                ProjectManager.NotificationFlag.AutoSave,
                manager => _autoSaveDropdown.SetValueWithoutNotify(GetAutoSaveDropdownIndex(manager.AutoSave)));

            //_embedAudioDataToggle.IsCheckedChanged+=MainSystem.ProjectManager.

            // Display

            _uiThemeDropdown.ResetOptions(UISystem.Themes, theme => LocalizableText.Localized($"{UIThemeLocalizationKeyPrefix}{theme.ThemeName}"));
            _uiThemeDropdown.SetValueWithoutNotify(_uiThemeDropdown.FindItemIndex(UISystem.CurrentTheme));
            _uiThemeDropdown.SelectedIndexChanged += val => UISystem.CurrentTheme = (UIThemeArgs)_uiThemeDropdown.Options[val].Item!;
            UISystem.ThemeChanged += val => _uiThemeDropdown.SetValueWithoutNotify(_uiThemeDropdown.FindItemIndex(val));
            _uiThemeDropdown.SetValueWithoutNotify(_uiThemeDropdown.FindItemIndex(UISystem.CurrentTheme));

            _showFpsToggle.IsCheckedChanged += val => MainSystem.GlobalSettings.IsFpsShown = val;
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.FpsShown,
                settings => _showFpsToggle.IsChecked = settings.IsFpsShown);

            _showIneffectivePropertiesToggle.IsCheckedChanged += val => MainSystem.GlobalSettings.IsIneffectivePropertiesVisible = val;
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.IneffectivePropertiesVisible,
                settings => _showIneffectivePropertiesToggle.IsChecked = settings.IsIneffectivePropertiesVisible);

            _distinguishPianoNotesToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsPianoNotesDistinguished = val;
            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.DistinguishPianoNotes,
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

        #region Resolutions

        private static readonly ImmutableArray<string> _resolutionDropdownOptions = ImmutableArray.Create(
            "960x540", "1280x720", "1600x900", "1920x1080", "2560x1440");

        private static Vector2Int GetResolutionDropdownOption(int optionIndex)
            => optionIndex switch {
                0 => new(960, 540),
                1 => new(1280, 720),
                2 => new(1600, 900),
                3 => new(1920, 1080),
                4 => new(2560, 1440),
                _ => ThrowHelper.ThrowInvalidOperationException<Vector2Int>(),
            };

        private static int GetResolutionDropdownIndex(Vector2Int option)
        {
            return (option.x, option.y) switch {
                (960, 540) => 0,
                (1280, 720) => 1,
                (1600, 900) => 2,
                (1920, 1080) => 3,
                (2560, 1440) => 4,
                _ => -1,
            };
        }

        #endregion
    }
}