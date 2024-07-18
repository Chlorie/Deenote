using Cysharp.Threading.Tasks;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Localization;
using Deenote.Project;
using Deenote.Project.Models;
using Deenote.Utilities;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class PropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;

        [Header("UI")]
        [Header("Project Info")]
        [SerializeField] Button _projectInfoGroupButton;
        [SerializeField] GameObject _projectInfoGroupGameObject;
        [SerializeField] Button _projectAudioButton;
        [SerializeField] TMP_Text _projectAudioText;
        [SerializeField] TMP_InputField _projectNameInputField;
        [SerializeField] TMP_InputField _projectComposerInputField;
        [SerializeField] TMP_InputField _projectChartDesignerInputField;
        [SerializeField] TMP_Dropdown _projectLoadedChartDropdown;

        [Header("Chart Info")]
        [SerializeField] Button _chartInfoGroupButton;
        [SerializeField] GameObject _chartInfoGroupGameObject;
        [SerializeField] TMP_InputField _chartNameInputField;
        [SerializeField] TMP_Dropdown _chartDifficultyDropdown;
        [SerializeField] TMP_InputField _chartLevelInputField;
        [SerializeField] TMP_InputField _chartSpeedInputField;
        [SerializeField] TMP_InputField _chartRemapVMinInputField;
        [SerializeField] TMP_InputField _chartRemapVMaxInputField;
        [SerializeField] TMP_Text _selectedNotesText;


        private void Awake()
        {
            _projectInfoGroupButton.onClick.AddListener(() => _projectInfoGroupGameObject.SetActive(!_projectInfoGroupGameObject.activeSelf));
            _projectAudioButton.onClick.AddListener(OnAudioButtonClickedAsync);
            _projectNameInputField.onSubmit.AddListener(OnProjectMusicNameChanged);
            _projectComposerInputField.onSubmit.AddListener(OnProjectComposerChanged);
            _projectChartDesignerInputField.onSubmit.AddListener(OnProjectChartDesignerChanged);

            _chartInfoGroupButton.onClick.AddListener(() => _chartInfoGroupGameObject.SetActive(!_chartInfoGroupGameObject.activeSelf));
            _chartNameInputField.onSubmit.AddListener(OnChartNameChanged);
            _chartDifficultyDropdown.onValueChanged.AddListener(OnChartDifficultyChanged);
            _chartLevelInputField.onSubmit.AddListener(null);
            _chartSpeedInputField.onSubmit.AddListener(null);
            _chartRemapVMinInputField.onSubmit.AddListener(null);
            _chartRemapVMaxInputField.onSubmit.AddListener(null);
            // TODO: complete

            AwakeNoteInfo();
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

        private void OnChartNameChanged(string value)
        {
            _editorController.EditChartName(value);
        }

        private void OnChartDifficultyChanged(int value)
        {
            _editorController.EditChartDifficulty(DifficultyExt.FromInt32(value));
        }

        #endregion

        #region Notify

        public void NotifyAudioFileChanged(string filePath) => _projectAudioText.text = Path.GetFileName(filePath);

        public void NotifyProjectMusicNameChanged(string name) => _projectNameInputField.SetTextWithoutNotify(name);

        public void NotifyProjectComposerChanged(string composer) => _projectComposerInputField.SetTextWithoutNotify(composer);

        public void NotifyProjectChartDesignerChanged(string charter) => _projectChartDesignerInputField.SetTextWithoutNotify(charter);

        public void NotifyChartNameChanged(string name) => _chartNameInputField.SetTextWithoutNotify(name);

        public void NotifyChartDifficultyChanged(Difficulty difficulty) => _chartDifficultyDropdown.SetValueWithoutNotify(difficulty.ToInt32());

        #endregion
    }
}
