using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.IO;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed partial class FileExplorerDialog : MonoBehaviour
    {
        [SerializeField] Dialog _dialog = default!;

        [Header("Content")]
        [SerializeField] InputField _directoryInput = default!;
        [SerializeField] Transform _fileItemParentTransform = default!;
        [SerializeField] GameObject _fileNameColumnGameObject = default!;
        [SerializeField] InputField _fileNameInput = default!;
        [SerializeField] TMP_Text _extensionText = default!;
        [SerializeField] Button _confirmButton = default!;
        [SerializeField] Button _cancelButton = default!;
        [Header("Prefabs")]
        [SerializeField] FileExplorerListItem _fileExplorerItemPrefab = default!;

        private PooledObjectListView<FileExplorerListItem> _fileItems;

        private PathFilter _pathFilter;

        private string CurrentInputFileName { get; [Obsolete("This property is bind to _fileNameInput.Value")] set; }
        /// <summary>
        /// Current selected file full path
        /// </summary>
        private string? CurrentSelectedFilePath { get; [Obsolete("Use SelectItem() instead of setting this")] set; }
        /// <summary>
        /// Current exploring directory
        /// </summary>
        private string CurrentDirectory { get; [Obsolete("Use TryNavigateToDirectory() instead of setting this")] set; }


        private void Awake()
        {
            _fileItems = new PooledObjectListView<FileExplorerListItem>(
                UnityUtils.CreateObjectPool(() =>
                {
                    var item = Instantiate(_fileExplorerItemPrefab, _fileItemParentTransform);
                    item.Parent = this;
                    return item;
                }));

            Awake_Pinned();
        }

        private void Start()
        {
            _directoryInput.OnEndEdit.AddListener(path =>
            {
                if (TryNavigateToDirectory(path))
                    return;
                _directoryInput.SetValueWithoutNotify(CurrentDirectory);
            });
            _fileNameInput.OnValueChanged.AddListener(fileName =>
            {
                _confirmButton.IsInteractable = Utils.IsValidFileName(fileName);
#pragma warning disable CS0618
                CurrentInputFileName = fileName;
#pragma warning restore CS0618
            });
        }

        private void OnEnable()
        {
            RefreshWithCurrentDirectory();
#pragma warning disable CS0618
            CurrentSelectedFilePath = "";
            _fileNameInput.Value = "";
#pragma warning restore CS0618
        }

        private void OnDisable()
        {
            // We choose to clear pool because in most of time, we needn't use file explorer
            _fileItems.Clear(clearPool: true);
        }

        public bool TryNavigateToDirectory(string? directory, bool toParentDirIfNotExists = false)
        {
            if (toParentDirIfNotExists) {
                while (directory is not null && !Directory.Exists(directory))
                    directory = Path.GetDirectoryName(directory);
                if (directory is null)
                    return false;
                NavigateToDirectoryInternal(directory);
                return true;
            }
            else {
                if (Directory.Exists(directory)) {
                    NavigateToDirectoryInternal(directory);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Refresh file list in current directory
        /// </summary>
        /// <remarks>
        /// If current directory doesnt exist, try load its parent directory.
        /// <br/>
        /// if current directory is root, use <see cref="Directory.GetCurrentDirectory"/>
        /// </remarks>
        private void RefreshWithCurrentDirectory()
        {
            string? directory = CurrentDirectory;
            while (directory is not null && !TryNavigateToDirectory(directory)) {
                directory = Path.GetDirectoryName(directory);
            }

            if (directory is null) {
                NavigateToDirectoryInternal(Directory.GetCurrentDirectory());
            }
        }

        private void NavigateToDirectoryInternal(string directory)
        {
            Debug.Assert(Directory.Exists(directory));

            if (CurrentDirectory != directory) {
#pragma warning disable CS0618
                CurrentDirectory = directory;
#pragma warning restore CS0618
                _directoryInput.SetValueWithoutNotify(directory);
            }

            // Refresh files

            using (var resettingFileItems = _fileItems.Resetting()) {
                // If is not root directory, The first item is parent directory
                var parent = Path.GetDirectoryName(directory);
                if (parent is not null) {
                    resettingFileItems.Add(out var item);
                    item.Initialize(parent, true, displayText: "..");
                }

                foreach (var dir in Directory.GetDirectories(directory)) {
                    resettingFileItems.Add(out var item);
                    item.Initialize(dir, true);
                }

                switch (_pathFilter.Kind) {
                    case PathFilterKind.NoFilter: {
                        foreach (var file in Directory.GetFiles(directory)) {
                            resettingFileItems.Add(out var item);
                            item.Initialize(file, false);
                        }
                        break;
                    }
                    case PathFilterKind.FilterByExtensions: {
                        foreach (var file in Directory.GetFiles(directory)) {
                            if (file.EndsWithOneOf(_pathFilter.ExtensionFilters.AsSpan())) {
                                resettingFileItems.Add(out var item);
                                item.Initialize(file, false);
                            }
                        }
                        break;
                    }
                    case PathFilterKind.DirectoriesOnly:
                    default: break;
                }
            }
            _fileItems.SetSiblingIndicesInOrder();
        }

        internal void SelectItem(FileExplorerListItem item)
        {
            if (item.IsDirectory) {
                if (TryNavigateToDirectory(item.Path, toParentDirIfNotExists: true))
                    return;
                Debug.Assert(false, "Cannot find parent of current directory");
                throw new DirectoryNotFoundException($"Cannot find dir or parent dir of {item.Path}");
            }
            else {
                if (CurrentSelectedFilePath == item.Path)
                    return;
#pragma warning disable CS0618
                CurrentSelectedFilePath = item.Path;
#pragma warning restore CS0618
                _fileNameInput.Value = _extensionText.gameObject.activeSelf
                    ? Path.GetFileNameWithoutExtension(item.Path)
                    : Path.GetFileName(item.Path);
            }
        }

        public enum PathFilterKind
        {
            NoFilter,
            DirectoriesOnly,
            FilterByExtensions,
        }

        private readonly struct PathFilter
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

            public ImmutableArray<string> ExtensionFilters => _filters;

            private PathFilter(ImmutableArray<string> filters)
            {
                _filters = filters;
            }

            public static PathFilter NoFilter => new(ImmutableArray<string>.Empty);

            public static PathFilter DirectoriesOnly => default;

            public static PathFilter FilterByExtensions(ImmutableArray<string> extensionFilters)
                => extensionFilters.IsDefaultOrEmpty ? default : new(extensionFilters);
        }
    }
}