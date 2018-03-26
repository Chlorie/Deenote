using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public static StatusBar instance;
    public UIParameters uiParameters;
    public LocalizedText statusText;
    public Image background;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of StatusBar");
        }
    }
    public static void SetStrings(string[] strings, bool error = false)
    {
        instance.statusText.Strings = strings;
        instance.background.color = error ? instance.uiParameters.statusBarErrorColor : instance.uiParameters.statusBarDefaultColor;
    }
}
