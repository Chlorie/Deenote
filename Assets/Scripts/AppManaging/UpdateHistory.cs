using System.Collections.Generic;
using UnityEngine;

public class UpdateHistory : MonoBehaviour
{
    public GameObject panel;
    public LocalizedText text;
    public LocalizedText title;
    public LocalizedText checkUpdateText;
    public VersionChecker versionChecker;
    private List<string> versions = new List<string>
    {
        "Deenote 0.6.11",
        "Deenote 0.6.10",
        "Deenote 0.6.9",
        "Deenote 0.6.8",
        "Deenote 0.6.7",
        "Deenote 0.6.6",
        "Deenote 0.6.5",
        "Deenote 0.6.4",
        "Deenote 0.6.3",
        "Deenote 0.6.2",
        "Deenote 0.6.1",
        "Deenote 0.6",
        "Deenote 0.5.10",
        "Deenote 0.5.9",
        "Deenote 0.5.8",
        "Deenote 0.5.7",
        "Deenote 0.5.6",
        "Deenote 0.5.5",
        "Deenote 0.5.4",
        "Deenote 0.5.3",
        "Deenote 0.5.2",
        "Deenote 0.5.1",
        "Deenote 0.5",
        "Deenote 0.4",
        "Deenote 0.3.1",
        "Deenote 0.3 build 2",
        "Deenote 0.3 build 1",
        "Deenote 0.2.1",
        "Deenote 0.2",
        "Deenote 0.1",
        "Deemo Chart Editor 0.2",
        "Deemo Chart Editor 0.1"
    };
    private List<string[]> updateInfo = new List<string[]>
    {
        new []{ "Final changed the shader used for charming glow effects. Now the glows should be much prettier than before.",
            "终于修改了charming光效的着色器。现在光效要比以前好看多了。" },
        new []{ "Now the lost frame of mp3 audio is partially \"fixed\" by adding an empty frame before the samples. " +
            "There won't be a weird 26ms offset between Deenote and the correct value(from Audition).",
            "部分地\"修复\"了mp3文件丢失第一帧的问题 (在所有采样前面增加一个空帧), 现在Deenote不会有奇怪的26毫秒offset了。" },
        new []{ "Fixed the issue that projects cannot be loaded.\n" +
            "Fixed update checker.",
            "修复了不能打开文件的问题。\n" +
            "修复了更新检查。" },
        new []{ "Fixed the problem of beat lines disappearing when importing charts from JSON files.\n" +
            "Fixed the problem where inserting notes in a large project may cause stack overflow.",
            "解决了导入JSON谱面会导致拍线消失的问题。\n解决了谱面中note很多时插入新note导致栈溢出的问题。" },
        new string[]{ "Added localization support for Chinese.\nFixed some minor bugs.\nCollided notes are tinted red now.",
            "增加了中文的本地化支持。\n修复了一些小漏洞。\n重叠note现在会显示红色。" },
        new string[]{ "Optimized the file format so that the project file won't take much space.",
            "优化了文件格式，项目文件将占用更少空间。" },
        new string[]{ "Added \"mirror\" function.\nAdded shortcut for adjusting note size.\nNow when you paste while " +
            "Shift key is held, the notes will stay in the exact horizontal position as the copied notes.",
            "增加了镜像功能。\n为调整note宽度添加了快捷键。\n现在粘贴时按住Shift键，粘贴的note会保持被复制的note的横向位置。" },
        new string[]{ "Added \"Note ID\" field in selected note info panel.\nBug fix: Slide notes not exported correctly.\n" +
            "Bug fix: Export file name is not correct.",
            "在选中note信息中添加了note编号域。\n修复漏洞：滑键导出格式不正确。\n修复漏洞：导出文件名不正确。" },
        new string[]{ "Now the app should not have problem closing itself.\nAdded options for resizing the window.",
            "现在关闭app应该不会出现问题了。\n增加了调整窗口大小的设置项。" },
        new string[]{ "Bug fix: After opening the tutorial all the inputs are ignored.",
            "修复漏洞：打开教程后所有输入都被忽略。"},
        new string[]{ "Added Chinese tutorial to this app. English tutorial coming soon.",
            "添加了中文教程。英文教程即将更新。" },
        new string[]{ "Added curve forming function.",
            "添加了曲线生成功能。" },
        new string[]{ "Now saving and loading files won't block the main thread.\nFixed the serious bug about music " +
            "playback repositioning.",
            "现在保存和读入文件不会再阻塞主线程了。\n修复了关于音乐回放位置的严重漏洞。" },
        new string[]{ "Separated update history from about.\nAdded update checker.\nMinor bug fixes about UI.",
            "将更新历史从关于中分离出来。\n添加了更新检测。\n修复用户界面的一些小漏洞。" },
        new string[]{ "Now you can change the music file used in the project.\nWhen creating a new file in the file " +
            "selector, files with the target extension will appear.",
            "现在你可以替换项目中使用的音乐文件了。\n创建新文件时同后缀名的已有文件也会在文件选择窗口中显示。" },
        new string[]{ "Bug fix: File cannot be opened.\nBug fix: Link lines cannot be toggled off.\n" +
            "Deleted Schwarzer’s famous words because I don’t want to die. XD",
            "修复漏洞：不能打开文件。\n修复漏洞：滑键连线不能关闭。\n删除了Schwarzer的名言，我还不想死（" },
        new string[]{ "Added drag-and-drop file opener. (Thanks to Schwarzer!)",
            "添加了拖动文件到窗口打开文件的功能。（感谢Schwarzer！）" },
        new string[]{ "Added support for mp3 music files.",
            "添加了对MP3音乐文件的支持。" },
        new string[]{ "Completely reworked on the code of line displaying.",
            "彻底重写了连线显示的代码。" },
        new string[]{ "Added toggle for VSync.\nNow editor settings are saved as well.",
            "添加了垂直同步的开关。\n现在编辑器设置也会被自动保存。" },
        new string[]{ "Changed default volume of piano sounds from 127 to 0.\nNow you can import ogg music files.",
            "将钢琴音的默认音量从127调整为0。\n现在你可以导入OGG音乐文件了。" },
        new string[]{ "Fixed the bug when deleting slide notes the remaining slide notes are incorrectly linked.",
            "修复了删除滑键时剩余的滑键会连接错误的漏洞。" },
        new string[]{ "Copy/Paste functions.\nQuantize notes.",
            "复制/粘贴功能。\n量化note功能。" },
        new string[]{ "Full edit function of note properties.",
            "全部note属性编辑功能。" },
        new string[]{ "File extension association.",
            "文件后缀名关联。" },
        new string[]{ "Minor changes to beat line saving.\nEdit panel UI redesigned.",
            "稍微更改了拍线存储方式。\n重新设计了编辑板块的UI。" },
        new string[]{ "Bug fixes.", "漏洞修复。" },
        new string[]{ "Add new notes.\nLink/Unlink selected notes.\nNote placement indicator.",
            "添加新note。\n连接/断开选中note。\nNote摆放位置指示。" },
        new string[]{ "Added color tint for selected notes.\nRemove notes.\nBeat line filling field auto-fill.",
            "选中note染色。\n删除note。\n拍线填充域自动填充。" },
        new string[]{ "Added all visual effects for chart viewing function.\nAdded a manual BPM calculator.\nVolume control " +
            "for the sounds.\nBeat line filling and displaying.\nLink lines between slide notes.\nUndo/Redo functions.\n" +
            "Fixed a few bugs.\nSelect notes.\n(Hidden feature: Convert Cytus v2 charts into Deemo charts. In my opinion " +
            "no one would like to use this or even care about this.)",
            "添加了谱面预览功能的所有特效。\n添加了一个手动BPM计算器。\n音效的音量控制。\n拍线填充及显示。\n滑键间连线。\n撤销/重做功能。\n" +
            "修复了一些漏洞。\n选择note。\n（隐藏功能：Cytus v2谱面直转。我觉得没人会用到这个功能，甚至都没人关心这个。）" },
        new string[]{ "Added some shortcuts.\nJSON file exporting.\n\"Save as\" feature.\nAdded some visual effects " +
            "(Lowered alpha values for the notes that are far away, added frame-by-frame disappearing animation and " +
            "shock wave/circle animation for notes that hit the judge line).\nFixed a few bugs.",
            "添加了一些快捷键。\n导出JSON格式谱面。\n另存为功能。\n添加了一些视觉效果（远处的note降低不透明度，添加note消失的逐帧动画及" +
            "冲击波和圆形特效）。\n修复了一些漏洞。" },
        new string[]{ "Chart viewing function finished. No effects yet.", "完成了谱面预览功能。目前还没有特效。" }
    };
    private int current = 0;
    private void UpdateContent()
    {
        text.SetStrings(updateInfo[current]);
        title.SetStrings("Update History - " + versions[current] + (current == 0 ? " (Current)" : ""),
            "更新历史 - " + versions[current] + (current == 0 ? " (当前版本)" : ""));
    }
    public void Activate()
    {
        panel.SetActive(true);
        CurrentState.ignoreAllInput = true;
        current = 0;
        UpdateContent();
    }
    public void Deactivate()
    {
        panel.SetActive(false);
        CurrentState.ignoreAllInput = false;
    }
    public void CheckUpdate()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            checkUpdateText.SetStrings("No Internet connection", "无网络连接");
            return;
        }
        versionChecker.CheckForUpdate();
    }
    public void PrevVersion()
    {
        if (current == versions.Count - 1) return;
        current++;
        UpdateContent();
    }
    public void NextVersion()
    {
        if (current == 0) return;
        current--;
        UpdateContent();
    }
}
