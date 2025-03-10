#nullable enable

using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.UI.Dialogs.Elements;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    partial class FileExplorerDialog
    {
        [Header("Pinned Directories")]
        [SerializeField] FileExplorerPinnedDirectoryListItem _pinnedItemPrefab = default!;
        [SerializeField] Transform _pinnedItemParentTransform = default!;
        internal PooledObjectListView<FileExplorerPinnedDirectoryListItem> _pinnedItems;

        private void Awake_Pinned()
        {
            _pinnedItems = new(UnityUtils.CreateObjectPool(_pinnedItemPrefab, _pinnedItemParentTransform,
                item => item.OnInstantiate(this), defaultCapacity: 0));

#if UNITY_EDITOR
            _pinnedItems.Add(out var item);
            item.Initialize(@"D:\Project Charts\Deenote\Music");
#endif
        }

        internal bool TryGetPinnedItem(string directory, [NotNullWhen(true)] out FileExplorerPinnedDirectoryListItem? item)
        {
            item = _pinnedItems.Find(directory, static (item, dir) => item.Directory == dir);
            return item is not null;
        }

        internal void UnpinDirectory(FileExplorerPinnedDirectoryListItem item)
        {
            var removed = _pinnedItems.Remove(item);
            Debug.Assert(removed == true);
        }

        internal void PinDirectory(string directoryPath)
        {
            Debug.Assert(System.IO.Directory.Exists(directoryPath));
            _pinnedItems.Add(out var pinned);
            pinned.Initialize(directoryPath);
        }
    }
}