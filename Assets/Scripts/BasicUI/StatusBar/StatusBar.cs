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
            Debug.LogError("Error: Unexpected multiple instance of StatusBar");
        }
    }
    public static void SetStrings(string[] strings, bool error = false)
    {
        instance.statusText.Strings = strings;
        if (error)
            instance.background.color = instance.uiParameters.statusBarErrorColor;
        else
            instance.background.color = instance.uiParameters.statusBarDefaultColor;
    }
}
