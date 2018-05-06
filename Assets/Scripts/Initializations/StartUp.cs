using UnityEngine;

public class StartUp : MonoBehaviour
{
    private void Start()
    {
        LanguageController.Language = 0;
        AppConfig.Read();
        StatusBar.SetStrings("Successfully launched the application", "程序启动成功");
    }
}
