#nullable enable

using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using UnityEngine;

namespace Deenote.Localization
{
    public sealed class LanguagePack
    {
        public string LanguageCode { get; }
        public string LanguageDisplayName { get; }
        private Dictionary<string, string> _translations;

        public string GetTranslation(string key) => _translations[key];

        public string? GetTranslationOrDefault(string key) => _translations.TryGetValue(key, out var val) ? val : null;

        private LanguagePack(string code, string displayName, Dictionary<string, string> translations)
        {
            LanguageCode = code;
            LanguageDisplayName = displayName;
            _translations = translations;
        }

        internal static bool TryLoad(string filePath, [NotNullWhen(true)] out LanguagePack? pack)
        {
            using var reader = File.OpenText(filePath);
            pack = null;
            var code = reader.ReadLine();
            if (code is null)
                return false;
            var name = reader.ReadLine();
            if (name is null)
                return false;

            var translations = new Dictionary<string, string>();

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
                if (!translations.TryAdd(key, value))
                    Debug.LogWarning($"Language pack {name} contains duplicated key: {key}");
            }

            pack = new LanguagePack(code, name, translations);
            return true;

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

        private static LanguagePack? _fallbackDefault;
        internal static LanguagePack FallbackDefault => _fallbackDefault ??= new LanguagePack(
            LocalizationSystem.DefaultLanguageCode,
            LocalizationSystem.DefaultLanguageName,
            new Dictionary<string, string>());
    }
}