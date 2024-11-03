#nullable enable

using Deenote.Project;
using Deenote.UI.ComponentModel;
using Deenote.UI.Controls;
using Deenote.Utilities;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed class PreferencesDialog : MonoBehaviour
    {
        [SerializeField] Dialog _dialog = default!;

        [Header("Properties")]
        [SerializeField] KVBooleanProperty _stageEffectProperty = default!;
        [SerializeField] KVBooleanProperty _showFpsProperty = default!;
        [SerializeField] KVBooleanProperty _vSyncProperty = default!;
        [SerializeField] KVInputProperty _mouseSensitivityProperty = default!;
        [SerializeField] Button _mouseSensitivityReverseButton = default!;
        [SerializeField] KVDropdownProperty _languageProperty = default!;
        [SerializeField] KVDropdownProperty _autoSaveProperty = default!;
        [SerializeField] KVBooleanProperty _ineffectiveProperty = default!;
        [SerializeField] KVBooleanProperty _distinguishPianoNotesProperty = default!;
        [SerializeField] KVBooleanProperty _saveAudioInProjectProperty = default!;

        private void Start()
        {
            _stageEffectProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GameStage.IsStageEffectOn = val ?? false);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.StageEffect,
                stage => _stageEffectProperty.CheckBox.SetValueWithoutNotify(stage.IsStageEffectOn));

            _showFpsProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GlobalSettings.IsFpsShown = val ?? false);
            MainSystem.GlobalSettings.RegisterPropertyChangeNotificationAndInvoke(
                MainSystem.Settings.NotifyProperty.FpsShown,
                settings => _showFpsProperty.CheckBox.SetValueWithoutNotify(settings.IsFpsShown));

            _vSyncProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GlobalSettings.IsVSyncOn = val ?? false);
            MainSystem.GlobalSettings.RegisterPropertyChangeNotificationAndInvoke(
                MainSystem.Settings.NotifyProperty.VSync,
                settings => _vSyncProperty.CheckBox.SetValueWithoutNotify(settings.IsVSyncOn));

            _mouseSensitivityProperty.InputField.OnValueChanged.AddListener(val =>
            {
                if (float.TryParse(val, out var fval))
                    MainSystem.Input.MouseScrollSensitivity = fval;
                else
                    _mouseSensitivityProperty.InputField.SetValueWithoutNotify(MainSystem.Input.MouseScrollSensitivity.ToString("F1"));
            });
            _mouseSensitivityReverseButton.OnClick.AddListener(
                () => MainSystem.Input.MouseScrollSensitivity = -MainSystem.Input.MouseScrollSensitivity);
            MainSystem.Input.RegisterPropertyChangeNotificationAndInvoke(
                Inputting.InputController.NotifyProperty.MouseScrollSensitivity,
                input => _mouseSensitivityProperty.InputField.SetValueWithoutNotify(MainSystem.Input.MouseScrollSensitivity.ToString("F1")));

            _languageProperty.Dropdown.ResetOptions(MainSystem.Localization.Languages);
            _languageProperty.Dropdown.SetValueWithoutNotify(
                _languageProperty.Dropdown.Options.Find(v => v == MainSystem.Localization.CurrentLanguage));
            _languageProperty.Dropdown.OnValueChanged.AddListener(
                val => MainSystem.Localization.CurrentLanguage = _languageProperty.Dropdown.Options[val].TextOrKey);
            MainSystem.Localization.OnLanguageChanged += val =>
            {
                _languageProperty.Dropdown.SetValueWithoutNotify(
                    _languageProperty.Dropdown.FindIndex(txt => txt == MainSystem.Localization.CurrentLanguage));
            };

            _autoSaveProperty.Dropdown.ResetOptions(ProjectAutoSaveOptionExt.DropDownOptions.AsSpan());
            _autoSaveProperty.Dropdown.SetValueWithoutNotify(MainSystem.ProjectManager.AutoSave.ToDropdownIndex());
            _autoSaveProperty.Dropdown.OnValueChanged.AddListener(
                val => MainSystem.ProjectManager.AutoSave = ProjectAutoSaveOptionExt.FromDropdownIndex(val));
            MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                ProjectManager.NotifyProperty.AutoSave,
                projm => _autoSaveProperty.Dropdown.SetValueWithoutNotify(projm.AutoSave.ToDropdownIndex()));

            _ineffectiveProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GlobalSettings.IsIneffectivePropertiesVisible = val ?? false);
            MainSystem.GlobalSettings.RegisterPropertyChangeNotificationAndInvoke(
                MainSystem.Settings.NotifyProperty.IneffectivePropertiesVisiblility,
                settings => _ineffectiveProperty.CheckBox.SetValueWithoutNotify(settings.IsIneffectivePropertiesVisible));

            _distinguishPianoNotesProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GameStage.IsPianoNotesDistinguished = val ?? false);
            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.DistinguishPianoNotes,
                stage => _distinguishPianoNotesProperty.CheckBox.SetValueWithoutNotify(stage.IsPianoNotesDistinguished));

            _saveAudioInProjectProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.ProjectManager.IsAudioDataSaveInProject = val ?? false);
            MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                ProjectManager.NotifyProperty.SaveAudioDataInProject,
                projm => _saveAudioInProjectProperty.CheckBox.SetValueWithoutNotify(projm.IsAudioDataSaveInProject));
        }

        public void Open()
        {
            _dialog.Open();
        }
    }
}