using Cysharp.Threading.Tasks;
using Deenote.UI.Windows;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class FileExplorerWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        [Header("UI")]
        [SerializeField] Button _backDirButton;
        /// <remark>
        /// Keep text property sync with <see cref="__currentDirectory"/>
        /// </remark>
        [SerializeField] TMP_InputField _currentDirectoryInputField;
        [SerializeField] Button _confirmButton;
        [SerializeField] Button _cancelButton;
        [SerializeField] TMP_InputField _fileNameInputField;
        [SerializeField] TMP_Text _fileExtensionText;
        [SerializeField] Transform _fileItemParentTransform;

        [Header("Prefabs")]
        [SerializeField] FileExplorerListItemController _fileItemPrefab;

        private ObjectPool<FileExplorerListItemController> _fileItemPool;
        private List<FileExplorerListItemController> _fileItems;

        private string[] _extensionFilters;
        private string _selectedFilePath;

        private string __currentDirectory;
        private string CurrentDirectory
        {
            get => __currentDirectory;
            set => _currentDirectoryInputField.text = __currentDirectory = value;
        }

        private void ResetFileList(string directory)
        {
            if (directory is not null)
                CurrentDirectory = directory;
            else
                CurrentDirectory ??= Directory.GetCurrentDirectory();

            foreach (var item in _fileItems)
                ReleaseFileItem(item);
            _fileItems.Clear();

            int i = 0;
            foreach (var dir in Directory.GetDirectories(CurrentDirectory)) {
                var item = GetFileItem(dir, true);
                item.transform.SetSiblingIndex(i++);
                _fileItems.Add(item);
            }

            if (_extensionFilters is null)
                return;

            if (_extensionFilters.Length == 0) {
                foreach (var file in Directory.GetFiles(CurrentDirectory)) {
                    var item = GetFileItem(file, false);
                    item.transform.SetSiblingIndex(i++);
                    _fileItems.Add(item);
                }
            }
            else {
                foreach (var file in Directory.GetFiles(CurrentDirectory)) {
                    if (file.EndsWithOneOf(_extensionFilters)) {
                        var item = GetFileItem(file, false);
                        item.transform.SetSiblingIndex(i++);
                        _fileItems.Add(item);
                    }
                }
            }
        }

        private void Awake()
        {
            _backDirButton.onClick.AddListener(() => ResetFileList(Path.GetDirectoryName(CurrentDirectory)));
            _currentDirectoryInputField.onSubmit.AddListener(path =>
            {
                if (Directory.Exists(path)) {
                    ResetFileList(path);
                }
                else {
                    _currentDirectoryInputField.text = CurrentDirectory;
                }
            });
            _fileNameInputField.onValueChanged.AddListener(fileName =>
            {
                // Set by selection
                if (!_fileNameInputField.IsInteractable())
                    return;

                _confirmButton.interactable = Utils.IsValidFileName(fileName);
            });

            _fileItemPool = UnityUtils.CreateObjectPool(_fileItemPrefab, _fileItemParentTransform);
            _fileItems = new();
        }

        private void OnDisable()
        {
            // Clear file items when close FileExplorer
            foreach (var item in _fileItems)
                ReleaseFileItem(item);
            _fileItems.Clear();
            _fileItemPool.Clear();
        }

        #region Pool

        private FileExplorerListItemController GetFileItem(string path, bool isDirectory)
        {
            var item = _fileItemPool.Get();
            item.Initialize(this, path, isDirectory);
            return item;
        }

        private void ReleaseFileItem(FileExplorerListItemController fileItem)
        {
            _fileItemPool.Release(fileItem);
        }

        #endregion

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