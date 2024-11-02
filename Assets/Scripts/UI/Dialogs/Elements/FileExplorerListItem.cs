using Deenote.UI.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Dialogs.Elements
{
    public sealed class FileExplorerListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Button _button = default!;
        [SerializeField] Button _pinButton = default!;

        private bool _isDirectory;
        private string _path = default!;
        private FileExplorerPinnedDirListItem? _pinnedItem;

        public bool IsDirectory => _isDirectory;
        public string Path => _path;

        public FileExplorerDialog Parent { get; internal set; } = default!;

        private void Start()
        {
            _button.OnClick.AddListener(() => Parent.SelectItem(this));
            _pinButton.OnClick.AddListener(() =>
            {
                if (_pinnedItem is not null) {
                    Parent.UnpinDirectory(_pinnedItem);
                }
                else {
                    Parent.PinDirectory(this);
                }
            });
        }

        internal void Initialize(string path, bool isDirectory, string? displayText = null)
        {
            _isDirectory = isDirectory;
            _path = path;

            _button.Text.SetRawText(displayText ?? System.IO.Path.GetFileName(path));
            _button.Image.sprite = isDirectory
                ? MainSystem.Args.KnownIconsArgs.FileListItemFolderSprite
                : MainSystem.Args.KnownIconsArgs.FileListItemFileSprite;

            _pinButton.gameObject.SetActive(false);
            _pinnedItem = null;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_isDirectory) {
                _pinButton.gameObject.SetActive(true);
                var pinnedItem = Parent.TryGetPinnedDirItem(Path);
                if (pinnedItem is null) {
                    _pinButton.Image.sprite = MainSystem.Args.KnownIconsArgs.FileExplorerPinSprite;
                }
                else {
                    _pinButton.Image.sprite = MainSystem.Args.KnownIconsArgs.FileExplorerUnpinSprite;
                }
                _pinnedItem = pinnedItem;
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (_isDirectory) {
                _pinButton.gameObject.SetActive(false);
            }
        }
    }
}