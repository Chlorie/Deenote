using Deenote.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class MessageBoxButtonController : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] LocalizedText _text;

        public Button Button => _button;

        public void Initialize(LocalizableText text)
        {
            _text.SetText(text);
        }
    }
}