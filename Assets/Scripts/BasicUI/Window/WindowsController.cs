using System.Collections.Generic;
using UnityEngine;

public class WindowsController : MonoBehaviour
{
    public static WindowsController instance;
    [HideInInspector] public List<Window> frontWindows = new List<Window>(); // Windows like message boxes that cannot lose focus
    public List<Window> windows = new List<Window>();
    public bool SetFocusToWindow(Window window)
    {
        if (frontWindows.Count == 0)
            window.transform.SetSiblingIndex(transform.childCount - 1);
        return frontWindows.Count == 0;
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instance of WindowsController");
        }
    }
}
