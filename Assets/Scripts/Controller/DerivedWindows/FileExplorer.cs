using System.Collections;
using System.Collections.Generic;
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
        _tagContent.ForceUpdate();
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
            Debug.LogError("Error: Unexpected multiple instance of FileExplorer");
        }
    }
}
