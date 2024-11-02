using Deenote.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;

namespace Deenote.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private LocalizableText _localizableText;
        private List<string> _args = new();

        public TMP_Text Text => this.MaybeGetComponent(ref _text);
        private TMP_Text? _text;

        public event Action<LocalizedText>? OnTextUpdated;

        // I have to do this because there's a huge amount of assigned components in editor...
        private LocalizableText LocalizableText
        {
            get => _localizableText;//_isLocalized ? LocalizableText.Localized(_textKey) : LocalizableText.Raw(_textKey);
            set => _localizableText = value;//(_isLocalized, _textKey) = (value.IsLocalized, value.TextOrKey);
        }

        private void Awake()
        {
            MainSystem.Localization.OnLanguageChanged += NotifyLanguageUpdated;
        }

        private void Start()
        {
            RefreshText();
        }

        private void OnDestroy()
        {
            MainSystem.Localization.OnLanguageChanged -= NotifyLanguageUpdated;
        }

        public void SetRawText(string text)
            => SetText(LocalizableText.Raw(text));

        public void SetLocalizedText(string textKey, string arg0)
            => SetText(LocalizableText.Localized(textKey), MemoryMarshal.CreateReadOnlySpan(ref arg0, 1));

        public void SetLocalizedText(string textKey)
            => SetText(LocalizableText.Localized(textKey), default);
        
        public void SetText(LocalizableText text, ReadOnlySpan<string> args = default)
        {
            bool valueChanged = false;
            if (LocalizableText != text) {
                LocalizableText = text;
                valueChanged = true;
            }
            if (!args.SequenceEqual(_args.AsSpan())) {
                _args.Clear();
                _args.AddRange(args);
                valueChanged = true;
            }

            if (valueChanged)
                RefreshText();
        }

        private void RefreshText()
        {
            string text = MainSystem.Localization.GetText(LocalizableText);
            if (_args.Count == 0) {
                Text.text = text;
            }
            else {
                var sb = new StringBuilder(text);
                for (int i = 0; i < _args.Count; i++) {
                    sb.Replace($"{{{i}}}", _args[i]);
                }
                Text.text = sb.ToString();
            }

            OnTextUpdated?.Invoke(this);
        }

        private void NotifyLanguageUpdated(string _) => RefreshText();
    }
}