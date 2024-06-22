using System.Collections.Generic;

public class LanguageSelector
{
    /*
     * Language ID
     * 0 - English (Default)
     * 1 - Chinese
     */
    public static List<LocalizedText_Legacy> localizedTexts = new List<LocalizedText_Legacy>();
    public static bool[] noLineBreak = { false, true };
    private static int language = 0;
    public static int Language
    {
        get { return language; }
        set
        {
            language = value;
            foreach (LocalizedText_Legacy text in localizedTexts) text.SetLanguage(value);
        }
    }
}
