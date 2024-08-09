using Deenote.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Deenote.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text = null!;
        [SerializeField] private string? _textKey;
        [SerializeField] private bool _isLocalized;

        private List<string> _args = new();

        public TMP_Text TmpText => _text;

        public event Action<LocalizedText> OnTextUpdated;

        // I have to do this because there's a huge amount of assigned components in editor...
        private LocalizableText LocalizableText
        {
            get => _isLocalized ? Localization.LocalizableText.Localized(_textKey) : Localization.LocalizableText.Raw(_textKey);
            set => (_isLocalized, _textKey) = (value.IsLocalized, value.TextOrKey);
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
        {
            SetText(LocalizableText.Raw(text));
        }

        public void SetLocalizedText(string textKey, ReadOnlySpan<string> args = default)
        {
            SetText(LocalizableText.Localized(textKey), args);
        }

        public void SetText(LocalizableText text, ReadOnlySpan<string> args = default)
        {
            if (LocalizableText == text && args.SequenceEqual(_args))
                return;

            LocalizableText = text;
            _args.Clear();
            foreach (var arg in args)
                _args.Add(arg);

            RefreshText();
        }

        private void RefreshText()
        {
            string text = MainSystem.Localization.GetText(LocalizableText);

            for (int i = 0; i < _args.Count; i++) {
                text = text.Replace($"{{{i}}}", _args[i]);
            }
            _text.text = text;

            OnTextUpdated?.Invoke(this);
        }

        public void NotifyLanguageUpdated()
        {
            RefreshText();
        }
    }
}
