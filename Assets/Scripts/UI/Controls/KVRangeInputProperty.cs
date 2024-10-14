using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVRangeInputProperty : KeyValueProperty
    {
        [SerializeField] InputField _lowerInput = default!;
        [SerializeField] InputField _upperInput = default!;

        public InputField LowerInputField => _lowerInput;
        public InputField UpperInputField => _upperInput;
    }
}