#nullable enable

using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    partial class FileExplorerDialog
    {
        [Header("Pinned Directories")]
        [SerializeField] FileExplorerPinnedDirListItem _pinnedDirListItemPrefab;
        [SerializeField] Transform _pinnedDirParentTransform;
        private PooledObjectListView<FileExplorerPinnedDirListItem> _pinnedDirs;

        private void Awake_Pinned()
        {
            _pinnedDirs = new(UnityUtils.CreateObjectPool(
                () =>
                {
                    var item = Instantiate(_pinnedDirListItemPrefab, _pinnedDirParentTransform);
                    item.Parent = this;
                    return item;
                }, defaultCapacity: 0));

#if UNITY_EDITOR
            _pinnedDirs.Add(out var item);
            item.Initialize(@"D:\Project Charts\Deenote\Music");
#endif
        }

        internal FileExplorerPinnedDirListItem? TryGetPinnedDirItem(string directory)
        {
            return _pinnedDirs.Find(directory, static (item, dir) => item.Directory == dir);
        }

        internal void UnpinDirectory(FileExplorerPinnedDirListItem item)
        {
            var removed = _pinnedDirs.Remove(item);
            Debug.Assert(removed == true);
        }

        internal void PinDirectory(FileExplorerListItem item)
        {
            if (!item.IsDirectory)
                throw new System.InvalidOperationException("Cannot pin a file");
            _pinnedDirs.Add(out var pinned);
            pinned.Initialize(item.Path);
        }
    }
}