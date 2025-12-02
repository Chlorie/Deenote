#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    partial class FileExplorerDialog
    {
        private const string DefaultFileExplorerTitleLocalizedKey = "Dialog_FileExplorer_Title";

        internal async UniTask<Result> OpenAsync(LocalizableText? dialogTitle, FilePathFilter pathFilter, InputBar inputBar, string? initialDirectory = null)
        {
            OpenSelfModalDialog();

            _dialog.SetTitle(dialogTitle ?? LocalizableText.Localized(DefaultFileExplorerTitleLocalizedKey));

            var clickTask = UniTask.WhenAny(
                _confirmButton.OnClickAsync(),
                _cancelButton.OnClickAsync(),
                _dialog.CloseButton.OnClickAsync());

            // Initialization

            _pathFilter = pathFilter;
            CurrentSelectedFilePath = null;
            SetInputBar(inputBar);

            NavigateToDirectory(initialDirectory ?? CurrentDirectory, true, true);

            // Wait

            int click = await clickTask;
            Debug.Log($@"In {nameof(FileExplorerDialog)} clicked {click switch { 0 => "Confirm", 1 => "Cancel", _ => "Close", }}");

            CloseSelfModalDialog();

            return new Result(this, click != 0);
        }

        public readonly struct Result
        {
            private readonly FileExplorerDialog _dialog;
            private readonly bool _cancelled;

            public bool IsCancelled => _cancelled;

            public Result(FileExplorerDialog dialog,bool cancelled)
            {
                _dialog = dialog;
                _cancelled = cancelled;
            }

            public string CurrentDirectory => _dialog.CurrentDirectory;
            public string? SelectedFilePath => _dialog.CurrentSelectedFilePath;
            public string? InputFileNameWithoutExtension => _dialog._fileNameInput.Value;
        }
    }
}