using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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

        private string? __currentSelectedPath;
        /// <summary>
        /// Current selected full file path
        /// </summary>
        private string? CurrentSelectedPath
        {
            get => __currentSelectedPath;
            set {
                if (__currentSelectedPath == value)
                    return;
                __currentSelectedPath = value;
                _fileNameInput.Value = value;
            }
        }

        private string __currentDirectory = default!;
        /// <summary>
        /// Current exploring directory
        /// </summary>
        /// <remarks>
        /// Requires call <see cref="RefreshFileList"/>
        /// </remarks>
        private string CurrentDirectory
        {
            get => __currentDirectory;
            set {
                if (__currentDirectory == value)
                    return;
                __currentDirectory = value;
                _directoryInput.SetValueWithoutNotify(value);
            }
        }

        private void Awake()
        {
            _fileItems = new PooledObjectListView<FileExplorerListItem>(
                UnityUtils.CreateObjectPool(() =>
                {
                    var item = Instantiate(_fileExplorerItemPrefab, _fileItemParentTransform);
                    item.Parent = this;
                    return item;
                }));
            CurrentDirectory = Directory.GetCurrentDirectory();
        }

        private void Start()
        {
            _directoryInput.OnEndEdit.AddListener(path =>
            {
                if (Directory.Exists(path)) {
                    CurrentDirectory = path;
                    RefreshFileList();
                }
                else
                    _directoryInput.SetValueWithoutNotify(CurrentDirectory);
            });
            _fileNameInput.OnValueChanged.AddListener(fileName =>
            {
                _confirmButton.IsInteractable = Utils.IsValidFileName(fileName);
            });
        }

        private void OnDisable()
        {
            // We choose to clear pool because in most of time, we needn't use file explorer
            _fileItems.Clear(clearPool: true);
        }

        private void RefreshFileList()
        {
            var directory = CurrentDirectory;

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
                if (item.Path == "..")
                    CurrentDirectory = Path.GetDirectoryName(item.Path);
                else
                    CurrentDirectory = item.Path;
                RefreshFileList();
            }
            else {
                CurrentSelectedPath = Path.GetFileName(item.Path);
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
                        return PathFilterKind.NoFilter;
                    if (_filters.IsEmpty)
                        return PathFilterKind.DirectoriesOnly;
                    return PathFilterKind.FilterByExtensions;
                }
            }

            public ImmutableArray<string> ExtensionFilters => _filters;

            private PathFilter(ImmutableArray<string> filters)
            {
                _filters = filters;
            }

            public static PathFilter NoFilter => default;

            public static PathFilter DirectoriesOnly => new(ImmutableArray<string>.Empty);

            public static PathFilter FilterByExtensions(ImmutableArray<string> extensionFilters)
                => extensionFilters.IsDefaultOrEmpty ? default : new(extensionFilters);
        }
    }
}