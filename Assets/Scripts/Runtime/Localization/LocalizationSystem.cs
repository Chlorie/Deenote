#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Deenote.Localization
{
    public static class LocalizationSystem
    {
        public static ReadOnlySpan<string> Languages => _languages.AsSpan();

        public static string CurrentLanguage
        {
            get => _currentLanguagePack.LanguageDisplayName;
            set {
                if (_currentLanguagePack.LanguageDisplayName == value)
                    return;
                _currentLanguagePack = GetLanguagePack(value);
                LanguageChanged?.Invoke(value);
            }
        }

        public static event Action<string>? LanguageChanged;

        static LocalizationSystem()
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
            _currentLanguagePack = _defaultLanguagePack = _languagePacks[DefaultLanguageCode];
        }

        public static string GetLanguageDisplayName(string languageCode) =>
            GetLanguagePack(languageCode).LanguageDisplayName;

        public static string GetText(LocalizableText text) =>
            !text.IsLocalized ? text.TextOrKey
                : _currentLanguagePack.TryGetTranslation(text.TextOrKey) ??
                  _defaultLanguagePack.TryGetTranslation(text.TextOrKey) ??
                  text.TextOrKey;

        private static LanguagePack _currentLanguagePack;
        private static LanguagePack _defaultLanguagePack;

        private record LanguagePack(string LanguageDisplayName, Dictionary<string, string> Translations)
        {
            public string? TryGetTranslation(string key) => Translations.GetValueOrDefault(key);
        }

        private const string DefaultLanguageCode = "en";
        private const string DefaultLanguageName = "English";
        private static readonly Dictionary<string, LanguagePack> _languagePacks = new();
        private static readonly List<string> _languages = new();

        private static void LoadLanguagePackFile(string filePath)
        {
            using StreamReader reader = File.OpenText(filePath);
            string? languageCode = reader.ReadLine();
            if (languageCode is null) return;

            string? name = reader.ReadLine();
            if (name is null) return;

            var existed = _languagePacks.GetValueOrAdd(languageCode,
                () => new LanguagePack(name, new Dictionary<string, string>()), out var pack);
            if (!existed) _languages.Add(name);

            while (reader.ReadLine() is { } line) {
                if (line.StartsWith('#') || // Ignore comments
                    line.IndexOf('=') is var separator && separator < 0) // Ignore lines without '='
                    continue;

                string value;
                ReadOnlySpan<char> firstLineValueSpan = line.AsSpan(separator + 1);
                if (firstLineValueSpan.SequenceEqual("\"\"\"")) // Multiline text
                    value = ReadMultilineText();
                else
                    value = firstLineValueSpan.ToString().Replace("<br/>", "\n");

                string key = line[..separator];
                if (!pack.Translations.TryAdd(key, value))
                    Debug.LogWarning($"Language pack {name} contains duplicated key: {key}");
            }

            string ReadMultilineText()
            {
                var lines = new List<string>();
                int skipCount = 0;
                while (reader.ReadLine() is { } line) {
                    if (line.EndsWith("\"\"\"") && line.AsSpan()[..^3].IsWhiteSpace()) {
                        skipCount = line.Length - 3;
                        break;
                    }
                    lines.Add(line);
                }

                var sb = new StringBuilder();
                foreach (var line in lines) {
                    int actualSkipCount = GetLeadingSpaceCount(line, skipCount);
                    sb.AppendLine(line[actualSkipCount..]);
                }
                return sb.ToString();

                static int GetLeadingSpaceCount(string str, int max)
                {
                    for (int i = 0; i < str.Length; i++) {
                        if (i >= max || !char.IsWhiteSpace(str[i]))
                            return i;
                    }
                    return str.Length;
                }
            }
        }

        private static LanguagePack GetLanguagePack(string languageCode)
        {
            if (!_languagePacks.TryGetValue(languageCode, out var res))
                throw new KeyNotFoundException($"Translations for {languageCode} is not found.");
            return res;
        }
    }
}