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
            if (await OpenAsync(dialogTitle, filter, InputBar.ReadOnly, initialDirectory)) {
                Debug.Assert(CurrentSelectedFilePath != null);
                return new Result(CurrentSelectedFilePath!);
            }
            return default;
        }

        public async UniTask<Result> OpenSelectDirectoryAsync(LocalizableText dialogTitle, string? initialDirectory = null)
        {
            if (await OpenAsync(dialogTitle, PathFilter.DirectoriesOnly, InputBar.Collapsed, initialDirectory))
                return new Result(CurrentDirectory);
            return default;
        }

        /// <summary>
        /// Open dialog and input a fileName
        /// </summary>
        /// <param name="fileExtension">The extension of inputted file</param>
        /// <param name="initialDirectory"></param>
        /// <returns></returns>
        public async UniTask<Result> OpenInputFileAsync(LocalizableText dialogTitle, string? defaultInput, string? fileExtension = null, string? initialDirectory = null)
        {
            PathFilter filter = fileExtension is null
                ? PathFilter.NoFilter
                : PathFilter.FilterByExtensions(ImmutableArray.Create(fileExtension));
            if (await OpenAsync(dialogTitle, filter, InputBar.WithDefaultText(defaultInput, fileExtension), initialDirectory))
                return new Result(Path.Combine(CurrentDirectory, $"{_fileNameInput.Value}{fileExtension}"));
            return default;
        }

        private async UniTask<bool> OpenAsync(LocalizableText dialogTitle, PathFilter filter, InputBar inputBar, string? initialDirectory = null)
        {
            OpenSelfModalDialog();

            _dialog.SetTitle(dialogTitle);

            var clickTask = UniTask.WhenAny(
                _confirmButton.OnClickAsync(),
                _cancelButton.OnClickAsync(),
                _dialog.CloseButton.OnClickAsync());

            // Initialization

            _pathFilter = filter;
            _inputBar = inputBar;
            switch (inputBar.Kind) {
                case InputBarKind.Collapsed:
                    _fileNameColumnTransform.gameObject.SetActive(false);
                    _confirmButton.IsInteractable = true;
                    break;
                case InputBarKind.ReadOnly:
                    _fileNameColumnTransform.gameObject.SetActive(true);
                    _fileNameInput.gameObject.SetActive(false);
                    _fileNameText.SetRawText("");
                    _confirmButton.IsInteractable = false;
                    break;
                case InputBarKind.Input:
                    _fileNameColumnTransform.gameObject.SetActive(true);
                    _fileNameInput.gameObject.SetActive(true);
                    _fileNameText.SetRawText(inputBar.HintExtension);
                    if (inputBar.InputText is { } it) {
                        _fileNameInput.Value = it;
                    }
                    _confirmButton.IsInteractable = false;
                    break;
            }
            CurrentSelectedFilePath = null;

            NavigateToDirectory(initialDirectory ?? CurrentDirectory, true, true);

            // Wait

            int click = await clickTask;
            Debug.Log($@"In {nameof(FileExplorerDialog)} clicked {click switch { 0 => "Confirm", 1 => "Cancel", _ => "Close", }}");

            CloseSelfModalDialog();

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