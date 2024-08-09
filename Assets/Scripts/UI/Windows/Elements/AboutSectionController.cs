using Deenote.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class AboutSectionController : MonoBehaviour
    {
        [SerializeField] Toggle _toggleButton;
        [SerializeField] LocalizedText _buttonText;

        [SerializeField] private LocalizableText _sectionText;
        [SerializeField] private LocalizableText _contentText;

        private LocalizedText _contentTmpText;

        public LocalizableText SectionText => _sectionText;
        public LocalizableText ContentText => _contentText;

        public void OnCreated(LocalizedText contentText, ToggleGroup toggleGroup)
        {
            _contentTmpText = contentText;
            _toggleButton.group = toggleGroup;
        }


        public void Initialize(LocalizableText sectionText, LocalizableText contentText)
        {
            _sectionText = sectionText;
            _contentText = contentText;

            _buttonText.SetText(sectionText);
        }

        private void Awake()
        {
            _toggleButton.onValueChanged.AddListener(selected =>
            {
                if (selected) {
                    _contentTmpText.SetText(_contentText);
                }
            });
        }

        public void Select()
        {
            if (_toggleButton.isOn) {
                _contentTmpText.SetText(_contentText);
            }
            else {
                _toggleButton.isOn = true;
            }
        }
    }
}