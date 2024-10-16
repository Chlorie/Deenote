using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed partial class NewProjectDialog : MonoBehaviour
    {
        [SerializeField] Dialog _dialog = default!;

        [SerializeField] InputField _projectNameInput;
        [SerializeField] LocalizedText _projectNameErrorText;

        [SerializeField] InputField _audioFileInput;
        [SerializeField] Button _audioFileExploreButton;
        [SerializeField] LocalizedText _audioFileErrorText;

        [SerializeField] InputField _directoryInput;
        [SerializeField] Button _directoryExploreButton;
        [SerializeField] LocalizedText _directoryErrorText;
        [SerializeField] CheckBox _sameDirectoryCheckBox;

        [SerializeField] LocalizedText _createHintText;
        [SerializeField] Button _createButton;
        [SerializeField] Button _cancelButton;

        // When null, use dir of _audioFile
        private bool _saveToAudioDirectory;

        private string __savePath;
        private string SavePath
        {
            get => __savePath;
            set {
                if (value == __savePath)
                    return;
                __savePath = value;
                _createHintText.SetLocalizedText("NewProjectDialog_Directory_CreateHint", value);
            }
        }

        private ProjectModel? _resultProject;

        private static readonly MessageBoxArgs _audioNotExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectAudioNotExists_MsgBox_Content"),
            LocalizableText.Localized("NewProjectAudioNotExists_MsgBox_Y"));

        private static readonly MessageBoxArgs _audioLoadFailedMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("NewProjectAudioLoadFailed_MsgBox_Content"),
            LocalizableText.Localized("NewProjectAudioLoadFailed_MsgBox_Y"));

        private static readonly MessageBoxArgs _dirExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("DirExists_MsgBox_Content"),
            LocalizableText.Localized("DirExists_MsgBox_Y"));

        private static readonly MessageBoxArgs _fileExistsMsgBoxArgs = new(
            LocalizableText.Localized("NewProject_MsgBox_Title"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Content"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_Y"),
            LocalizableText.Localized("FileExistsOverwrite_MsgBox_N"));

        private UniTaskCompletionSource<ProjectModel> _tcs;
        private CancellationTokenSource? _sharedCts;


        private void Start()
        {
            _dialog.CloseButton.OnClick.AddListener(() =>
            {
                _sharedCts?.Cancel();
                _tcs.TrySetCanceled();
            });

            _projectNameInput.OnValueChanged.AddListener(val =>
            {
                if (string.IsNullOrEmpty(val) || !Utils.IsValidFileName(val)) {
                    _projectNameErrorText.gameObject.SetActive(true);
                    UpdateButtonsState();
                }
                else {
                    _projectNameErrorText.gameObject.SetActive(false);
                    UpdateButtonsState();
                    SavePath = Path.Combine(_directoryInput.Value, $"{val}.dnt");
                }
            });

            _audioFileInput.OnValueChanged.AddListener(val =>
            {
                if (string.IsNullOrEmpty(val) || !Utils.IsValidPath(val)) {
                    _audioFileErrorText.gameObject.SetActive(true);
                    UpdateButtonsState();
                }
                else {
                    _audioFileErrorText.gameObject.SetActive(false);
                    UpdateButtonsState();
                    if (_saveToAudioDirectory) {
                        // Use audio dir as save dir
                        SavePath = Path.Combine(Path.GetDirectoryName(val), $"{_projectNameInput.Value}.dnt");
                    }
                }
            });
            _audioFileExploreButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                var res = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(MainSystem.Args.SupportLoadAudioFileExtensions);
                if (res.IsCancelled)
                    return;

                _audioFileInput.Value = res.Path;
            });

            _directoryInput.OnValueChanged.AddListener(val =>
            {
                if (string.IsNullOrEmpty(val) || !Utils.IsValidPath(val)) {
                    _directoryErrorText.gameObject.SetActive(true);
                    UpdateButtonsState();
                }
                else {
                    _directoryErrorText.gameObject.SetActive(false);
                    UpdateButtonsState();
                    SavePath = Path.Combine(Path.GetDirectoryName(val), $"{_projectNameInput.Value}.dnt");
                }
            });
            _directoryExploreButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                var res = await MainSystem.FileExplorerDialog.OpenSelectDirectoryAsync();
                if (res.IsCancelled)
                    return;

                _directoryInput.Value = res.Path;
            });
            _sameDirectoryCheckBox.OnValueChanged.AddListener(saveInSameDir =>
            {
                if (saveInSameDir.GetValueOrDefault()) {
                    _directoryInput.IsInteractable = false;
                    _directoryExploreButton.IsInteractable = false;
                    _saveToAudioDirectory = true;
                }
                else {
                    _directoryInput.IsInteractable = true;
                    _directoryExploreButton.IsInteractable = true;
                    _saveToAudioDirectory = false;
                }
            });

            // Buttons

            _createButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                string audioFilePath = _audioFileInput.Value;
                if (!File.Exists(audioFilePath)) {
                    await MainSystem.MessageBoxDialog.OpenAsync(_audioNotExistsMsgBoxArgs);
                    return;
                }
                if (Directory.Exists(SavePath)) {
                    await MainSystem.MessageBoxDialog.OpenAsync(_dirExistsMsgBoxArgs);
                    return;
                }
                if (File.Exists(SavePath)) {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_fileExistsMsgBoxArgs);
                    if (res != 0)
                        return;
                }

                // Load Audio

                using var audioFs = File.OpenRead(audioFilePath);
                var cts = _sharedCts = new CancellationTokenSource();
                AudioClip? clip;
                try {
                    clip = await AudioUtils.LoadAsync(audioFs, Path.GetExtension(audioFilePath), cts.Token);
                } catch (TaskCanceledException) {
                    return;
                } finally {
                    _sharedCts.Dispose();
                    _sharedCts = null;
                }

                if (clip is null) {
                    await MainSystem.MessageBoxDialog.OpenAsync(_audioLoadFailedMsgBoxArgs);
                    return;
                }

                // Create Project
                // TODO: Save as ref path? i havent impl that now.
                byte[] audioBytes = new byte[audioFs.Length];
                audioFs.Seek(0, SeekOrigin.Begin);
                audioFs.Read(audioBytes);
                var proj = new ProjectModel() {
                    AudioClip = clip,
                    AudioFileData = audioBytes,
                    MusicName = Path.GetFileNameWithoutExtension(audioFilePath),
                    ProjectFilePath = SavePath,
                };
                _resultProject = proj;

                _tcs.TrySetResult(proj);
            });
            _cancelButton.OnClick.AddListener(() =>
            {
                _dialog.Close();
                _tcs?.TrySetCanceled();
            });
        }

        private void UpdateButtonsState()
        {
            var active = !_projectNameErrorText.gameObject.activeSelf
                && !_audioFileErrorText.gameObject.activeSelf
                && !_directoryErrorText.gameObject.activeSelf;

            _createButton.IsInteractable = active;
        }

        public async UniTask<ProjectModel?> OpenCreateNewAsync()
        {
            _tcs = new UniTaskCompletionSource<ProjectModel>();
            using var s_d = _dialog.Open();

            try {
                return await _tcs.Task;
            } catch (TaskCanceledException) {
                return null;
            } finally {
                _tcs = null;
            }
        }
    }
}