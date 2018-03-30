using System.Collections.Generic;
using UnityEngine;

public class WindowsController : MonoBehaviour
{
    public static WindowsController instance;
    private List<Window> _blockingWindows = new List<Window>(); // Windows like message boxes that cannot lose focus
    [HideInInspector] public Window focusedWindow;
    [SerializeField] private GameObject _actionBlocker;
    public bool Blocking => _blockingWindows.Count != 0;
    public bool SetFocusToWindow(Window window)
    {
        if (_blockingWindows.Count == 0 || window.blocking)
        {
            window.transform.SetAsLastSibling();
            focusedWindow = window;
        }
        return _blockingWindows.Count == 0;
    }
    public void MoveWindowToBottom(Window window)
    {
        window.transform.SetAsFirstSibling();
    }
    public void AddBlockingWindow(Window window)
    {
        if (!Blocking) _actionBlocker.SetActive(true);
        _actionBlocker.transform.SetSiblingIndex(transform.childCount - 2);
        _blockingWindows.Add(window);
    }
    public void RemoveBlockingWindow(Window window)
    {
        _blockingWindows.Remove(window);
        if (!Blocking) { _actionBlocker.SetActive(false); return; }
        _actionBlocker.transform.SetSiblingIndex(transform.childCount - 2);
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
