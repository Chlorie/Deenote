using System.Collections.Generic;

public class LanguageSelector
{
    /*
     * Language ID
     * 0 - English (Default)
     * 1 - Chinese
     */
    public static List<LocalizedText> localizedTexts = new List<LocalizedText>();
    public static bool[] noLineBreak = { false, true };
    private static int language = 0;
    public static int Language
    {
        get { return language; }
        set
        {
            language = value;
            foreach (LocalizedText text in localizedTexts) text.SetLanguage(value);
        }
    }
}
