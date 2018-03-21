using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarInitialization : MonoBehaviour
{
    public ToolbarSelectable projectSelectable;
    public ToolbarSelectable editSelectable;
    public ToolbarSelectable settingsSelectable;
    private void InitializeProjectSelectable()
    {
        projectSelectable.items.Add(new ButtonInfo
        {
            strings = new string[] { "Test window" },
            shortcut = "",
            callback = delegate
            {
                FileExplorer.instance.Open(FileExplorer.Mode.SelectFile, delegate { });
            }
        });
        projectSelectable.items.Add(new ButtonInfo
        {
            strings = new string[] { "Quit(Q)", "退出(Q)" },
            shortcut = "Alt+F4",
            callback = delegate { }
        });
    }
    private void InitializeEditSelectable()
    {
        //editSelectable.items.Add(new ButtonInfo
        //{
        //    strings = new string[] { "Undo(U)", "撤销(U)" },
        //    shortcut = "Ctrl+Z",
        //    callback = delegate { }
        //});
        //editSelectable.items.Add(new ButtonInfo
        //{
        //    strings = new string[] { "Redo(R)", "重做(R)" },
        //    shortcut = "Ctrl+Y",
        //    callback = delegate { }
        //});
    }
    private void InitializeSettingsSelectable()
    {
        settingsSelectable.items.Add(new ButtonInfo
        {
            strings = new string[] { "Set Language to English(E)", "将语言设置为英文(E)" },
            callback = delegate { LanguageController.Language = 0; }
        });
        settingsSelectable.items.Add(new ButtonInfo
        {
            strings = new string[] { "Set Language to Chinese(C)", "将语言设置为中文(C)" },
            callback = delegate { LanguageController.Language = 1; }
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
