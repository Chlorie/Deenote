using Cysharp.Threading.Tasks;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Localization;
using Deenote.Project;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.Windows.Components;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
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
            _projectInfoGroupButton.onClick.AddListener(() => _projectInfoGroupGameObject.SetActive(!_projectInfoGroupGameObject.activeSelf));
            _projectAudioButton.onClick.AddListener(OnAudioButtonClickedAsync);
            _projectNameInputField.onEndEdit.AddListener(OnProjectMusicNameChanged);
            _projectComposerInputField.onEndEdit.AddListener(OnProjectComposerChanged);
            _projectChartDesignerInputField.onEndEdit.AddListener(OnProjectChartDesignerChanged);
            _projectLoadedChartDropdown.Dropdown.onValueChanged.AddListener(OnProjectLoadedChartChanged);

            _chartInfoGroupButton.onClick.AddListener(() => _chartInfoGroupGameObject.SetActive(!_chartInfoGroupGameObject.activeSelf));
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
            _chartDifficultyDropdown.ResetOptions(DifficultyExt.DropdownOptions);
            _chartDifficultyDropdown.Dropdown.SetValueWithoutNotify(_gameStageController.Chart.Difficulty.ToInt32());
        }

        #region UI Events

        private static readonly LocalizableText[] _loadAudioFailedMessageButtonTexts = new[] {
            LocalizableText.Localized("Message_AudioLoadFailed_Y"),
            LocalizableText.Localized("Message_AudioLoadFailed_N"),
        };

        private async UniTaskVoid OnAudioButtonClickedAsync()
        {
            FileStream fs = null;
            try {
            SelectFile:
                var res = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportAudioFileExtensions);
                if (res.IsCancelled)
                    return;

                fs = File.OpenRead(res.Path);
                if (!AudioUtils.TryLoad(fs, Path.GetExtension(res.Path), out var clip)) {
                    var btn = await MainSystem.MessageBox.ShowAsync(
                        LocalizableText.Localized("Message_AudioLoadFailed_Title"),
                        LocalizableText.Localized("Message_AudioLoadFailed_Content"),
                        _loadAudioFailedMessageButtonTexts);
                    if (btn != 0)
                        return;
                    // Reselect file
                    goto SelectFile;
                }

                var bytes = new byte[fs.Length];
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(bytes);
                // TODO: 这里的bytes可以不用了 
                _editorController.EditProjectAudio(res.Path, bytes, clip);
            } finally {
                fs?.Dispose();
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

        public void NotifyProjectChanged(ProjectModel project)
        {
            NotifyAudioFileChanged(project.SaveAsRefPath ? project.AudioFileRelativePath : null);
            NotifyProjectMusicNameChanged(project.MusicName);
            NotifyProjectComposerChanged(project.Composer);
            NotifyProjectChartDesignerChanged(project.ChartDesigner);
            _projectLoadedChartDropdown.ResetOptions(project.Charts.Select(c => string.IsNullOrEmpty(c.Name) ? $"<{c.Difficulty.ToDisplayString()}>" : c.Name));
        }

        public void NotifyAudioFileChanged(MayBeNull<string> filePath)
        {
            if (filePath.HasValue)
                _projectAudioText.SetRawText(Path.GetFileName(filePath.Value));
            else
                _projectAudioText.SetLocalizedText("Window_Properties_ProjectInfo_Audio_Embeded");
        }

        public void NotifyProjectMusicNameChanged(string name) => _projectNameInputField.SetTextWithoutNotify(name);

        public void NotifyProjectComposerChanged(string composer) => _projectComposerInputField.SetTextWithoutNotify(composer);

        public void NotifyProjectChartDesignerChanged(string charter) => _projectChartDesignerInputField.SetTextWithoutNotify(charter);

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

        public void NotifyChartNameChanged(string name, Difficulty fallbackDifficulty)
        {
            _chartNameInputField.SetTextWithoutNotify(name);
            _projectLoadedChartDropdown.SetOption(_projectLoadedChartDropdown.Dropdown.value, LocalizableText.Raw(string.IsNullOrEmpty(name) ? $"<{fallbackDifficulty.ToDisplayString()}>" : name));
        }

        public void NotifyChartDifficultyChanged(Difficulty difficulty) => _chartDifficultyDropdown.Dropdown.SetValueWithoutNotify(difficulty.ToInt32());

        public void NotifyChartLevelChangd(string level) => _chartLevelInputField.SetTextWithoutNotify(level);

        public void NotifyChartSpeedChanged(float speed) => _chartSpeedInputField.SetTextWithoutNotify(speed.ToString("F1"));

        public void NotifyChartRemapVMinChanged(int remapVMin) => _chartRemapVMinInputField.SetTextWithoutNotify(remapVMin.ToString());

        public void NotifyChartRemapVMaxChanged(int remapVMax) => _chartRemapVMinInputField.SetTextWithoutNotify(remapVMax.ToString());

        #endregion
    }
}
