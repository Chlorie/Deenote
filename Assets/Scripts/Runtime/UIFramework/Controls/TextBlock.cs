#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using Deenote.Localization;

namespace Deenote.UIFramework.Controls
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public sealed class TextBlock : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _tmpText = default!;
        [SerializeField] LocalizableText _localizableText;
        [SerializeField] bool _useCustomFont;

        private List<string>? _localizationArgs;

        public TMP_Text TmpText => _tmpText;

        public string Text => _tmpText.text;

        public void SetText(LocalizableText text, ReadOnlySpan<string> args = default)
        {
            bool valueChanged = false;
            if (_localizableText != text) {
                _localizableText = text;
                valueChanged = true;
            }
            var oldArgs = _localizationArgs is null ? ReadOnlySpan<string>.Empty : _localizationArgs.AsSpan();
            if (!args.SequenceEqual(oldArgs)) {
                if (_localizationArgs is null)
                    _localizationArgs = new List<string>(args.Length);
                else
                    _localizationArgs.Clear();
                _localizationArgs.AddRange(args);
                valueChanged = true;
            }

            if (valueChanged)
                RefreshDisplayText();
        }

        public void SetText(ArgedLocalizableText text)
            => SetText(text.LocalizableText, text.Args);

        public void SetLocalizedText(string key, ReadOnlySpan<string> args = default)
            => SetText(LocalizableText.Localized(key), args);

        public void SetLocalizedText(string key, string arg0)
            => SetLocalizedText(key, MemoryMarshal.CreateReadOnlySpan(ref arg0, 1));

        public void SetRawText([AllowNull] string text) => SetText(LocalizableText.Raw(text ?? ""));

        private void Awake()
        {
            _tmpText.font = UISystem.FontAsset;
            LocalizationSystem.LanguageChanged += _LanguageChanged;
        }

        private void Start()
        {
            _LanguageChanged(LocalizationSystem.CurrentLanguage);
        }

        private void OnDestroy()
        {
            LocalizationSystem.LanguageChanged -= _LanguageChanged;
        }

        private void _LanguageChanged(LanguagePack lang) => RefreshDisplayText();

        private void RefreshDisplayText()
        {
            string text = LocalizationSystem.GetText(_localizableText);
            if (_localizationArgs?.Count > 0) {
                var sb = new StringBuilder(text);
                for (int i = 0; i < _localizationArgs.Count; i++) {
                    sb.Replace($"{{{i}}}", _localizationArgs[i]);
                }
                _tmpText.text = sb.ToString();
            }
            else {
                _tmpText.text = text;
            }
        }

        private void OnValidate()
        {
            _tmpText ??= GetComponent<TextMeshProUGUI>();
        }
    }
}