using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using UnityEngine;

namespace Deenote.Localization
{
    [SuppressMessage("ReSharper", "LocalVariableHidesMember")]
    public sealed class LocalizationSystem : MonoBehaviour
    {
        private string _defaultLanguage = "English";
        private Dictionary<string, string> _defaultLocalizedTexts = null!;

        private string? _currentLanguage;
        private Dictionary<string, string>? _localizedTexts;

        private List<Dictionary<string, string>> _allTextDictionaries = new();

        public event Action? OnLanguageChanged;

        // We public List<string> because Unity's Dropdown requires a List
        public List<string> Languages { get; } = new();

        public string CurrentLanguage
        {
            get => _currentLanguage ??= _defaultLanguage;
            set {
                if (_currentLanguage == value)
                    return;
                _currentLanguage = value;

                if (!TryGetTextDictionary(_currentLanguage, out _localizedTexts)) {
                    _currentLanguage = null;
                }

                OnLanguageChanged?.Invoke();
                MainSystem.PreferenceWindow.NotifyLanguageChanged(value);
            }
        }

        public string GetText(LocalizableText text)
        {
            if (!text.IsLocalized)
                return text.TextOrKey;

            if (_localizedTexts?.TryGetValue(text.TextOrKey, out var val) == true)
                return val;
            else if (_defaultLocalizedTexts.TryGetValue(text.TextOrKey, out val))
                return val;
            else
                return text.TextOrKey;
        }

        private void Awake()
        {
            _defaultLanguage = "English";

            // Load all languages at start
            var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
            var files = Directory.GetFiles(folder);
            foreach (var file in files) {
                if (file.EndsWith(".txt")) {
                    var (name, texts) = LoadLanguagePack(file);
                    Languages.Add(name);
                    _allTextDictionaries.Add(texts);
                }
            }

            TryGetTextDictionary(_defaultLanguage, out var defTexts);
            _defaultLocalizedTexts = defTexts ?? new Dictionary<string, string>();
        }

        private (string Name, Dictionary<string, string> Texts) LoadLanguagePack(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            using var reader = new StreamReader(fs);

            Dictionary<string, string> dict = new();
            var name = reader.ReadLine();

            while (!reader.EndOfStream && reader.ReadLine() is { } line) {
                // Comment
                if (line.StartsWith('#'))
                    continue;

                // If '=' not found, we ignore this line
                int seperator = line.IndexOf('=');
                if (seperator < 0)
                    continue;

                string key = line[..seperator];
                string value;
                // ReSharper disable once ReplaceSequenceEqualWithConstantPattern
                if (line.AsSpan(seperator + 1).SequenceEqual("\"\"\"")) {
                    StringBuilder sb = new();
                    while (!reader.EndOfStream) {
                        line = reader.ReadLine();
                        if (line is null or "\"\"\"")
                            break;
                        sb.AppendLine(line);
                    }
                    value = sb.ToString();
                }
                else {
                    value = line[(seperator + 1)..].Replace("<br/>", "\r\n");
                }

                if (!dict.TryAdd(key, value))
                    Debug.LogWarning($"Language pack {name} contains duplicated key: {key}");
            }

            return (name, dict);
        }

        private bool TryGetTextDictionary(string languageName, [NotNullWhen(true)] out Dictionary<string, string>? texts)
        {
            var index = Languages.IndexOf(languageName);
            if (index < 0) {
                texts = null;
                return false;
            }
            texts = _allTextDictionaries[index];
            return true;
        }
    }
}