using System.Collections.Generic;

public static class LanguageController
{
    public static readonly List<LocalizedText> LocalizedTexts = new List<LocalizedText>();
    public static readonly bool[] NoLineBreak = { false, true };
    private static int _language;
    public static int LanguageCount => NoLineBreak.Length;
    public delegate void LanguageChangeCall();
    public static event LanguageChangeCall Call;
    public static int Language
    {
        get => _language;
        set
        {
            _language = value;
            for (int i = 0; i < LocalizedTexts.Count; i++) LocalizedTexts[i].SetLanguage(value);
            Call?.Invoke();
        }
    }
}
