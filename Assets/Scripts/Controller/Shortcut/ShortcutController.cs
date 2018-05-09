using System.Collections.Generic;
using UnityEngine;

public class ShortcutController : MonoBehaviour
{
    public static ShortcutController Instance { get; private set; }
    [HideInInspector] public List<ToolbarSelectable> toolbarSelectables;
    public static int selectedInputField = 0;
    private void CheckShortcuts()
    {
        if (selectedInputField == 0) // No input fields selected
        {
            if (!WindowsController.Instance.Blocking) // No front windows obstructing
            {
                // Check toolbar activating shortcut sequences
                // Check global shortcut sequences in toolbar dropdowns
                foreach (ToolbarSelectable toolbarSelectable in toolbarSelectables)
                {
                    if (toolbarSelectable.shortcut?.IsActive == true)
                    {
                        toolbarSelectable.OnClick(); // Activate corresponding toolbar selectable
                        return;
                    }
                    foreach (ToolbarOperation operation in toolbarSelectable.operations)
                        if (operation.globalShortcut?.IsActive == true)
                        {
                            operation.operation.callback?.Invoke(); // Invoke corresponding method
                            return;
                        }
                }
            }
            if (ToolbarController.Instance.currentSelected == null) // No toolbar dropdowns obstructing
            {
                // Check shortcut sequences of currently focusing window
                Window window = WindowsController.Instance.focusedWindow;
                if (window != null)
                    foreach (Operation operation in window.operations)
                        if (operation.shortcut?.IsActive == true)
                        {
                            operation.callback?.Invoke(); // Invoke corresponding method
                            return;
                        }
            }
            else
            {
                // Check shortcuts in currently opened toolbar dropdown
                foreach (ToolbarOperation operation in ToolbarController.Instance.currentSelected.operations)
                    if (operation.operation.shortcut?.IsActive == true)
                    {
                        ToolbarController.Instance.DeselectAll();
                        operation.operation.callback?.Invoke(); // Invoke corresponding method
                        return;
                    }
            }
        }
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ShortcutController");
        }
    }
    private void Update() => CheckShortcuts();
}
