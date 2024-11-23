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
        public Sprite FileListItemFolderSprite = default!;
        public Sprite FileListItemFileSprite = default!;
        public Sprite FileExplorerPinSprite = default!;
        public Sprite FileExplorerUnpinSprite = default!;
        [Header("Note Infos")]
        public Sprite NoteInfoSoundsEditSprite = default!;
        public Sprite NoteInfoSoundsAcceptSprite = default!;
        public Sprite NoteInfoSoundsCollapseSprite = default!;
    }
}