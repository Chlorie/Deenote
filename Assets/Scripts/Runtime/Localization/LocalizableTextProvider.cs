#nullable enable

using UnityEngine;

namespace Deenote.Localization
{
    [DisallowMultipleComponent]
    public sealed class LocalizableTextProvider : MonoBehaviour
    {
        [SerializeField] LocalizableText _text;

        public LocalizableText Text
        {
            get => _text;
            set => _text = value;
        }
    }
}