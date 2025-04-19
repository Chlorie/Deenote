#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Cysharp.Threading.Tasks;
using Deenote.Library.Collections;
using Deenote.Localization;
using Deenote.UI.Dialogs;
using Deenote.UI.Dialogs.Elements;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Deenote.UI
{
    public sealed class DialogManager : MonoBehaviour
    {
        [SerializeField] MessageBox _messageBox = default!;
        [SerializeField] FileExplorerDialog _fileExplorerDialog = default!;
        [SerializeField] NewProjectDialog _newProjectDialog = default!;
        [SerializeField] PreferencesDialog _preferencesDialog = default!;
        [SerializeField] AboutDialog _aboutDialog = default!;
        [SerializeField] GameObject _raycastBlocker = default!;

        private readonly Stack<ModalDialog> _activeDialogs = new();

        public NewProjectDialog NewProjectDialog => _newProjectDialog;
        public PreferencesDialog PreferencesDialog => _preferencesDialog;
        public AboutDialog AboutDialog => _aboutDialog;

        internal List<string>? _configtmpFileExplorerPinned;

        private void Awake()
        {
            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.AddList("ui/file_exploer/pinned", _fileExplorerDialog.GetPinnedItems() ?? _configtmpFileExplorerPinned);
                configs.Add("ui/new_proj/same_dir", _newProjectDialog._saveToAudioDirectory);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                _configtmpFileExplorerPinned = configs.GetStringList("ui/file_exploer/pinned");
                _newProjectDialog._saveToAudioDirectory = configs.GetBoolean("ui/new_proj/same_dir", false);
            };
        }

        public UniTask<int> OpenMessageBoxAsync(in MessageBoxArgs data, ReadOnlySpan<string> contentArgs = default)
        {
            return _messageBox.OpenAsync(data, contentArgs);
        }

        public UniTask<int> OpenMessageBoxAsync(in MessageBoxArgs data, string contentArg0, string contentArg1)
        {
            using var so = SpanOwner<string>.Allocate(2);
            var span = so.Span;
            span[0] = contentArg0;
            span[1] = contentArg1;
            return OpenMessageBoxAsync(data, contentArgs: span);
        }

        /// <summary>
        /// Open file explorer to select a file
        /// </summary>
        /// <param name="extensionFilters">Filtered extensions</param>
        /// <param name="initialDirectory">
        /// The listed directory when dialog is open,
        /// list the last explored dir if <see langword="null" />
        /// </param>
        /// <returns></returns>
        public async UniTask<FileExploreResult> OpenFileExplorerSelectFileAsync(LocalizableText? dialogTitle, ImmutableArray<string> extensionFilters, string? initialDirectory = null)
        {
            FileExplorerDialog.FilePathFilter filter = extensionFilters.IsDefaultOrEmpty
                ? FileExplorerDialog.FilePathFilter.NoFilter
                : FileExplorerDialog.FilePathFilter.FilterByExtensions(extensionFilters);

            var res = await _fileExplorerDialog.OpenAsync(dialogTitle, filter, FileExplorerDialog.InputBar.ReadOnly, initialDirectory);
            if (res.IsCancelled)
                return default;

            Debug.Assert(res.SelectedFilePath is not null);
            return new FileExploreResult(res.SelectedFilePath!);
        }

        public async UniTask<FileExploreResult> OpenFileExplorerSelectDirectoryAsync(LocalizableText? dialogTitle, string? initialDirectory = null)
        {
            var res = await _fileExplorerDialog.OpenAsync(dialogTitle, FileExplorerDialog.FilePathFilter.DirectoriesOnly, FileExplorerDialog.InputBar.Collapsed, initialDirectory);
            if (res.IsCancelled)
                return default;
            return new(res.CurrentDirectory);
        }

        /// <summary>
        /// Open file explorer and input filename
        /// </summary>
        /// <param name="dialogTitle"></param>
        /// <param name="defaultInput"></param>
        /// <param name="fileExtension"></param>
        /// <param name="initialDirectory"></param>
        /// <returns></returns>
        public async UniTask<FileExploreResult> OpenFileExplorerInputFileAsync(LocalizableText? dialogTitle, string? defaultInput, string? fileExtension = null, ImmutableArray<string> extensionFilters = default, string? initialDirectory = null)
        {
            var filter = (fileExtension, extensionFilters) switch {
                (_, { IsDefaultOrEmpty: false }) => FileExplorerDialog.FilePathFilter.FilterByExtensions(extensionFilters),
                (null, { IsDefaultOrEmpty: true }) => FileExplorerDialog.FilePathFilter.NoFilter,
                (not null, { IsDefaultOrEmpty: true }) => FileExplorerDialog.FilePathFilter.FilterByExtensions(ImmutableArray.Create(fileExtension)),
            };
            var res = await _fileExplorerDialog.OpenAsync(dialogTitle, filter, FileExplorerDialog.InputBar.WithDefaultText(defaultInput, fileExtension), initialDirectory);
            if (res.IsCancelled)
                return default;
            return new(Path.Combine(res.CurrentDirectory, $"{res.InputFileNameWithoutExtension}{fileExtension}"));
        }

        public void RegisterModalDialog(ModalDialog dialog)
        {
            dialog.IsActiveChanged += (dlg, active) =>
            {
                if (active) {
                    _activeDialogs.Push(dlg);
                    _raycastBlocker.SetActive(true);
                    _raycastBlocker.transform.SetAsLastSibling();
                    dlg.transform.SetAsLastSibling();
                }
                else {
                    var popped = _activeDialogs.Pop();
                    Debug.Assert(popped == dlg);
                    if (_activeDialogs.TryPeek(out var top)) {
                        top.transform.SetAsLastSibling();
                    }
                    else {
                        _raycastBlocker.SetActive(false);
                    }
                }
            };
        }

        public readonly struct FileExploreResult
        {
            private readonly string? _path;

            public string Path => _path!;

            public bool IsCancelled => _path is null;

            internal FileExploreResult(string path)
            {
                _path = path;
            }
        }
    }
}