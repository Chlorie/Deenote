#nullable enable

using Deenote.UIFramework.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Library;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
using Deenote.Library.Collections;
using Deenote.Library.IO;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed partial class FileExplorerDialog : ModalDialog
    {
        [SerializeField] Dialog _dialog = default!;

        [Header("Explorer")]
        [SerializeField] Button _parentDirectoryButton = default!;
        [SerializeField] TextBox _directoryInput = default!;
        [SerializeField] Button _openInSystemExplorerButton = default!;
        [SerializeField] Transform _fileListContentTransform = default!;
        [SerializeField] Transform _fileNameColumnTransform = default!;
        [SerializeField] TextBox _fileNameInput = default!;
        [SerializeField] TextBlock _fileNameText = default!;
        [SerializeField] Button _confirmButton = default!;
        [SerializeField] Button _cancelButton = default!;

        [Header("Prefabs")]
        [SerializeField] FileExplorerFileListItem _fileItemPrefab = default!;
        private PooledObjectListView<FileExplorerFileListItem> _fileItems = default!;

        private FilePathFilter _pathFilter;
        private InputBar _inputBar;

        private string _currentDirectory_bf = default!; // Use TryNavigateToDirectory to edit
        private string? _currentSelectedFile_bf;

        public string CurrentDirectory => _currentDirectory_bf;
        public string? CurrentSelectedFilePath
        {
            get => _currentSelectedFile_bf;
            private set {
                if (Utils.SetField(ref _currentSelectedFile_bf, value)) {
                    switch (_inputBar.Kind) {
                        case InputBarKind.ReadOnly:
                            _fileNameText.SetRawText(Path.GetFileName(value));
                            _confirmButton.IsInteractable = !string.IsNullOrEmpty(value);
                            break;
                        case InputBarKind.Input:
                            if (_inputBar.HintExtension is null)
                                _fileNameInput.Value = Path.GetFileName(value);
                            else
                                _fileNameInput.Value = Path.GetFileNameWithoutExtension(value);
                            break;
                        case InputBarKind.Collapsed:
                        default:
                            break;
                    }
                }
            }
        }

        private void SetInputBar(InputBar inputBar)
        {
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
                    _confirmButton.IsInteractable = PathUtils.IsValidFileName(inputBar.InputText);
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _fileItems = new(UnityUtils.CreateObjectPool(_fileItemPrefab, _fileListContentTransform,
                item => item.OnInstantiate(this)));

            Awake_Pinned();
            NavigateToDirectory(Directory.GetCurrentDirectory()); // App initialization

            _parentDirectoryButton.Clicked += () =>
            {
                NavigateToDirectory(Path.GetDirectoryName(CurrentDirectory), true, true);
            };
            _directoryInput.EditSubmitted += path =>
            {
                if (TryNavigateToDirectory(path))
                    return;
                NavigateToDirectory(CurrentDirectory);
            };
            _fileNameInput.ValueChanged += fileName =>
            {
                if (_inputBar.Kind is InputBarKind.Input)
                    _confirmButton.IsInteractable = PathUtils.IsValidFileName(fileName);
            };

#if UNITY_STANDALONE_WIN
            _openInSystemExplorerButton.Clicked += () =>
            {
                // We locate to the file only if selected file is in current displaying directory
                if (CurrentSelectedFilePath is not null
                    && Path.GetDirectoryName(CurrentSelectedFilePath.AsSpan()).SequenceEqual(CurrentDirectory)) {
                    //                                                             Ensure path seperator charater is valid
                    System.Diagnostics.Process.Start("explorer.exe", $@"/select,""{Path.GetFullPath(CurrentSelectedFilePath)}""");
                }
                else {
                    System.Diagnostics.Process.Start("explorer.exe", Path.GetFullPath(CurrentDirectory));
                }
            };
#else
            _openInSystemExplorerButton.gameObject.SetActive(false);
#endif
        }

        private void OnDisable()
        {
            // We choose to clear pool because in most of time, we needn't use file explorer
            _fileItems.Clear(clearPool: true);
        }

        #region Directory Navigation

        public bool TryNavigateToDirectory([AllowNull] string directory)
        {
            if (Directory.Exists(directory)) {
                NavigateToDirectory(directory!);
                return true;
            }
            return false;
        }

        private void NavigateToDirectory(string directory, bool fallbackToParent = false, bool fallbackToCurrentDirectory = false)
        {
            if (Directory.Exists(directory)) {
                NavigateToDirectory(directory);
                return;
            }
            else if (!fallbackToParent) {
                throw new DirectoryNotFoundException($"Cannot navigate to {directory}");
            }

            while (true) {
                directory = Path.GetDirectoryName(directory);
                if (directory is null)
                    break;
                if (Directory.Exists(directory)) {
                    NavigateToDirectory(directory);
                }
            }
            if (fallbackToCurrentDirectory)
                NavigateToDirectory(Directory.GetCurrentDirectory());
            else
                throw new DirectoryNotFoundException($"Cannot navigate to {directory} nor its parent directories");
        }

        private void NavigateToDirectory(string directory)
        {
            Debug.Assert(Directory.Exists(directory));

            if (CurrentDirectory != directory) {
                _currentDirectory_bf = directory;
                _directoryInput.SetValueWithoutNotify(directory);
            }

            RefreshFileList();

            _parentDirectoryButton.IsInteractable = Path.GetDirectoryName(directory) is not null;
        }

        private void RefreshFileList()
        {
            using (var resetter = _fileItems.Resetting()) {
                foreach (var dir in Directory.GetDirectories(CurrentDirectory)) {
                    resetter.Add(out var item);
                    item.Initialize(dir, true);
                }

                if (_pathFilter.Kind is not PathFilterKind.DirectoriesOnly) {
                    foreach (var file in Directory.GetFiles(CurrentDirectory)) {
                        if (_pathFilter.IsFilePathShouldShow(file)) {
                            resetter.Add(out var item);
                            item.Initialize(file, false);
                        }
                    }
                }
            }
            _fileItems.SetSiblingIndicesInOrder();
        }

        #endregion

        internal void SelectFileItem(FileExplorerFileListItem item)
        {
            if (item.IsDirectory) {
                NavigateToDirectory(item.Path);
            }
            else {
                CurrentSelectedFilePath = item.Path;
            }
        }

        internal enum PathFilterKind
        {
            NoFilter,
            DirectoriesOnly,
            FilterByExtensions,
        }

        private void OnValidate()
        {
            _dialog ??= GetComponent<Dialog>();
        }

        internal readonly struct FilePathFilter
        {
            private readonly ImmutableArray<string> _filters;

            public PathFilterKind Kind
            {
                get {
                    if (_filters.IsDefault)
                        return PathFilterKind.DirectoriesOnly;
                    if (_filters.IsEmpty)
                        return PathFilterKind.NoFilter;
                    return PathFilterKind.FilterByExtensions;
                }
            }

            internal bool IsFilePathShouldShow(string path)
            {
                switch (Kind) {
                    case PathFilterKind.NoFilter:
                        return true;
                    case PathFilterKind.DirectoriesOnly:
                        return false;
                    case PathFilterKind.FilterByExtensions:
                        foreach (var filter in _filters) {
                            if (path.EndsWith(filter, StringComparison.OrdinalIgnoreCase))
                                return true;
                        }
                        return false;
                    default:
                        return true;
                }
            }

            private FilePathFilter(ImmutableArray<string> filters)
            {
                _filters = filters;
            }

            public static FilePathFilter NoFilter => new(ImmutableArray<string>.Empty);

            public static FilePathFilter DirectoriesOnly => default;

            public static FilePathFilter FilterByExtensions(ImmutableArray<string> extensionFilters)
                => extensionFilters.IsDefaultOrEmpty ? default : new(extensionFilters);
        }

        internal enum InputBarKind
        {
            Collapsed,
            ReadOnly,
            Input,
        }

        internal readonly struct InputBar
        {
            public InputBarKind Kind { get; }

            public string? InputText { get; }

            public string? HintExtension { get; }

            private InputBar(InputBarKind kind, string? inputText = null, string? hintExtension = null)
            {
                Kind = kind;
                InputText = inputText;
                HintExtension = hintExtension;
            }

            public static InputBar Collapsed => new(InputBarKind.Collapsed);

            public static InputBar ReadOnly => new(InputBarKind.ReadOnly);

            public static InputBar WithDefaultText(string? input, string? extension)
                => new(InputBarKind.Input, input, extension);
        }
    }
}