using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVBooleanProperty : KeyValueProperty
    {
        [SerializeField] CheckBox _checkBox = default!;

        public CheckBox CheckBox => _checkBox;
    }
}