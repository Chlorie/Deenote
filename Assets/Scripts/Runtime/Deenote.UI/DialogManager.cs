#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Dialogs;
using Deenote.UI.Dialogs.Elements;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public UniTask<FileExplorerDialog.Result> OpenFileExplorerSelectFileAsync(LocalizableText dialogTitle, ImmutableArray<string> extensionFilters, string? initialDirectory = null)
        {
            return _fileExplorerDialog.OpenSelectFileAsync(dialogTitle, extensionFilters, initialDirectory); ;
        }

        public UniTask<FileExplorerDialog.Result> OpenFileExplorerSelectDirectoryAsync(LocalizableText dialogTitle, string? initialDirectory = null)
        {
            return _fileExplorerDialog.OpenSelectDirectoryAsync(dialogTitle, initialDirectory); ;
        }

        /// <summary>
        /// Open file explorer and input filename
        /// </summary>
        /// <param name="dialogTitle"></param>
        /// <param name="defaultInput"></param>
        /// <param name="fileExtension"></param>
        /// <param name="initialDirectory"></param>
        /// <returns></returns>
        public UniTask<FileExplorerDialog.Result> OpenFileExplorerInputFileAsync(LocalizableText dialogTitle, string? defaultInput, string? fileExtension = null, string? initialDirectory = null)
        {
            return _fileExplorerDialog.OpenInputFileAsync(dialogTitle, defaultInput, fileExtension, initialDirectory);
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

    }
}