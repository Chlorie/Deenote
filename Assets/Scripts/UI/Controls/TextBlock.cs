#nullable enable

using Deenote.Localization;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class TextBlock : MonoBehaviour
    {
        [SerializeField] TMP_Text _tmpText = default!;
        [SerializeField] LocalizedText _localizedText = default!;
     
        public TMP_Text TmpText => _tmpText;

        public LocalizedText LocalizedText => _localizedText;
    }
}