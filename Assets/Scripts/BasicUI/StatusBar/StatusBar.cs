using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    public static StatusBar Instance { get; private set; }
    public LocalizedText statusText;
    public Image background;
    public static bool ErrorState
    {
        set
        {
            Instance.background.color = value ? Parameters.Params.statusBarErrorColor :
           Parameters.Params.statusBarDefaultColor;
        }
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of StatusBar");
        }
    }
    public static void SetStrings(params string[] strings) => Instance.statusText.Strings = strings;
}
