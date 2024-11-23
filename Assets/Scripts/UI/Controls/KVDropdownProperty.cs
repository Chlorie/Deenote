#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVDropdownProperty : KeyValueProperty
    {
        [SerializeField] Dropdown _dropdown = default!;

        public Dropdown Dropdown => _dropdown;
    }
}