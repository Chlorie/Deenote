#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library;
using Deenote.Library.IO;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Deenote.UIFramework.Controls;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed partial class NewProjectDialog : ModalDialog
    {
        [SerializeField] Dialog _dialog = default!;
        [SerializeField] GameObject _contentRaycastBlocker = default!;

        [SerializeField] TextBox _projectNameInput = default!;
        [SerializeField] TextBlock _projectNameErrorText = default!;

        [SerializeField] TextBox _audioFileInput = default!;
        [SerializeField] Button _audioFileExploreButton = default!;
        [SerializeField] TextBlock _audioFileErrorText = default!;

        [SerializeField] GameObject _directoryInputGameObject = default!;
        [SerializeField] TextBox _directoryInput = default!;
        [SerializeField] Button _directoryExploreButton = default!;
        [SerializeField] TextBlock _directoryErrorText = default!;
        [SerializeField] CheckBox _sameDirectoryCheckBox = default!;

        [SerializeField] TextBlock _createResultText = default!;

        [SerializeField] Button _createButton = default!;
        [SerializeField] Button _cancelButton = default!;

        private (bool IsValid, string Text) _projectName;
        private (bool IsValid, string Text) _audioFilePath;
        private (bool IsValid, string Text) _directory;
        internal bool _saveToAudioDirectory;

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _audioNotExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectAudioNotExists_MsgBox_Content"),
            LocalizableText.Localized("NewProjectAudioNotExists_MsgBox_Y")) {
            HighlightIndex=-1
        };

        private static readonly MessageBoxArgs _audioLoadFailedMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectAudioLoadFailed_MsgBox_Content"),
            LocalizableText.Localized("NewProjectAudioLoadFailed_MsgBox_Y")) {
            HighlightIndex = -1
        };

        private static readonly MessageBoxArgs _dirExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("DirExists_MsgBox_Content"),
            LocalizableText.Localized("DirExists_MsgBox_Y")) {
            HighlightIndex = -1
        };

        private static readonly MessageBoxArgs _fileExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Content"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Y"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_N"));

        #endregion

        #region LocalizedTextKeys

        private const string ProjectPathCreateHintKey = "Dialog_NewProjectResultPath";

        private const string SelectAudioFileExplorerTitleKey = "NewProject_FileExplorer_SelectAudio_Title";
        private const string SelectDirectoryExplorerTitleKey = "NewProject_FileExplorer_SelectDirectory_Title";

        #endregion

        private ResetableCancellationTokenSource _cts = new();

        private string _projectResultPath = default!;

        protected override void Awake()
        {
            base.Awake();

            _dialog.CloseButton.Clicked += () =>
            {
                _cts.Cancel();
            };
            _cancelButton.Clicked += () =>
            {
                _cts.Cancel();
            };

            _projectNameInput.ValueChanged += val =>
            {
                if (string.IsNullOrEmpty(val) || !PathUtils.IsValidFileName(val)) {
                    _projectName = (false, val);
                    _projectNameErrorText.gameObject.SetActive(true);
                }
                else {
                    _projectName = (true, val);
                    _projectNameErrorText.gameObject.SetActive(false);
                }
                UpdateResult();

                //if (string.IsNullOrEmpty(val) || !Utils.IsValidFileName(val)) {
                //    _projectNameErrorText.gameObject.SetActive(true);
                //    UpdateResultText(false);
                //}
                //else {
                //    _projectNameErrorText.gameObject.SetActive(false);
                //    UpdateResultText(true);
                //}
            };
            _audioFileInput.ValueChanged += val =>
            {
                bool valid = IsValidPath(val);
                _audioFileErrorText.gameObject.SetActive(!valid);
                _audioFilePath = (valid, val);
                UpdateResult();
            };
            _audioFileExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                    LocalizableText.Localized(SelectAudioFileExplorerTitleKey),
                    MainSystem.Args.SupportLoadAudioFileExtensions);
                if (res.IsCancelled)
                    return;
                // If user hasnt input project name, we use the audio file name as the project name
                if (string.IsNullOrWhiteSpace(_projectName.Text)) {
                    var pname = Path.GetFileNameWithoutExtension(res.Path);
                    Debug.Assert(PathUtils.IsValidFileName(pname));
                    _projectName = (true, pname);
                    _projectNameInput.SetValueWithoutNotify(pname);
                }
                Debug.Assert(IsValidPath(res.Path));
                _audioFileInput.Value = res.Path;
            });
            _directoryInput.ValueChanged += val =>
            {
                var valid = IsValidPath(val);
                _directoryErrorText.gameObject.SetActive(!valid);
                _directory = (valid, val);
                UpdateResult();
            };
            _directoryExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.DialogManager.OpenFileExplorerSelectDirectoryAsync(
                    LocalizableText.Localized(SelectDirectoryExplorerTitleKey));
                if (res.IsCancelled)
                    return;
                _directoryInput.Value = res.Path;
            });
            _sameDirectoryCheckBox.IsCheckedChanged += val =>
            {
                bool same = val.GetValueOrDefault();
                _saveToAudioDirectory = same;
                if (same) {
                    _directoryInputGameObject.SetActive(false);
                }
                else {
                    _directoryInputGameObject.SetActive(true);
                }
                UpdateResult();
            };
        }

        private static bool IsValidPath(string input)
            => !string.IsNullOrWhiteSpace(input) && PathUtils.IsValidPath(input) && Path.IsPathFullyQualified(input);

        private void ResetDialog()
        {
            _projectName = default;
            _audioFilePath = default;
            //_directory = default; // Do not reset directory
            _saveToAudioDirectory = false;

            _projectNameInput.Value = "";
            _audioFileInput.Value = "";
            _directoryInput.Value = _directory.Text!;
            _sameDirectoryCheckBox.IsChecked = false;
            // Do not show error text when dialog is just opened
            _projectNameErrorText.gameObject.SetActive(false);
            _audioFileErrorText.gameObject.SetActive(false);
            _directoryErrorText.gameObject.SetActive(false);
            UpdateResult();
        }

        private void UpdateResult()
        {
            if (!_projectName.IsValid)
                goto Invalid;
            if (!_audioFilePath.IsValid)
                goto Invalid;

            string dir;
            if (_saveToAudioDirectory) {
                dir = Path.GetDirectoryName(_audioFilePath.Text);
            }
            else {
                if (!_directory.IsValid)
                    goto Invalid;
                dir = _directory.Text;
            }
            _projectResultPath = Path.Combine(dir, $"{_projectName.Text}.dnt");
            _createResultText.gameObject.SetActive(true);
            _createResultText.SetLocalizedText(ProjectPathCreateHintKey, _projectResultPath);
            _createButton.IsInteractable = true;

            return;
        Invalid:
            _createResultText.SetRawText("");
            _createButton.IsInteractable = false;
        }

        private void SetBlockInput(bool block)
        {
            _contentRaycastBlocker.gameObject.SetActive(block);
        }

        private void OnValidate()
        {
            _dialog ??= GetComponent<Dialog>();
        }
    }
}