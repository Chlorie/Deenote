using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] [TextArea] private string[] strings;
    private Text text;
    private Text TextProperty { get { return text ?? (text = gameObject.GetComponent<Text>()); } }
    [HideInInspector] public Color color { set { text.color = value; } }
    public string[] Strings
    {
        get
        {
            return strings;
        }
        set
        {
            if (value.Length > LanguageController.LanguageCount) // Too much texts
            {
                Debug.LogError("Error: Too many languages in a LocalizedText");
                return;
            }
            strings = value;
            SetLanguage(LanguageController.Language);
        }
    }
    public void SetStrings(params string[] newStrings)
    {
        Strings = newStrings;
    }
    public void SetLanguage(int language)
    {
        if (strings == null || strings.Length == 0) // Empty strings
            Debug.Log("An instance of LocalizedText is not initialized when SetLanguage");
        else
        {
            if (language >= strings.Length) language = 0;
            TextProperty.text = SpaceConverter(strings[language], LanguageController.noLineBreak[language]);
        }
    }
    public void ForceUpdate()
    {
        SetLanguage(LanguageController.Language);
    }
    private string SpaceConverter(string original, bool noLineBreak)
    {
        if (noLineBreak)
            return original.Replace(' ', '\u00a0');
        else
            return original;
    }
    private void Awake()
    {
        LanguageController.localizedTexts.Add(this);
    }
}
