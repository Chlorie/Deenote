using UnityEngine;

public class QuitApp : MonoBehaviour
{
    public static QuitApp Instance { get; private set; }
    public static void ShowConfirmQuitMessage()
    {
        string[] title = { "Quit", "退出" };
        string[] notice = { "Are you sure to quit Deenote?", "你确认要退出Deenote吗？" };
        if (EditTracker.Instance.Edited)
            MessageBox.Instance.Activate(title, notice,
                new MessageBox.ButtonInfo
                {
                    callback = () =>
                    {
                        ProjectManagement.Save();
                        Instance.QuitAppActions();
                    },
                    texts = new[] { "Quit and save", "退出并保存" }
                },
                new MessageBox.ButtonInfo
                {
                    callback = Instance.QuitAppActions,
                    texts = new[] { "Quit but not save", "退出但不保存" }
                },
                new MessageBox.ButtonInfo
                {
                    callback = null,
                    texts = new[] { "Back", "返回" }
                });
        else
            MessageBox.Instance.Activate(title, notice,
                new MessageBox.ButtonInfo
                {
                    callback = Instance.QuitAppActions,
                    texts = new[] { "Quit", "退出" }
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
        UnityEditor.EditorApplication.isPlaying = false;
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
