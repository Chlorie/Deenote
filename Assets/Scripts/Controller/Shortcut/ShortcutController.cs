using System.Collections.Generic;
using UnityEngine;

public class ShortcutController : MonoBehaviour
{
    public static ShortcutController instance;
    [HideInInspector] public List<ToolbarSelectable> toolbarSelectables;
    public static int selectedInputField = 0;
    private void CheckShortcuts()
    {
        if (selectedInputField == 0) // No input fields selected
        {
            if (!WindowsController.instance.Blocking) // No front windows obstructing
            {
                // Check toolbar activating shortcut sequences
                // Check global shortcut sequences in toolbar dropdowns
                for (int i = 0; i < toolbarSelectables.Count; i++)
                {
                    if (toolbarSelectables[i].shortcut?.IsActive == true)
                    {
                        toolbarSelectables[i].OnClick(); // Activate corresponding toolbar selectable
                        return;
                    }
                    for (int j = 0; j < toolbarSelectables[i].operations.Count; j++)
                        if (toolbarSelectables[i].operations[j].globalShortcut?.IsActive == true)
                        {
                            toolbarSelectables[i].operations[j].operation.callback?.Invoke(); // Invoke corresponding method
                            return;
                        }
                }
            }
            if (ToolbarController.instance.currentSelected == null) // No toolbar dropdowns obstructing
            {
                // Check shortcut sequences of currently focusing window
                Window window = WindowsController.instance.focusedWindow;
                if (window != null)
                {
                    List<Operation> operations = window.operations;
                    for (int i = 0; i < operations.Count; i++)
                        if (operations[i].shortcut?.IsActive == true)
                        {
                            operations[i].callback?.Invoke(); // Invoke corresponding method
                            return;
                        }
                }
            }
            else
            {
                // Check shortcuts in currently opened toolbar dropdown
                List<ToolbarOperation> operations = ToolbarController.instance.currentSelected.operations;
                for (int i = 0; i < operations.Count; i++)
                    if (operations[i].operation.shortcut?.IsActive == true)
                    {
                        ToolbarController.instance.DeselectAll();
                        operations[i].operation.callback?.Invoke(); // Invoke corresponding method
                        return;
                    }
            }
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ShortcutController");
        }
    }
    private void Update()
    {
        CheckShortcuts();
    }
}
