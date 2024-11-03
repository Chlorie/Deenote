#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVInputProperty : KeyValueProperty
    {
        [SerializeField] InputField _valueInput = default!;

        public InputField InputField => _valueInput;
    }
}