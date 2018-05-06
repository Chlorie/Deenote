using UnityEngine;
using UnityEditor;

public class QuitApp : MonoBehaviour
{
    public static QuitApp Instance { get; private set; }
    public static void ShowConfirmQuitMessage()
    {
        MessageBox.Activate(new[] { "Quit", "退出" }, new[] { "Are you sure to quit Deenote?", "你确认要退出Deenote吗？" },
            new MessageBox.ButtonInfo
            {
                callback = () =>
                {
                    Instance.QuitAppActions();
                },
                texts = new[] { "Quit and save", "退出并保存" }
            },
            new MessageBox.ButtonInfo
            {
                callback = () =>
                {
                    Instance.QuitAppActions();
                },
                texts = new[] { "Quit but not save", "退出但不保存" }
            },
            new MessageBox.ButtonInfo
            {
                callback = null,
                texts = new[] { "Back", "返回" }
            });
    }
    private void QuitAppActions()
    {
        AppConfig.Write();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of QuitApp");
        }
    }
    private void OnApplicationQuit() => ShowConfirmQuitMessage();
}
