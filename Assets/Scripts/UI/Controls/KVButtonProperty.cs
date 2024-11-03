#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class KVButtonProperty : KeyValueProperty
    {
        [SerializeField] Button _button;

        public Button Button => _button;
    }
}