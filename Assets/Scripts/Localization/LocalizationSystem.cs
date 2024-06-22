using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Deenote.Localization
{
    public sealed class LocalizationSystem : MonoBehaviour
    {
        private string _defaultLanguage;
        private Dictionary<string, string> _defaultLocalizedTexts;

        private string _currentLanguage;
        private Dictionary<string, string> _localizedTexts;

        private List<LocalizedText> _aliveTexts = new();

        private Dictionary<string, Dictionary<string, string>> _languages;

        public IReadOnlyCollection<string> Languages => _languages.Keys;

        public string CurrentLanguage => _currentLanguage ??= _defaultLanguage;

        public void SetLanguage(string language)
        {
            if (_currentLanguage == language)
                return;

            _localizedTexts = _languages[language];
            _currentLanguage = language;

            foreach (var locText in _aliveTexts) {
                locText.NotifyLanguageUpdated();
            }
        }

        public string GetLocalizedText(string key)
        {
            if (_localizedTexts?.TryGetValue(key, out var val) == true)
                return val;
            else if (_defaultLocalizedTexts.TryGetValue(key, out val))
                return val;
            else
                return key;
        }

        public void RegisterLocalizedText(LocalizedText text) => _aliveTexts.Add(text);

        public void UnregisterLocalizedText(LocalizedText text) => _aliveTexts.Remove(text);

        private void Awake()
        {
            _defaultLanguage = "English";
            _aliveTexts = new();

            // Load all languages at start
            var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
            var files = Directory.GetFiles(folder);
            _languages = new();
            foreach (var file in files) {
                if (file.EndsWith(".txt")) {
                    var (name, texts) = LoadLanguagePack(file);
                    _languages.Add(name, texts);
                }
            }

            _defaultLocalizedTexts = _languages[_defaultLanguage];
        }

        private (string Name, Dictionary<string, string> Texts) LoadLanguagePack(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            using var reader = new StreamReader(fs);

            var dict = new Dictionary<string, string>();

            var name = reader.ReadLine();

            while (true) {
                var line = reader.ReadLine();
                if (line is null)
                    break;

                // Comment
                if (line.StartsWith('#'))
                    continue;

                // If '=' not found, we ignore this line
                var seperator = line.IndexOf('=');
                if (seperator < 0)
                    continue;

                dict.Add(line[..seperator], line[(seperator + 1)..].Replace("<br/>","\n"));
            }

            return (name, dict);
        }
    }
}