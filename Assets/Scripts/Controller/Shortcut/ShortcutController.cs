using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShortcutController : MonoBehaviour
{
    public static ShortcutController instance;
    [HideInInspector] public List<ToolbarSelectable> toolbarSelectables;
    private bool _focusingOnToolbar = false;
    private void CheckShortcuts()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        // - REMIND - 
        // Remember the following if statement is executed per frame,
        // and it has a GetComponent in it, which is very costly.
        // (Deenote 0.6.7 still does this in the following way)
        // Should change this to something more efficient later.
        if (obj == null || obj.GetComponent<InputField>() == null)
        {
            if (WindowsController.instance.frontWindows.Count == 0) // No front windows obstructing
            {
                // Check toolbar activating shortcut sequences
                // Check global shortcut sequences in toolbar dropdowns
                for (int i = 0; i < toolbarSelectables.Count; i++)
                {
                    if (toolbarSelectables[i].shortcut?.IsActive == true)
                        toolbarSelectables[i].OnClick(); // Activate corresponding toolbar selectable
                    for (int j = 0; j < toolbarSelectables[i].operations.Count; j++)
                        if (toolbarSelectables[i].operations[j].globalShortcut?.IsActive == true)
                            toolbarSelectables[i].operations[j].operation.callback?.Invoke(); // Invoke corresponding method
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
                            operations[i].callback?.Invoke(); // Invoke corresponding method
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
            Debug.LogError("Error: Unexpected multiple instance of ShortcutController");
        }
    }
    private void Update()
    {
        CheckShortcuts();
    }
}
