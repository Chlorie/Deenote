using UnityEngine;

public class FileExplorer : Window
{
    public static FileExplorer instance;
    public enum Mode
    {
        SelectFolder,
        SelectFile,
        InputFileName
    }
    protected override void Open()
    {
        base.Open();
        tagContent.ForceUpdate();
        MoveToCenter();
    }
    public void Open(Mode mode, Callback callback, params string[] extensions)
    {
        Open();
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of FileExplorer");
        }
    }
}
