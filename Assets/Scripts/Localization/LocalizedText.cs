using TMPro;
using UnityEngine;

namespace Deenote.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] TMP_Text _text;
        [SerializeField] string _textKey;

        private bool _isLocalized;

        public TMP_Text TmpText => _text;

        private void Awake()
        {
            MainSystem.Localization.RegisterLocalizedText(this);
        }

        private void OnDestroy()
        {
            MainSystem.Localization.UnregisterLocalizedText(this);
        }

        public void NotifyLanguageUpdated()
        {
            if (_isLocalized) {
                if (_textKey == null)
                    _text.text = "";
                else
                    _text.text = MainSystem.Localization.GetLocalizedText(_textKey);
            }
        }

        public void SetRawText(string text)
        {
            _isLocalized = false;
            _text.text = text;
        }

        public void SetLocalizedText(string textKey)
        {
            if (_isLocalized && _textKey == textKey) {
                return;
            }

            _textKey = textKey;
            _isLocalized = true;
            NotifyLanguageUpdated();
        }

        public void SetText(LocalizableText text)
        {
            if (text.IsLocalized) {
                SetLocalizedText(text.TextOrKey);
            }
            else {
                SetRawText(text.TextOrKey);
            }
        }
    }
}
