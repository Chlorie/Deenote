#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVDropdownProperty : KeyValueProperty
    {
        [SerializeField] Dropdown _dropdown;

        public Dropdown Dropdown => _dropdown;
    }
}