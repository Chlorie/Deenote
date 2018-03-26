using System.Collections.Generic;
using UnityEngine;

public class WindowsController : MonoBehaviour
{
    public static WindowsController instance;
    [HideInInspector] public List<Window> frontWindows = new List<Window>(); // Windows like message boxes that cannot lose focus
    [HideInInspector] public Window focusedWindow;
    public bool SetFocusToWindow(Window window)
    {
        if (frontWindows.Count == 0)
        {
            window.transform.SetSiblingIndex(transform.childCount - 1);
            focusedWindow = window;
        }
        return frontWindows.Count == 0;
    }
    public void UpdateFocusedWindowRef()
    {
        focusedWindow = (transform.childCount == 0) ? null : transform.GetChild(transform.childCount - 1).GetComponent<Window>();
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of WindowsController");
        }
    }
}
