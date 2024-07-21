using System;

namespace Deenote.Localization
{
    [Serializable]
    public readonly struct LocalizableText
    {
        public readonly bool IsLocalized;
        public readonly string TextOrKey;

        private LocalizableText(bool isLocalized, string textOrKey)
        {
            IsLocalized = isLocalized;
            TextOrKey = textOrKey;
        }

        public static LocalizableText Raw(string text) => new(false, text);

        public static LocalizableText Localized(string textKey) => new(true, textKey);

        public override bool Equals(object obj) => obj is LocalizableText text && (this == text);
        public override int GetHashCode() => HashCode.Combine(IsLocalized, TextOrKey);

        public static bool operator ==(LocalizableText left, LocalizableText right) => left.IsLocalized == right.IsLocalized && left.TextOrKey == right.TextOrKey;
        public static bool operator !=(LocalizableText left, LocalizableText right) => !(left == right);

        public static bool operator ==(LocalizableText left, string right) => left == Raw(right);
        public static bool operator !=(LocalizableText left, string right) => left != Raw(right);
    }
}