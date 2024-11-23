#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.Utilities;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Dialogs.Elements
{
    public sealed partial class FileExplorerPinnedDirListItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;
        [SerializeField] Button _unpinButton = default!;

        public FileExplorerDialog Parent { get; internal set; } = default!;

        public string Directory { get; private set; } = default!; // Init in Initialize()

        private readonly MessageBoxArgs _dirNotFoundMsgBoxArgs = new(
            LocalizableText.Localized("DirNotFound_MsgBox_Title"),
            LocalizableText.Localized("PinDirNotFound_MsgBox_Content"),
            LocalizableText.Localized("PinDirNotFound_MsgBox_Y"),
            LocalizableText.Localized("PinDirNotFound_MsgBox_N"));

        private void Awake()
        {
            _unpinButton.Image.sprite = MainSystem.Args.KnownIconsArgs.FileExplorerUnpinSprite;
        }

        private void Start()
        {
            _button.OnClick.AddListener(async UniTaskVoid () =>
            {
                if (Parent.TryNavigateToDirectory(Directory))
                    return;
                var res = await MainSystem.MessageBoxDialog.OpenAsync(_dirNotFoundMsgBoxArgs);
                if (res != 0)
                    return;
                Parent.UnpinDirectory(this);
            });
            _unpinButton.OnClick.AddListener(() => Parent.UnpinDirectory(this));
        }

        internal void Initialize(string directory)
        {
            Directory = directory;
            _button.Text.SetRawText(Path.GetFileName(directory));
        }
    }
}