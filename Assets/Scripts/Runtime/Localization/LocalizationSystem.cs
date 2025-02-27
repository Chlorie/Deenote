#nullable enable

using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Deenote.Localization
{
    public static class LocalizationSystem
    {
        internal const string DefaultLanguageCode = "en";
        internal const string DefaultLanguageName = "English";
        private static readonly Dictionary<string, LanguagePack> _languageDict = new();

        private static LanguagePack _currentLanguagePack;
        private static LanguagePack _defaultLanguagePack;

        public static Dictionary<string, LanguagePack>.ValueCollection Languages => _languageDict.Values;

        public static LanguagePack CurrentLanguage
        {
            get => _currentLanguagePack;
            set {
                if (_currentLanguagePack == value)
                    return;

                if (!_languageDict.ContainsKey(value.LanguageCode))
                    ThrowHelper.ThrowArgumentException("Language pack is not found in the dictionary.");

                _currentLanguagePack = value;
                LanguageChanged?.Invoke(value);
            }
        }

        public static event Action<LanguagePack>? LanguageChanged;

        static LocalizationSystem()
        {
            var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
            var files = Directory.GetFiles(folder);
            foreach (var file in files) {
                if (!file.EndsWith(".txt")) continue;
                if (LanguagePack.TryLoad(file, out var pack)) {
                    _languageDict.TryAdd(pack.LanguageCode, pack);
                }
            }

            if (!_languageDict.ContainsKey(DefaultLanguageCode)) {
                Debug.LogWarning("Default language translation file is not found.");
                _languageDict.Add(DefaultLanguageCode, LanguagePack.FallbackDefault);
            }

            _currentLanguagePack = _defaultLanguagePack = _languageDict[DefaultLanguageCode];
        }

        public static string GetText(LocalizableText text) =>
            !text.IsLocalized ? text.TextOrKey
                : _currentLanguagePack.GetTranslationOrDefault(text.TextOrKey) ??
                  _defaultLanguagePack.GetTranslationOrDefault(text.TextOrKey) ??
                  text.TextOrKey;
    }
}