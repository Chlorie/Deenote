#nullable enable

using System;
using UnityEngine;

namespace Deenote.Localization
{
    [Serializable]
    public struct LocalizableText
    {
        [SerializeField] private bool _isLocalized;
        [SerializeField] private string _textOrKey;
        public readonly bool IsLocalized => _isLocalized;
        public readonly string TextOrKey => _textOrKey;

        private LocalizableText(bool isLocalized, string textOrKey)
        {
            _isLocalized = isLocalized;
            _textOrKey = textOrKey;
        }

        public static LocalizableText Raw(string text) => new(false, text);

        public static LocalizableText Localized(string textKey) => new(true, textKey);

        public override readonly bool Equals(object? obj) => obj is LocalizableText text && this == text;
        public override readonly int GetHashCode() => HashCode.Combine(IsLocalized, TextOrKey);

        public static bool operator ==(LocalizableText left, LocalizableText right) =>
            left.IsLocalized == right.IsLocalized && left.TextOrKey == right.TextOrKey;

        public static bool operator !=(LocalizableText left, LocalizableText right) => !(left == right);

        public static bool operator ==(LocalizableText left, string right) => left == Raw(right);
        public static bool operator !=(LocalizableText left, string right) => left != Raw(right);
    }
}