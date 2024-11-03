#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System.Collections.Immutable;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    partial class FileExplorerDialog
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
        public async UniTask<Result> OpenSelectFileAsync(LocalizableText dialogTitle, ImmutableArray<string> extensionFilters, string? initialDirectory = null)
        {
            PathFilter filter = extensionFilters.IsDefaultOrEmpty
                ? PathFilter.NoFilter
                : PathFilter.FilterByExtensions(extensionFilters);
            if (await OpenAsync(dialogTitle, filter, initialDirectory)) {
                Debug.Assert(CurrentSelectedFilePath != null);
                return new Result(CurrentSelectedFilePath!);
            }
            return default;
        }

        public async UniTask<Result> OpenSelectDirectoryAsync(LocalizableText dialogTitle, string? initialDirectory = null)
        {
            if (await OpenAsync(dialogTitle, PathFilter.DirectoriesOnly, initialDirectory))
                return new Result(CurrentDirectory);
            return default;
        }

        /// <summary>
        /// Open dialog and input a fileName
        /// </summary>
        /// <param name="fileExtension">The extension of inputted file</param>
        /// <param name="initialDirectory"></param>
        /// <returns></returns>
        public async UniTask<Result> OpenInputFileAsync(LocalizableText dialogTitle, string? fileExtension = null, string? initialDirectory = null)
        {
            PathFilter filter = fileExtension is null
                ? PathFilter.NoFilter
                : PathFilter.FilterByExtensions(ImmutableArray.Create(fileExtension));
            if (await OpenAsync(dialogTitle, filter, initialDirectory, inputMode: true, inputModeExtension: fileExtension))
                return new Result(Path.Combine(CurrentDirectory, $"{CurrentInputFileName}{fileExtension}"));
            return default;
        }

        private async UniTask<bool> OpenAsync(LocalizableText dialogTitle, PathFilter filter, string? initialDirectory = null, bool inputMode = false, string? inputModeExtension = null)
        {
            using var s_dialogOpen = _dialog.Open();
            _dialog.SetTitle(dialogTitle);

            using var comfirmHandler = _confirmButton.GetAsyncClickEventHandler();
            using var cancelHandler = _cancelButton.GetAsyncClickEventHandler();
            using var closeHandler = _dialog.CloseButton.GetAsyncClickEventHandler();
            var clickTask = UniTask.WhenAny(comfirmHandler.OnClickAsync(), cancelHandler.OnClickAsync(), closeHandler.OnClickAsync());

            _pathFilter = filter;
            switch (filter.Kind) {
                case PathFilterKind.NoFilter:
                case PathFilterKind.FilterByExtensions:
                    _fileNameColumnGameObject.SetActive(true);
                    if (inputMode == true) {
                        _fileNameInput.IsInteractable = true;
                        if (inputModeExtension is null)
                            _extensionText.gameObject.SetActive(false);
                        else {
                            _extensionText.gameObject.SetActive(true);
                            _extensionText.text = inputModeExtension;
                        }
                    }
                    else {
                        _fileNameInput.IsInteractable = false;
                        _extensionText.gameObject.SetActive(false);
                    }
                    break;
                case PathFilterKind.DirectoriesOnly:
                    _confirmButton.IsInteractable = true;
                    _fileNameColumnGameObject.SetActive(false);
                    break;
                default:
                    break;
            }

            if (!TryNavigateToDirectory(initialDirectory)) {
                RefreshWithCurrentDirectory();
            }

            int click = await clickTask;
            Debug.Log($@"In {nameof(FileExplorerDialog)} clicked {click switch { 0 => "Confirm", 1 => "Cancel", _ => "Close", }}");
            return click == 0;
        }

        public readonly struct Result
        {
            private readonly string? _path;

            public string Path => _path!;

            public bool IsCancelled => _path is null;

            internal Result(string path)
            {
                _path = path;
            }
        }
    }
}