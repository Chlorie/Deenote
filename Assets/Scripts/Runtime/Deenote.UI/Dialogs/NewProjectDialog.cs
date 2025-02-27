#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.UIFramework.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Library;
using System.IO;
using System.Threading;
using UnityEngine;
using Deenote.Localization;

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

        private bool IsSaveToAudioDirectory => _sameDirectoryCheckBox.IsChecked.GetValueOrDefault();

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _audioNotExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectAudioNotExists_MsgBox_Content"),
            LocalizableText.Localized("NewProjectAudioNotExists_MsgBox_Y"), -1);

        private static readonly MessageBoxArgs _audioLoadFailedMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectAudioLoadFailed_MsgBox_Content"),
            LocalizableText.Localized("NewProjectAudioLoadFailed_MsgBox_Y"), -1);

        private static readonly MessageBoxArgs _dirExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("DirExists_MsgBox_Content"),
            LocalizableText.Localized("DirExists_MsgBox_Y"), -1);

        private static readonly MessageBoxArgs _fileExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Content"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Y"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_N"));

        #endregion

        #region LocalizedTextKeys

        private const string ProjectPathCreateHintKey = "Dialog_NewProjectResultPath";

        private const string SelectAudioFileExplorerTitleKey = "Dialog_FileExplorer_SelectAudio_Title";
        private const string SelectDirectoryExplorerTitleKey = "Dialog_FileExplorer_SelectDirectory_Title";

        #endregion

        private CancellationTokenSource? _sharedCts;

        private string _projectResultPath = default!;

        protected override void Awake()
        {
            base.Awake();

            _dialog.CloseButton.Clicked += () =>
            {
                CloseSelfModalDialog();
                _sharedCts?.Cancel();
            };

            _projectNameInput.ValueChanged += val =>
            {
                if (string.IsNullOrEmpty(val) || !Utils.IsValidFileName(val)) {
                    _projectNameErrorText.gameObject.SetActive(true);
                    UpdateResultText(false);
                }
                else {
                    _projectNameErrorText.gameObject.SetActive(false);
                    UpdateResultText(true);
                }
            };
            _audioFileInput.ValueChanged += val =>
            {
                bool valid = IsValidPath(val);
                _audioFileErrorText.gameObject.SetActive(!valid);
                if (IsSaveToAudioDirectory) {
                    UpdateResultText(valid);
                }
            };
            _audioFileExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.FileExplorer.OpenSelectFileAsync(
                    LocalizableText.Localized(SelectAudioFileExplorerTitleKey),
                    MainSystem.Args.SupportLoadAudioFileExtensions);
                if (res.IsCancelled)
                    return;
                if (string.IsNullOrWhiteSpace(_projectNameInput.Value))
                    _projectNameInput.Value = Path.GetFileNameWithoutExtension(res.Path);
                _audioFileInput.Value = res.Path;
            });
            _directoryInput.ValueChanged += OnDirectoryValueChanged;
            _directoryExploreButton.Clicked += UniTask.Action(async UniTaskVoid () =>
            {
                var res = await MainWindow.FileExplorer.OpenSelectDirectoryAsync(
                    LocalizableText.Localized(SelectDirectoryExplorerTitleKey));
                if (res.IsCancelled)
                    return;
                _directoryInput.Value = res.Path;
            });
            _sameDirectoryCheckBox.IsCheckedChanged += (val =>
            {
                bool same = val.GetValueOrDefault();
                if (same) {
                    _directoryInputGameObject.SetActive(false);
                }
                else {
                    _directoryInputGameObject.SetActive(true);
                    OnDirectoryValueChanged(_directoryInput.Value);
                }
                UpdateResultText(null);
            });

            _cancelButton.Clicked += CloseSelfModalDialog;
        }

        private static bool IsValidPath(string input)
            => !string.IsNullOrWhiteSpace(input) && Utils.IsValidPath(input) && Path.IsPathFullyQualified(input);

        private void ResetDialog()
        {
            _projectNameInput.Value = "";
            _audioFileInput.Value = "";
            OnDirectoryValueChanged(_directoryInput.Value);
            _sameDirectoryCheckBox.IsChecked = false;
        }

        private void OnDirectoryValueChanged(string val)
        {
            var valid = IsValidPath(val);
            _directoryErrorText.gameObject.SetActive(!valid);
            UpdateResultText(valid);
        }

        private void UpdateResultText(bool? isValid)
        {
            if (isValid is false)
                goto SetInvalidPath;

            string dir;
            if (IsSaveToAudioDirectory) {
                var audio = _audioFileInput.Value;
                if (isValid is null && !IsValidPath(audio))
                    goto SetInvalidPath;
                dir = Path.GetDirectoryName(audio);
            }
            else {
                dir = _directoryInput.Value;
                if (isValid is null && !IsValidPath(dir))
                    goto SetInvalidPath;
            }
            _projectResultPath = Path.Combine(dir, $"{_projectNameInput.Value}.dnt");
            _createResultText.gameObject.SetActive(true);
            _createResultText.SetLocalizedText(ProjectPathCreateHintKey, _projectResultPath);
            _createButton.IsInteractable = true;

        SetInvalidPath:
            _createResultText.gameObject.SetActive(false);
            _createButton.IsInteractable = false;
        }

        private void OnValidate()
        {
            _dialog ??= GetComponent<Dialog>();
        }
    }
}