using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText_Legacy : MonoBehaviour
{
    private Text text;
    private Text TextProperty { get { if (text == null) text = GetComponent<Text>(); return text; } }
    [SerializeField] [TextArea] private string[] strings;
    public string[] Strings => strings;
    public Color Color { set => TextProperty.color = value; }
    public string CurrentText => TextProperty.text;
    public void SetStrings(params string[] newStrings)
    {
        strings = newStrings;
        if (strings != null) SetLanguage(LanguageSelector.Language);
    }
    public void SetLanguage(int language)
    {
        if (strings == null || strings.Length == 0)
            Debug.Log("No localization text assigned for a certain LocalizationText component");
        else if (language < strings.Length)
            TextProperty.text = LineBreakConversion(strings[language], LanguageSelector.noLineBreak[language]);
        else
            TextProperty.text = LineBreakConversion(strings[0], LanguageSelector.noLineBreak[0]);
    }
    private string LineBreakConversion(string original, bool noLineBreak) =>
        noLineBreak ? original.Replace(" ", "\u00A0") : original;
    private void Awake()
    {
        LanguageSelector.localizedTexts.Add(this);
        SetLanguage(LanguageSelector.Language);
    }
    private void OnDestroy() => LanguageSelector.localizedTexts.Remove(this);
}
