#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using System;
using System.IO;
using System.Threading;
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

        private bool IsSaveToAudioDirectory => _sameDirectoryCheckBox.Value.GetValueOrDefault();

        private string? __savePath;
        private string? SavePath => __savePath;

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

        private UniTaskCompletionSource<ProjectModel> _tcs = default!; // Init on Open
        private CancellationTokenSource? _sharedCts;


        private void Start()
        {
            _dialog.CloseButton.OnClick.AddListener(OnClose);

            _projectNameInput.OnValueChanged.AddListener(val =>
            {
                if (string.IsNullOrEmpty(val) || !Utils.IsValidFileName(val)) {
                    _projectNameErrorText.gameObject.SetActive(true);
                    UpdateSavePath(false);
                }
                else {
                    _projectNameErrorText.gameObject.SetActive(false);
                    UpdateSavePath(true);
                }
            });

            _audioFileInput.OnValueChanged.AddListener(val =>
            {
                if (IsValidPath(val)) {
                    _audioFileErrorText.gameObject.SetActive(false);
                    if (IsSaveToAudioDirectory) {
                        UpdateSavePath(true);
                    }
                }
                else {
                    _audioFileErrorText.gameObject.SetActive(true);
                    if (IsSaveToAudioDirectory) {
                        UpdateSavePath(false);
                    }
                }
            });
            _audioFileExploreButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                var res = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(
                    LocalizableText.Localized("SelectAudio_FileExplorer_Title"),
                    MainSystem.Args.SupportLoadAudioFileExtensions);
                if (res.IsCancelled)
                    return;

                if (string.IsNullOrWhiteSpace(_projectNameInput.Value))
                    _projectNameInput.Value = Path.GetFileNameWithoutExtension(res.Path);

                _audioFileInput.Value = res.Path;
            });

            _directoryInput.OnValueChanged.AddListener(val =>
            {
                if (IsValidPath(val)) {
                    _directoryErrorText.gameObject.SetActive(false);
                    UpdateSavePath(true);
                }
                else {
                    _directoryErrorText.gameObject.SetActive(true);
                    UpdateSavePath(false);
                }
            });
            _directoryExploreButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                var res = await MainSystem.FileExplorerDialog.OpenSelectDirectoryAsync(
                    LocalizableText.Localized("NewProject_FileExplorer_SelectDirectory_Title"));
                if (res.IsCancelled)
                    return;

                _directoryInput.Value = res.Path;
            });
            _sameDirectoryCheckBox.OnValueChanged.AddListener(saveInSameDir =>
            {
                if (saveInSameDir.GetValueOrDefault()) {
                    _directoryInput.IsInteractable = false;
                    _directoryExploreButton.IsInteractable = false;
                    _directoryErrorText.gameObject.SetActive(false);
                }
                else {
                    _directoryInput.IsInteractable = true;
                    _directoryExploreButton.IsInteractable = true;
                    _directoryInput.OnValueChanged.Invoke(_directoryInput.Value);
                }
                UpdateSavePath(null);
            });

            _projectNameInput.Value = "";
            _audioFileInput.Value = "";
            _directoryInput.OnValueChanged.Invoke(_directoryInput.Value);
            _sameDirectoryCheckBox.Value = false;

            // Buttons

            _createButton.OnClick.AddListener(async UniTaskVoid () =>
            {
                string audioFilePath = _audioFileInput.Value;
                if (!File.Exists(audioFilePath)) {
                    await MainSystem.MessageBoxDialog.OpenAsync(_audioNotExistsMsgBoxArgs);
                    return;
                }
                Debug.Assert(SavePath is not null);
                string savePath = SavePath!;
                if (Directory.Exists(savePath)) {
                    await MainSystem.MessageBoxDialog.OpenAsync(_dirExistsMsgBoxArgs);
                    return;
                }
                if (File.Exists(savePath)) {
                    var res = await MainSystem.MessageBoxDialog.OpenAsync(_fileExistsMsgBoxArgs);
                    if (res != 0)
                        return;
                }

                // Load Audio

                using var audioFs = File.OpenRead(audioFilePath);
                var cts = _sharedCts = new CancellationTokenSource();
                AudioClip? clip;
                try {
                    SetAllInputsInteractable(false);
                    clip = await AudioUtils.LoadAsync(audioFs, Path.GetExtension(audioFilePath), cts.Token);
                } catch (OperationCanceledException) {
                    Debug.Log("Create new project cancelled.");
                    return;
                } finally {
                    SetAllInputsInteractable(true);
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
                    MusicName = _projectNameInput.Value,
                    ProjectFilePath = savePath!,
                    AudioFileRelativePath = Path.GetRelativePath(savePath, audioFilePath),
                };
                proj.Charts.Add(new ChartModel(new()) {
                    Difficulty = Difficulty.Hard,
                    Level = "10",
                });

                _tcs.TrySetResult(proj);
            });
            _cancelButton.OnClick.AddListener(() =>
            {
                _dialog.Close();
                OnClose();
            });

            void OnClose()
            {
                _sharedCts?.Cancel();
                _tcs.TrySetCanceled();
            }
        }

        private void SetAllInputsInteractable(bool interactable)
        {
            _projectNameInput.IsInteractable = interactable;
            _audioFileInput.IsInteractable = interactable;
            _audioFileExploreButton.IsInteractable = interactable;
            _directoryInput.IsInteractable = interactable;
            _directoryExploreButton.IsInteractable = interactable;
            _sameDirectoryCheckBox.IsInteractable = interactable;
            _createButton.IsInteractable = interactable;
        }

        /// <param name="isValid">When null, this method will check the path</param>
        private void UpdateSavePath(bool? isValid)
        {
            if (isValid is false)
                goto SetInvalid;

            string dir;
            if (IsSaveToAudioDirectory) {
                var audio = _audioFileInput.Value;
                if (isValid is null && !IsValidPath(audio))
                    goto SetInvalid;
                dir = Path.GetDirectoryName(audio);
            }
            else {
                dir = _directoryInput.Value;
                if (isValid is null && !IsValidPath(dir))
                    goto SetInvalid;
            }
            __savePath = Path.Combine(dir, $"{_projectNameInput.Value}.dnt");
            _createHintText.SetLocalizedText("NewProjectDialog_Directory_CreateHint", __savePath);
            _createButton.IsInteractable = true;
            return;

        SetInvalid:
            __savePath = null;
            _createHintText.SetRawText("");
            _createButton.IsInteractable = false;
        }

        private static bool IsValidPath(string input)
            => !string.IsNullOrWhiteSpace(input) && Utils.IsValidPath(input) && Path.IsPathFullyQualified(input);

        public async UniTask<ProjectModel?> OpenCreateNewAsync()
        {
            _tcs = new UniTaskCompletionSource<ProjectModel>();
            using var s_d = _dialog.Open();

            try {
                return await _tcs.Task;
            } catch (OperationCanceledException) {
                Debug.Log("Create new project cancelled.");
                return null;
            } finally {
                _tcs = null!;
            }
        }
    }
}