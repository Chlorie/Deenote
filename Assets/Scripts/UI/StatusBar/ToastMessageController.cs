using Deenote.Localization;
using UnityEngine;

namespace Deenote.UI.StatusBar
{
    public sealed class ToastMessageController : MonoBehaviour
    {
        [SerializeField] LocalizedText _text;

        public void Initialize(LocalizableText text)
        {
            _text.SetText(text);
        }
    }
}