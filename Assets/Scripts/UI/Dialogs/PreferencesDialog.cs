using Deenote.Project;
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
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.StageEffect,
                stage => _stageEffectProperty.CheckBox.SetValueWithoutNotify(stage.IsStageEffectOn));

            _showFpsProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.StatusBarView.IsFpsShown = val ?? false);
            MainSystem.StatusBarView.RegisterPropertyChangeNotification(
                Views.StatusBarView.NotifyProperty.FpsShown,
                bar => _showFpsProperty.CheckBox.SetValueWithoutNotify(bar.IsFpsShown));

            _vSyncProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GlobalSettings.IsVSyncOn = val ?? false);
            MainSystem.GlobalSettings.RegisterPropertyChangeNotification(
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
            // TODO: Mouse sensiti.. notify /其实不用吧，就这一个入口

            _languageProperty.Dropdown.ResetOptions(MainSystem.Localization.Languages);
            _languageProperty.Dropdown.SetValueWithoutNotify(
                _languageProperty.Dropdown.Options.Find(v => v == MainSystem.Localization.CurrentLanguage));
            _languageProperty.Dropdown.OnValueChanged.AddListener(
                val => MainSystem.Localization.CurrentLanguage = _languageProperty.Dropdown.Options[val].TextOrKey);
            // TODO: Notify language changed

            _autoSaveProperty.Dropdown.ResetOptions(ProjectManager.EnumExts.AutoSaveDropDownOptions.AsSpan());
            _autoSaveProperty.Dropdown.SetValueWithoutNotify(ProjectManager.EnumExts.ToDropdownIndex(MainSystem.ProjectManager.AutoSave));
            _autoSaveProperty.Dropdown.OnValueChanged.AddListener(
                val => MainSystem.ProjectManager.AutoSave = ProjectManager.EnumExts.AutoSaveOptionFromDropdownIndex(val));
            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                ProjectManager.NotifyProperty.AutoSave,
                projm => _autoSaveProperty.Dropdown.SetValueWithoutNotify(ProjectManager.EnumExts.ToDropdownIndex(projm.AutoSave)));

            _ineffectiveProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.NoteInfoPanelView.IsIneffectivePropertiesVisible = val ?? false);
            MainSystem.NoteInfoPanelView.RegisterPropertyChangeNotification(
                Views.NoteInfoPanelView.NotifyProperty.IneffectivePropertiesVisiblility,
                view => _ineffectiveProperty.CheckBox.SetValueWithoutNotify(view.IsIneffectivePropertiesVisible));

            _distinguishPianoNotesProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.GameStage.IsPianoNotesDistinguished = val ?? false);
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.DistinguishPianoNotes,
                stage => _distinguishPianoNotesProperty.CheckBox.SetValueWithoutNotify(stage.IsPianoNotesDistinguished));

            _saveAudioInProjectProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.ProjectManager.IsAudioDataSaveInProject = val ?? false);
            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                ProjectManager.NotifyProperty.SaveAudioDataInProject,
                projm => _saveAudioInProjectProperty.CheckBox.SetValueWithoutNotify(projm.IsAudioDataSaveInProject));
        }
    }
}