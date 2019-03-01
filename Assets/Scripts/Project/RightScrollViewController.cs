using System.Diagnostics;
using UnityEngine;

public class RightScrollViewController : MonoBehaviour
{
    public GameObject scrollView;
    public void OpenQuitScreen()
    {
        MessageScreen.Activate(
            new[] { "Are you sure that you want to quit?", "真的要退出吗?" },
            new[] { "<color=#ff5555>Make sure that you have SAVED your project!</color>",
                "<color=#ff5555>请确认你已经保存当前的项目文件!</color>" },
            new[] { "Yes, I'm quite sure! Quit now!", "是的, 我很确定(理直气壮)" }, QuitScreenYes,
            new[] { "No, take me back to my project...", "不是, 回到刚才的项目..." });
    }
    public void QuitScreenYes()
    {
        FindObjectOfType<ProjectController>().SavePlayerPrefs();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        Process.GetCurrentProcess().Kill();
#endif
    }
    public void OnButtonClick(GameObject subPanel) => subPanel.SetActive(!(subPanel.activeSelf));
    private void Start()
    {
        scrollView.SetActive(false);
        scrollView.SetActive(true);
    }
}
