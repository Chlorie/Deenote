using System.Collections.Generic;

public class LanguageController
{
    public static List<LocalizedText> localizedTexts = new List<LocalizedText>();
    public static bool[] noLineBreak = { false, true };
    private static int language = 0;
    public static int LanguageCount { get { return noLineBreak.Length; } }
    public delegate void LanguageChangeCall();
    public static LanguageChangeCall call = null;
    public static int Language
    {
        get { return language; }
        set
        {
            language = value;
            for (int i = 0; i < localizedTexts.Count; i++) localizedTexts[i].SetLanguage(value);
            call?.Invoke();
        }
    }
}
