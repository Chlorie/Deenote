using System.IO;
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
    private string[] _extensions;
    private DirectoryInfo _currentDirectory = null;
    private Mode _mode;
    private Callback _callback;
    public string Result { get; private set; }
    protected override void Open()
    {
        base.Open();
        tagContent.ForceUpdate();
        MoveToCenter();
    }
    public void Open(Mode mode, Callback callback, params string[] extensions)
    {
        Open();
        _extensions = extensions;
        _mode = mode;
        if (callback == null)
        {
            Close();
            Debug.LogError("Error: Expected callback when opening the file explorer");
        }
        _callback = callback;
        if (_currentDirectory == null) _currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        
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
