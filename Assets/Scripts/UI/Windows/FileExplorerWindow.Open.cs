using Cysharp.Threading.Tasks;
using System.IO;

namespace Deenote.UI.Windows
{
    partial class FileExplorerWindow
    {
        /// <summary>
        /// Open dialog and select a file
        /// </summary>
        /// <param name="extensionFilters">Filtered extensions</param>
        /// <param name="initialDirectory">
        /// The listed directory when dialog is open,
        /// list the last explored dir if <see langword="null" />
        /// </param>
        /// <returns></returns>
        public async UniTask<Result> OpenSelectFileAsync(string[] extensionFilters, string? initialDirectory = null)
        {
            if (await OpenAsync(extensionFilters, initialDirectory))
                return new(_selectedFilePath);
            else
                return default;
        }

        public async UniTask<Result> OpenSelectDirectoryAsync(string? initialDirectory = null)
        {
            if (await OpenAsync(null, initialDirectory))
                return new(CurrentDirectory);
            else
                return default;
        }

        /// <summary>
        /// Open dialog and input a fileName
        /// </summary>
        /// <param name="fileExtension">The extension of inputted file</param>
        /// <param name="initialDirectory"></param>
        /// <returns></returns>
        public async UniTask<Result> OpenInputFileAsync(string? fileExtension = null, string? initialDirectory = null)
        {
            if (await OpenAsync(null, initialDirectory, true, fileExtension))
                return new(Path.Combine(CurrentDirectory!, $"{_fileNameInputField.text}{fileExtension}"));
            else
                return default;
        }

        private async UniTask<bool> OpenAsync(string[]? extensionFilters, string? initialDirectory = null,
            bool inputMode = false, string? inputModeFileExtension = null)
        {
            Window.IsActivated = true;
            MainSystem.WindowsManager.FocusOn(Window);

            using var confirmHandler = _confirmButton.GetAsyncClickEventHandler();
            using var cancelHandler = _cancelButton.GetAsyncClickEventHandler();
            var clickTask = UniTask.WhenAny(confirmHandler.OnClickAsync(), cancelHandler.OnClickAsync());

            if (extensionFilters is null) { // select directory
                _confirmButton.interactable = true;
                _extensionFilters = null;
                _fileNameInputField.gameObject.SetActive(false);
            }
            else { // Select file
                _confirmButton.interactable = false;
                _extensionFilters = extensionFilters;
                _fileNameInputField.gameObject.SetActive(true);
            }

            if (inputMode) {
                _fileNameInputField.interactable = true;
                if (inputModeFileExtension is null) {
                    _fileExtensionText.gameObject.SetActive(false);
                }
                else {
                    _fileExtensionText.gameObject.SetActive(true);
                    _fileExtensionText.text = inputModeFileExtension;
                }
            }
            else {
                _fileNameInputField.interactable = false;
                _fileExtensionText.gameObject.SetActive(false);
            }

            ResetFileList(initialDirectory);

            var index = await clickTask;
            Window.IsActivated = false;
            return index == 0;
        }

        public readonly struct Result
        {
            public readonly string Path;

            public bool IsCancelled => Path is null;

            internal Result(string path)
            {
                Path = path;
            }
        }
    }
}