#nullable enable

using UnityEngine;

namespace Deenote.UI
{
    [CreateAssetMenu(
        fileName = nameof(KnownIconsArgs),
        menuName = $"{nameof(Deenote)}/{nameof(UI)}/{nameof(KnownIconsArgs)}")]
    public sealed class KnownIconsArgs : ScriptableObject
    {
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