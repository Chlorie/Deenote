using UnityEngine;

public class ToolbarInitialization : MonoBehaviour
{
    public ToolbarSelectable projectSelectable;
    public ToolbarSelectable editSelectable;
    public ToolbarSelectable settingsSelectable;
    private void InitializeProjectSelectable()
    {
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new string[] { "Test window" },
            operation = new Operation
            {
                callback = delegate { FileExplorer.instance.Open(FileExplorer.Mode.SelectFile, delegate { }); },
                shortcut = new Shortcut { key = KeyCode.O }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.O }
        });
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new string[] { "Quit", "退出" },
            operation = new Operation
            {
                callback = delegate { }, // Add this later
                shortcut = new Shortcut { key = KeyCode.Q }
            },
            globalShortcut = new Shortcut { alt = true, key = KeyCode.F4 }
        });
    }
    private void InitializeEditSelectable()
    {
        //editSelectable.items.Add(new ButtonInfo
        //{
        //    strings = new string[] { "Undo", "撤销" },
        //    shortcut = "Ctrl+Z",
        //    callback = delegate { }
        //});
        //editSelectable.items.Add(new ButtonInfo
        //{
        //    strings = new string[] { "Redo", "重做" },
        //    shortcut = "Ctrl+Y",
        //    callback = delegate { }
        //});
    }
    private void InitializeSettingsSelectable()
    {
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new string[] { "Set Language to English", "将语言设置为英文" },
            operation = new Operation
            {
                callback = delegate { LanguageController.Language = 0; },
                shortcut = new Shortcut { key = KeyCode.E }
            }
        });
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new string[] { "Set Language to Chinese", "将语言设置为中文" },
            operation = new Operation
            {
                callback = delegate { LanguageController.Language = 1; },
                shortcut = new Shortcut { key = KeyCode.C }
            }
        });
    }
    private void Start()
    {
        InitializeProjectSelectable();
        InitializeEditSelectable();
        InitializeSettingsSelectable();
        StatusBar.SetStrings(new string[]
        {
            "Initialized toolbar selectables",
            "工具栏选项初始化完成"
        });
    }
}
