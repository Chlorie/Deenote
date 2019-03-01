using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public Window parentWindow;
    public void CloseWindow() => parentWindow.Close();
    // If this button appears in a window, add a close shortcut to the window
    private void Awake() => parentWindow.operations.Add(new Operation
    {
        callback = CloseWindow,
        shortcut = new Shortcut { key = KeyCode.F4, ctrl = true } // Ctrl+F4 to close the window
    });
    // Called in editor, automatically fill Parent Window field
    private void OnValidate()
    {
        Window window = null;
        Transform windowTransform = transform;
        while (windowTransform != null && window == null)
        {
            window = windowTransform.GetComponent<Window>();
            windowTransform = windowTransform.parent;
        }
        parentWindow = window;
    }
}
