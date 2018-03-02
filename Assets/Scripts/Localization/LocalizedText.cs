using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    private Text text;
    private Text textProperty { get { if (text == null) text = GetComponent<Text>(); return text; } }
    [SerializeField][TextArea] private string[] strings;
    public string[] Strings { get { return strings; } }
    [HideInInspector] public Color color { set { textProperty.color = value; } }
    public string CurrentText { get { return textProperty.text; } }
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
            textProperty.text = LineBreakConversion(strings[language], LanguageSelector.noLineBreak[language]);
        else
            textProperty.text = LineBreakConversion(strings[0], LanguageSelector.noLineBreak[0]);
    }
    private string LineBreakConversion(string original, bool noLineBreak)
    {
        if (noLineBreak)
            return original.Replace(" ", "\u00A0");
        else
            return original;
    }
    private void Awake()
    {
        LanguageSelector.localizedTexts.Add(this);
        SetLanguage(LanguageSelector.Language);
    }
    private void OnDestroy()
    {
        LanguageSelector.localizedTexts.Remove(this);
    }
}
