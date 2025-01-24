#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UIFramework.Controls;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Dialogs.Elements
{
    public sealed class FileExplorerPinnedDirectoryListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Button _button = default!;
        [SerializeField] Button _unpinButton = default!;

        private bool _isHovering;

        public string Directory { get; private set; } = default!;

        public FileExplorerDialog Dialog { get; private set; } = default!;

        private static readonly MessageBoxArgs _dirNotFoundMsgBoxArgs = new(
            LocalizableText.Localized("DirNotFound_MsgBox_Title"),
            LocalizableText.Localized("PinDirNotFound_MsgBox_Content"),
            LocalizableText.Localized("PinDirNotFound_MsgBox_Y"),
            LocalizableText.Localized("PinDirNotFound_MsgBox_N"));

        private void Awake()
        {
            _unpinButton.Image.sprite = MainWindow.Args.UIIcons.FileExplorerUnpinSprite;
        }

        private void Start()
        {
            _button.Clicked += UniTask.Action(async () =>
            {
                if (Dialog.TryNavigateToDirectory(Directory))
                    return;
                var res = await MainWindow.MessageBox.OpenAsync(_dirNotFoundMsgBoxArgs);
                if (res != 0)
                    return;
                Dialog.UnpinDirectory(this);
            });
            _unpinButton.Clicked += () => Dialog.UnpinDirectory(this);
        }

        internal void OnInstantiate(FileExplorerDialog dialog)
        {
            Dialog = dialog;
        }

        internal void Initialize(string directory)
        {
            Directory = directory;
            _button.Text.SetRawText(Path.GetFileName(directory));

            _isHovering = false;
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
            _unpinButton.gameObject.SetActive(_isHovering);
        }
    }
}