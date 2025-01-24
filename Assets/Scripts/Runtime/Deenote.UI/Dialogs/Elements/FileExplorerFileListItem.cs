#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Dialogs.Elements
{
    public sealed class FileExplorerFileListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Button _button = default!;
        [SerializeField] Button _pinButton = default!;

        private FileExplorerPinnedDirectoryListItem? _pinnedItem;
        private bool _isHovering;

        public string Path { get; private set; } = default!;
        public bool IsDirectory { get; private set; }

        public FileExplorerDialog Dialog { get; private set; } = default!;

        private void Awake()
        {
            _button.Clicked += () => Dialog.SelectFileItem(this);
            _pinButton.Clicked += () =>
            {
                if (_pinnedItem is not null) {
                    Dialog.UnpinDirectory(_pinnedItem);
                }
                else {
                    Dialog.PinDirectory(Path);
                }
            };
        }

        internal void OnInstantiate(FileExplorerDialog dialog)
        {
            Dialog = dialog;
        }

        internal void Initialize(string path, bool isDirectory)
        {
            Path = path;
            IsDirectory = isDirectory;

            _button.Text.SetRawText(System.IO.Path.GetFileName(path));
            _button.Image.gameObject.SetActive(isDirectory);

            _isHovering = false;
            _pinnedItem = null;
            DoVisualTransition();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            DoVisualTransition();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            DoVisualTransition();
        }

        private void DoVisualTransition()
        {
            if (_isHovering && IsDirectory) {
                _pinButton.gameObject.SetActive(true);
                if (Dialog.TryGetPinnedItem(Path, out var item)) {
                    _pinButton.Image.sprite = MainWindow.Args.UIIcons.FileExplorerPinSprite;
                }
                else {
                    _pinButton.Image.sprite = MainWindow.Args.UIIcons.FileExplorerUnpinSprite;
                }
                _pinnedItem = item;
            }
            else {
                _pinButton.gameObject.SetActive(false);
            }
        }
    }
}