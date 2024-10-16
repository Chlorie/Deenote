using Cysharp.Threading.Tasks;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Localization;
using Deenote.Project;
using Deenote.Project.Models;
using Deenote.UI.Windows.Components;
using Deenote.Utilities;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class PropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        public Window Window => _window;

        [Header("Notify")]
        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;

        [Header("UI")]
        [Header("Project Info")]
        [SerializeField] Button _projectInfoGroupButton;
        [SerializeField] GameObject _projectInfoGroupGameObject;
        [SerializeField] Button _projectAudioButton;
        [SerializeField] LocalizedText _projectAudioText;
        [SerializeField] TMP_InputField _projectNameInputField;
        [SerializeField] TMP_InputField _projectComposerInputField;
        [SerializeField] TMP_InputField _projectChartDesignerInputField;
        [SerializeField] WindowDropdown _projectLoadedChartDropdown;

        [Header("Chart Info")]
        [SerializeField] Button _chartInfoGroupButton;
        [SerializeField] GameObject _chartInfoGroupGameObject;
        [SerializeField] TMP_InputField _chartNameInputField;
        [SerializeField] WindowDropdown _chartDifficultyDropdown;
        [SerializeField] TMP_InputField _chartLevelInputField;
        [SerializeField] TMP_InputField _chartSpeedInputField;
        [SerializeField] TMP_InputField _chartRemapVMinInputField;
        [SerializeField] TMP_InputField _chartRemapVMaxInputField;
        [SerializeField] TMP_Text _selectedNotesText;

        private void Awake()
        {
            _projectInfoGroupButton.onClick.AddListener(() =>
                _projectInfoGroupGameObject.SetActive(!_projectInfoGroupGameObject.activeSelf));
            _projectAudioButton.onClick.AddListener(OnAudioButtonClickedAsync);
            _projectNameInputField.onEndEdit.AddListener(OnProjectMusicNameChanged);
            _projectComposerInputField.onEndEdit.AddListener(OnProjectComposerChanged);
            _projectChartDesignerInputField.onEndEdit.AddListener(OnProjectChartDesignerChanged);
            _projectLoadedChartDropdown.Dropdown.onValueChanged.AddListener(OnProjectLoadedChartChanged);

            _chartInfoGroupButton.onClick.AddListener(() =>
                _chartInfoGroupGameObject.SetActive(!_chartInfoGroupGameObject.activeSelf));
            _chartNameInputField.onEndEdit.AddListener(OnChartNameChanged);
            _chartDifficultyDropdown.Dropdown.onValueChanged.AddListener(OnChartDifficultyChanged);
            _chartLevelInputField.onEndEdit.AddListener(OnChartLevelChanged);
            _chartSpeedInputField.onEndEdit.AddListener(OnChartSpeedChanged);
            _chartRemapVMinInputField.onEndEdit.AddListener(OnChartRemapVMinChanged);
            _chartRemapVMaxInputField.onEndEdit.AddListener(OnChartRemapVMaxChanged);

            _projectLoadedChartDropdown.ClearOptions();
            AwakeNoteInfo();
        }

        private void Start()
        {
            _chartDifficultyDropdown.ResetOptions(DifficultyExt.DropdownOptions.AsSpan());
            _chartDifficultyDropdown.Dropdown.SetValueWithoutNotify(_gameStageController.Chart.Difficulty.ToInt32());
        }

        #region UI Events

        private static readonly LocalizableText[] _loadAudioFailedMessageButtonTexts = new[] {
            LocalizableText.Localized("Message_AudioLoadFailed_Y"),
            LocalizableText.Localized("Message_AudioLoadFailed_N"),
        };
        [Obsolete]
        private async UniTaskVoid OnAudioButtonClickedAsync()
        {
            using DisposableGuard guard = new();
            while (true) {
                var res = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args
                    .SupportAudioFileExtensions);
                if (res.IsCancelled)
                    return;

                var fs = guard.Set(File.OpenRead(res.Path));
                var clip = await AudioUtils.LoadAsync(fs, Path.GetExtension(res.Path));
                if (clip is null) {
                    var btn = await MainSystem.MessageBox.ShowAsync(
                        LocalizableText.Localized("Message_AudioLoadFailed_Title"),
                        LocalizableText.Localized("Message_AudioLoadFailed_Content"),
                        _loadAudioFailedMessageButtonTexts);
                    if (btn != 0)
                        return;
                    // Reselect file
                    continue;
                }
                var bytes = new byte[fs.Length];
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bytes);
                _editorController.EditProjectAudio(res.Path, bytes, clip);
                break;
            }
        }

        private void OnProjectMusicNameChanged(string value)
        {
            _editorController.EditProjectMusicName(value);
        }

        private void OnProjectComposerChanged(string value)
        {
            _editorController.EditProjectComposer(value);
        }

        private void OnProjectChartDesignerChanged(string value)
        {
            _editorController.EditProjectChartDesigner(value);
        }

        private void OnProjectLoadedChartChanged(int value)
        {
            _gameStageController.LoadChart(_projectManager.CurrentProject, value);
        }

        private void OnChartNameChanged(string value)
        {
            _editorController.EditChartName(value);
        }

        private void OnChartDifficultyChanged(int value)
        {
            _editorController.EditChartDifficulty(DifficultyExt.FromInt32(value));
        }

        private void OnChartLevelChanged(string value)
        {
            _editorController.EditChartLevel(value);
        }

        private void OnChartSpeedChanged(string value)
        {
            if (float.TryParse(value, out var speed)) {
                _editorController.EditChartSpeed(speed);
            }
            else {
                NotifyChartSpeedChanged(_gameStageController.Chart.Data.Speed);
            }
        }

        private void OnChartRemapVMinChanged(string value)
        {
            if (int.TryParse(value, out var rvMin)) {
                _editorController.EditChartRemapVMin(rvMin);
            }
            else {
                NotifyChartRemapVMinChanged(_gameStageController.Chart.Data.RemapMinVelocity);
            }
        }

        private void OnChartRemapVMaxChanged(string value)
        {
            if (int.TryParse(value, out var rvMax)) {
                _editorController.EditChartRemapVMax(rvMax);
            }
            else {
                NotifyChartRemapVMaxChanged(_gameStageController.Chart.Data.RemapMaxVelocity);
            }
        }

        #endregion

        #region Notify

        [Obsolete]
        public void NotifyProjectChanged(ProjectModel project)
        {
            NotifyAudioFileChanged(project.SaveAsRefPath ? project.AudioFileRelativePath : null);
            NotifyProjectMusicNameChanged(project.MusicName);
            NotifyProjectComposerChanged(project.Composer);
            NotifyProjectChartDesignerChanged(project.ChartDesigner);
            _projectLoadedChartDropdown.ResetOptions(project.Charts.Select(c =>
                string.IsNullOrEmpty(c.Name) ? $"<{c.Difficulty.ToDisplayString()}>" : c.Name));
        }
        [Obsolete]
        public void NotifyAudioFileChanged(string? filePath)
        {
            if (filePath is not null)
                _projectAudioText.SetRawText(Path.GetFileName(filePath));
            else
                _projectAudioText.SetLocalizedText("Window_Properties_ProjectInfo_Audio_Embeded");
        }
        [Obsolete]
        public void NotifyProjectMusicNameChanged(string name) => _projectNameInputField.SetTextWithoutNotify(name);
        [Obsolete]
        public void NotifyProjectComposerChanged(string composer) =>
            _projectComposerInputField.SetTextWithoutNotify(composer);
        [Obsolete]
        public void NotifyProjectChartDesignerChanged(string charter) =>
            _projectChartDesignerInputField.SetTextWithoutNotify(charter);
        [Obsolete]
        public void NotifyChartChanged(ProjectModel project, int chartIndex)
        {
            _projectLoadedChartDropdown.Dropdown.SetValueWithoutNotify(chartIndex);

            var chart = project.Charts[chartIndex];
            NotifyChartNameChanged(chart.Name, chart.Difficulty);
            NotifyChartDifficultyChanged(chart.Difficulty);
            NotifyChartLevelChangd(chart.Level);
            NotifyChartSpeedChanged(chart.Data.Speed);
            NotifyChartRemapVMinChanged(chart.Data.RemapMinVelocity);
            NotifyChartRemapVMaxChanged(chart.Data.RemapMaxVelocity);
        }
        [Obsolete]
        public void NotifyChartNameChanged(string name, Difficulty fallbackDifficulty)
        {
            _chartNameInputField.SetTextWithoutNotify(name);
            _projectLoadedChartDropdown.SetOption(_projectLoadedChartDropdown.Dropdown.value,
                LocalizableText.Raw(string.IsNullOrEmpty(name) ? $"<{fallbackDifficulty.ToDisplayString()}>" : name));
        }
        [Obsolete]
        public void NotifyChartDifficultyChanged(Difficulty difficulty) =>
            _chartDifficultyDropdown.Dropdown.SetValueWithoutNotify(difficulty.ToInt32());
        [Obsolete]
        public void NotifyChartLevelChangd(string level) => _chartLevelInputField.SetTextWithoutNotify(level);
        [Obsolete]
        public void NotifyChartSpeedChanged(float speed) =>
            _chartSpeedInputField.SetTextWithoutNotify(speed.ToString("F1"));
        [Obsolete]
        public void NotifyChartRemapVMinChanged(int remapVMin) =>
            _chartRemapVMinInputField.SetTextWithoutNotify(remapVMin.ToString());
        [Obsolete]
        public void NotifyChartRemapVMaxChanged(int remapVMax) =>
            _chartRemapVMinInputField.SetTextWithoutNotify(remapVMax.ToString());

        #endregion
    }
}