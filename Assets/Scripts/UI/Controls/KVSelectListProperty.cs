using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVSelectListProperty : KeyValueProperty
    {
        [SerializeField] ToggleList _toggleList = default!;

        public ToggleList ToggleList => _toggleList;
    }
}