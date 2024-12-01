#nullable enable

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。

using UnityEngine;

namespace Deenote.UI
{
    [CreateAssetMenu(
        fileName = nameof(UIIcons),
        menuName = $"{nameof(Deenote)}/{nameof(UI)}/{nameof(UIIcons)}")]
    public sealed class UIIcons : ScriptableObject
    {
        [Header("CheckBox")]
        public Sprite CheckBoxChecked;
        public Sprite CheckBoxIndeterminate;
        // Considering should i move these into their owning classes..
        [Header("FileListItem")]
        public Sprite FileListItemFolderSprite;
        public Sprite FileListItemFileSprite;
        public Sprite FileExplorerPinSprite;
        public Sprite FileExplorerUnpinSprite;
        [Header("Note Infos")]
        public Sprite NoteInfoSoundsEditSprite;
        public Sprite NoteInfoSoundsAcceptSprite;
        public Sprite NoteInfoSoundsCollapseSprite;
    }
}