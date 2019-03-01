using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] [TextArea] private string[] _strings;
    private Text _text;
    private Text TextProperty => _text ? _text : (_text = gameObject.GetComponent<Text>());
    public Color Color { set => _text.color = value; }
    public string[] Strings
    {
        get => _strings;
        set
        {
            if (value.Length > LanguageController.LanguageCount) // Too much texts
            {
                Debug.LogError("Error: Too many languages in a LocalizedText");
                return;
            }
            _strings = value;
            SetLanguage(LanguageController.Language);
        }
    }
    public void SetLanguage(int language)
    {
        if (_strings == null || _strings.Length == 0) // Empty strings
            Debug.Log("An instance of LocalizedText is not initialized when SetLanguage");
        else
        {
            if (language >= _strings.Length) language = 0;
            TextProperty.text = SpaceConverter(_strings[language], LanguageController.NoLineBreak[language]);
        }
    }
    private string SpaceConverter(string original, bool noLineBreak) => 
        noLineBreak ? original.Replace(' ', '\u00a0') : original;
    private void Awake() => LanguageController.LocalizedTexts.Add(this);
    private void Start() => SetLanguage(LanguageController.Language);
    // Called in editor, automatically updates the text component
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_strings != null && _strings.Length != 0 && !UnityEditor.EditorApplication.isPlaying)
            TextProperty.text = _strings[0];
    }
#endif
}
