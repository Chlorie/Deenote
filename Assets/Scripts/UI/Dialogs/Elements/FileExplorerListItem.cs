using Deenote.UI.Controls;
using UnityEngine;

namespace Deenote.UI.Dialogs.Elements
{
    public sealed class FileExplorerListItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;

        private bool _isDirectory;
        private string _path = default!;

        public bool IsDirectory => _isDirectory;
        public string Path => _path;

        public FileExplorerDialog Parent { get; internal set; } = default!;

        private void Start()
        {
            _button.OnClick.AddListener(() => Parent.SelectItem(this));
        }

        internal void Initialize(string path, bool isDirectory, string? displayText = null)
        {
            _isDirectory = isDirectory;
            _path = path;

            _button.Text.Text.text = displayText ?? System.IO.Path.GetFileName(path);
            _button.Image.sprite = isDirectory
                ? MainSystem.Args.KnownIconsArgs.FileListItemFolderSprite
                : MainSystem.Args.KnownIconsArgs.FileListItemFileSprite;
        }
    }
}