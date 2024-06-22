namespace Deenote.Localization
{
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
    }
}