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
        private const string DefaultLanguage = "English";
        private Dictionary<string, string>? _defaultLocalizedTexts;

        private string? _currentLanguage;
        private Dictionary<string, string>? _localizedTexts;

        private List<LocalizedText> _aliveTexts = new();

        private Dictionary<string, Dictionary<string, string>> _languages = new();

        public IReadOnlyCollection<string> Languages => _languages.Keys;

        public string CurrentLanguage => _currentLanguage ??= DefaultLanguage;

        public void SetLanguage(string language)
        {
            if (_currentLanguage == language)
                return;

            _localizedTexts = _languages[language];
            _currentLanguage = language;

            foreach (var locText in _aliveTexts)
            {
                locText.NotifyLanguageUpdated();
            }
        }

        public string GetLocalizedText(string key)
        {
            if (_localizedTexts?.TryGetValue(key, out var val) == true)
                return val;
            else if (_defaultLocalizedTexts?.TryGetValue(key, out val) ?? false)
                return val;
            else
                return key;
        }

        public void RegisterLocalizedText(LocalizedText text) => _aliveTexts.Add(text);

        public void UnregisterLocalizedText(LocalizedText text) => _aliveTexts.Remove(text);

        private void Awake()
        {
            // Load all languages at start
            var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
            var files = Directory.GetFiles(folder);
            foreach (var file in files)
            {
                if (!file.EndsWith(".txt")) continue;
                var (name, texts) = LoadLanguagePack(file);
                _languages.Add(name, texts);
            }

            _defaultLocalizedTexts = _languages[DefaultLanguage];
        }

        private (string Name, Dictionary<string, string> Texts) LoadLanguagePack(string filePath)
        {
            using var fs = File.OpenRead(filePath);
            using var reader = new StreamReader(fs);

            var dict = new Dictionary<string, string>();

            var name = reader.ReadLine();

            while (!reader.EndOfStream && reader.ReadLine() is { } line)
            {
                // Comment
                if (line.StartsWith('#'))
                    continue;

                // If '=' not found, we ignore this line
                int seperator = line.IndexOf('=');
                if (seperator < 0)
                    continue;

                string key = line[..seperator];
                string value;
                if (line.AsSpan(seperator + 1) == "\"\"\"")
                {
                    StringBuilder sb = new();
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();
                        if (line is null or "\"\"\"")
                            break;
                        sb.AppendLine(line);
                    }
                    value = sb.ToString();
                }
                else
                {
                    value = line[(seperator + 1)..].Replace("<br/>", "\r\n");
                }

                if (!dict.TryAdd(key, value))
                    Debug.LogWarning($"Language pack {name} contains duplicated key: {key}");
            }

            return (name, dict);
        }
    }
}