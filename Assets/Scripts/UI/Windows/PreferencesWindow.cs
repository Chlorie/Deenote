using Deenote.Project;
using Deenote.UI.Windows.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed class PreferencesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        public Window Window => _window;

        [Header("Notify")]

        [Header("UI")]
        [SerializeField] Toggle _stageEffectToggle;
        [SerializeField] Toggle _showFpsToggle;
        [SerializeField] Toggle _vSyncToggle;
        [SerializeField] TMP_InputField _mouseSensitivityInputField;
        [SerializeField] Button _mouseSensitivityReverseButton;
        [SerializeField] WindowDropdown _languageDropdown;
        [SerializeField] WindowDropdown _autoSaveDropDown;
        [SerializeField] Toggle _showIneffectivePropertiesToggle;
        [SerializeField] Toggle _distinguishPianoNoteToggle;
        [SerializeField] Toggle _saveAudioDataInProjectToggle;

        private void Awake()
        {
            _stageEffectToggle.onValueChanged.AddListener(val => MainSystem.GameStage.IsStageEffectOn = val);
            _showFpsToggle.onValueChanged.AddListener(val => MainSystem.StatusBar.IsFpsShown = val);
            _vSyncToggle.onValueChanged.AddListener(val => MainSystem.GlobalSettings.IsVSyncOn = val);
            _mouseSensitivityInputField.onEndEdit.AddListener(OnMouseWheelSensitivityChanged);
            _mouseSensitivityReverseButton.onClick.AddListener(() => MainSystem.Input.MouseScrollSensitivity = -MainSystem.Input.MouseScrollSensitivity);
            _languageDropdown.Dropdown.onValueChanged.AddListener(index => MainSystem.Localization.CurrentLanguage = _languageDropdown.Options[index].TextOrKey);
            _autoSaveDropDown.Dropdown.onValueChanged.AddListener(index => MainSystem.ProjectManager.AutoSave = ProjectManager.EnumExts.AudoSaveOptionFromDropdownIndex(index));
            _showIneffectivePropertiesToggle.onValueChanged.AddListener(val => MainSystem.PropertiesWindow.IsIneffectivePropertiesVisible = val);
            _distinguishPianoNoteToggle.onValueChanged.AddListener(val => MainSystem.GameStage.IsPianoNotesDistinguished = val);
            _saveAudioDataInProjectToggle.onValueChanged.AddListener(val => MainSystem.ProjectManager.IsAudioDataSaveInProject = val);

            _window.SetOnIsActivatedChanged(activated => { if (activated) OnWindowActivated(); });
        }

        private void Start()
        {
            _languageDropdown.ResetOptions(MainSystem.Localization.Languages);
            _languageDropdown.Dropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(opt => opt == MainSystem.Localization.CurrentLanguage));
            _autoSaveDropDown.ResetOptions(ProjectManager.EnumExts.AutoSaveDropDownOptions);
            _autoSaveDropDown.Dropdown.SetValueWithoutNotify(ProjectManager.EnumExts.ToDropdownIndex(MainSystem.ProjectManager.AutoSave));
        }

        #region Events

        private void OnMouseWheelSensitivityChanged(string value)
        {
            if (float.TryParse(value, out var sense)) {
                MainSystem.Input.MouseScrollSensitivity = sense;
            }
            else {
                NotifyMouseScrollSensitivityChanged(MainSystem.Input.MouseScrollSensitivity);
            }
        }

        #endregion

        private void OnWindowActivated()
        {
            NotifyIsStageEffectOnChanged(MainSystem.GameStage.IsStageEffectOn);
            NotifyIsFpsShownChanged(MainSystem.StatusBar.IsFpsShown);
            NotifyIsVSyncOnChanged(MainSystem.GlobalSettings.IsVSyncOn);
            NotifyMouseScrollSensitivityChanged(MainSystem.Input.MouseScrollSensitivity);
            NotifyLanguageChanged(MainSystem.Localization.CurrentLanguage);
            NotifyAutoSaveChanged(MainSystem.ProjectManager.AutoSave);
            NotifyIsIneffectivePropertiesVisible(MainSystem.PropertiesWindow.IsIneffectivePropertiesVisible);
            NotifyIsPianoNoteDistinguished(MainSystem.GameStage.IsPianoNotesDistinguished);
            NotifyIsAudioDataSaveInProjectChanged(MainSystem.ProjectManager.IsAudioDataSaveInProject);
        }

        #region Notify

        public void NotifyIsStageEffectOnChanged(bool value)
        {
            if (!_window.IsActivated)
                return;
            _stageEffectToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyIsFpsShownChanged(bool value)
        {
            if (!_window.IsActivated)
                return;
            _showFpsToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyIsVSyncOnChanged(bool value)
        {
            if (!_window.IsActivated)
                return;
            _vSyncToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyMouseScrollSensitivityChanged(float value)
        {
            if (!_window.IsActivated)
                return;
            _mouseSensitivityInputField.SetTextWithoutNotify(value.ToString("F1"));
        }

        public void NotifyLanguageChanged(string value)
        {
            if (!_window.IsActivated)
                return;
            // _languageDropDown is not localizable, so directly get TextOrKey
            if (value == _languageDropdown.Options[_languageDropdown.Dropdown.value].TextOrKey)
                return;
            _languageDropdown.Dropdown.SetValueWithoutNotify(_languageDropdown.FindIndex(text => text == value));
        }

        public void NotifyAutoSaveChanged(ProjectManager.AutoSaveOption option)
        {
            if (!_window.IsActivated)
                return;
            _autoSaveDropDown.Dropdown.SetValueWithoutNotify(ProjectManager.EnumExts.ToDropdownIndex(option));
        }

        public void NotifyIsIneffectivePropertiesVisible(bool value)
        {
            if (!_window.IsActivated)
                return;
            _showIneffectivePropertiesToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyIsPianoNoteDistinguished(bool value)
        {
            if (!_window.IsActivated)
                return;
            _distinguishPianoNoteToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyIsAudioDataSaveInProjectChanged(bool value)
        {
            if (!_window.IsActivated)
                return;
            _saveAudioDataInProjectToggle.SetIsOnWithoutNotify(value);
        }

        #endregion
    }
}