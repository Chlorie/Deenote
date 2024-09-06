using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Deenote.Localization
{
    public sealed class LocalizationSystem
    {
        public ListReadOnlyView<string> Languages => _languages;

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set {
                if (_currentLanguage == value) return;
                _currentLanguagePack = GetLanguagePack(value);
                _currentLanguage = value;
                OnLanguageChanged?.Invoke(value);
            }
        }

        public event Action<string>? OnLanguageChanged;

        public LocalizationSystem()
        {
            var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
            var files = Directory.GetFiles(folder);
            foreach (var file in files) {
                if (!file.EndsWith(".txt")) continue;
                LoadLanguagePackFile(file);
            }
            if (_languagePacks.TryAdd(DefaultLanguageCode,
                    new LanguagePack(DefaultLanguageName, new Dictionary<string, string>())))
                Debug.LogWarning("Default language translation file is not found.");
            _currentLanguage = DefaultLanguageCode;
            _currentLanguagePack = _defaultLanguagePack = _languagePacks[DefaultLanguageCode];
        }

        public string GetLanguageDisplayName(string languageCode) =>
            GetLanguagePack(languageCode).LanguageDisplayName;

        public string GetText(LocalizableText text) =>
            !text.IsLocalized ? text.TextOrKey
                : _currentLanguagePack.TryGetTranslation(text.TextOrKey) ??
                  _defaultLanguagePack.TryGetTranslation(text.TextOrKey) ??
                  text.TextOrKey;

        private string _currentLanguage;
        private LanguagePack _currentLanguagePack;
        private LanguagePack _defaultLanguagePack;

        private record LanguagePack(string LanguageDisplayName, Dictionary<string, string> Translations)
        {
            public string? TryGetTranslation(string key) => Translations.GetValueOrDefault(key);
        }

        private const string DefaultLanguageCode = "en";
        private const string DefaultLanguageName = "English";
        private readonly Dictionary<string, LanguagePack> _languagePacks = new();
        private readonly List<string> _languages = new();

        private void LoadLanguagePackFile(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            using StreamReader reader = new(fs);
            var languageCode = reader.ReadLine()!;
            var name = reader.ReadLine()!;
            var existed = _languagePacks.GetValueOrAdd(languageCode,
                () => new LanguagePack(name, new Dictionary<string, string>()), out var pack);
            if (!existed) _languages.Add(languageCode);

            while (!reader.EndOfStream && reader.ReadLine() is { } line) {
                if (line.StartsWith('#') || // Ignore comments
                    (line.IndexOf('=') is var separator && separator < 0)) // Ignore lines without '='
                    continue;
                string key = line[..separator], value = line[(separator + 1)..];
                if (value == "\"\"\"") { // Multiline text
                    StringBuilder sb = new();
                    while (!reader.EndOfStream && reader.ReadLine() is { } line2 and not "\"\"\"")
                        sb.AppendLine(line2);
                    value = sb.ToString();
                }
                else
                    value = value.Replace("<br/>", "\n");
                if (!pack.Translations.TryAdd(key, value))
                    Debug.LogWarning($"Language pack {name} contains duplicated key: {key}");
            }
        }

        private LanguagePack GetLanguagePack(string languageCode)
        {
            if (!_languagePacks.TryGetValue(languageCode, out var res))
                throw new KeyNotFoundException($"Translations for {languageCode} is not found.");
            return res;
        }
    }
}