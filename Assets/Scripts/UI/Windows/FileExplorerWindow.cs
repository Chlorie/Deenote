using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class FileExplorerWindow : MonoBehaviour
    {
        [SerializeField] private Window _window = null!;
        public Window Window => _window;

        [Header("UI")]
        [SerializeField] private Button _backDirButton = null!;
        /// <remarks>
        /// Keep text property sync with <see cref="__currentDirectory"/>
        /// </remarks>
        [SerializeField] private TMP_InputField _currentDirectoryInputField = null!;
        [SerializeField] private Button _confirmButton = null!;
        [SerializeField] private Button _cancelButton = null!;
        [SerializeField] private TMP_InputField _fileNameInputField = null!;
        [SerializeField] private TMP_Text _fileExtensionText = null!;
        [SerializeField] private Transform _fileItemParentTransform = null!;

        [Header("Prefabs")]
        [SerializeField] private FileExplorerListItemController _fileItemPrefab = null!;

        private PooledObjectListView<FileExplorerListItemController> _fileItems;

        // null: filter directory
        // []: no filter
        private string[]? _extensionFilters;
        private string? _selectedFilePath;

        private string? __currentDirectory;

        private string? CurrentDirectory
        {
            get => __currentDirectory;
            set => _currentDirectoryInputField.text = (__currentDirectory = value) ?? "";
        }

        private void ResetFileList(string? directory)
        {
            if (directory is not null)
                CurrentDirectory = directory;
            else
                CurrentDirectory ??= Directory.GetCurrentDirectory();

            var resettingFileItems = _fileItems.Resetting();

            foreach (var dir in Directory.GetDirectories(CurrentDirectory)) {
                resettingFileItems.Add(out var item);
                item.Initialize(dir, true);
            }

            if (_extensionFilters is null) {
                resettingFileItems.Dispose();
                _fileItems.SetSiblingIndicesInOrder();
                return;
            }

            if (_extensionFilters.Length == 0) {
                foreach (var file in Directory.GetFiles(CurrentDirectory)) {
                    resettingFileItems.Add(out var item);
                    item.Initialize(file, false);
                }
            }
            else {
                foreach (var file in Directory.GetFiles(CurrentDirectory)) {
                    if (file.EndsWithOneOf(_extensionFilters)) {
                        resettingFileItems.Add(out var item);
                        item.Initialize(file, false);
                    }
                }
            }

            resettingFileItems.Dispose();
            _fileItems.SetSiblingIndicesInOrder();
        }

        private void Awake()
        {
            _backDirButton.onClick.AddListener(() => ResetFileList(Path.GetDirectoryName(CurrentDirectory)));
            _currentDirectoryInputField.onEndEdit.AddListener(path =>
            {
                if (Directory.Exists(path)) {
                    ResetFileList(path);
                }
                else {
                    _currentDirectoryInputField.text = CurrentDirectory ?? "";
                }
            });
            _fileNameInputField.onValueChanged.AddListener(fileName =>
            {
                // Set by selection
                if (!_fileNameInputField.IsInteractable())
                    return;

                _confirmButton.interactable = Utils.IsValidFileName(fileName);
            });

            _fileItems = new PooledObjectListView<FileExplorerListItemController>(UnityUtils.CreateObjectPool(() =>
            {
                var item = Instantiate(_fileItemPrefab, _fileItemParentTransform);
                item.OnCreated(this);
                return item;
            }, 0));
        }

        private void OnDisable()
        {
            // Clear file items when close FileExplorer
            _fileItems.Clear(clearPool: true);
        }

        internal void SelectItem(FileExplorerListItemController fileItem)
        {
            if (fileItem.IsDirectory) {
                ResetFileList(fileItem.Path);
            }
            else {
                _selectedFilePath = fileItem.Path;
                _fileNameInputField.text = fileItem.FileName;
                _confirmButton.interactable = true;
            }
        }
    }
}