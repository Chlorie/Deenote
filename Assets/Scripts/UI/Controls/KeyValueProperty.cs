#nullable enable

using Deenote.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public abstract class KeyValueProperty : MonoBehaviour
    {
        [SerializeField] Image _keyImage = default!; // May be inactive
        [SerializeField] LocalizedText _keyText=default!; // May be inactive
    }
}