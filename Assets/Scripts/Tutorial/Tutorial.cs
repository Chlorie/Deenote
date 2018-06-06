using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private StageController stage;
    private string[][] text = new string[][]
    {
        new string[] // Language 0 (English)
        {
            @"All the shortcuts:

-Project-

[Ctrl+N] Create a new project file
[Ctrl+O] Open a project file
[Ctrl+S] Save current project
[Ctrl+Shift+S] Save the project as another file
[Ctrl+E] Export all charts created in this project to JSON files
[Ctrl+Q] Quit the program

-Stage-

[Enter] Toggle play/stop state
[Space] Instant play (Play while holding space, stop and go back to where you started playing when you release space)
[Home] Jump to the start of the song
[End] Jump to the end of the song
[Ctrl+Up/Down] Adjust note falling speed
[Alt+Up/Down] Adjust music playing speed
[Up/Down or Mouse wheel scroll] Scroll the chart backward/forward
[Shift+Up/Down] Scroll the chart faster

-Editor-

[Ctrl+Z] Undo (Max undo steps: 100)
[Ctrl+Shift+Z] Redo
[Delete] Delete select notes
[G] Toggle whether or not to snap to grid
[Ctrl+A] Select all notes
[L] Link selected notes (Change to slide notes)
[U] Unlink selected notes (Change back to click notes)
[Q] Quantize selected notes to the grids
[A/D] Adjust selected notes' position values left/right by 0.01
[W/S] Adjust selected notes' time values up/down by 1ms
[Shift+W/A/S/D] Adjust selected notes' position/time by 0.1/10ms
[Z/X] Adjust selected notes' size values by -0.01/0.01
[Shift+Z/X] Adjust selected notes' size values by -0.1/0.1
[M] Reflect selected notes across the middle line (position = 0 line)
[Ctrl+X] Cut selected notes
[Ctrl+C] Copy selected notes
[Ctrl+V] Paste notes from the clipboard
[I] Form a curve passing selected notes (natural cubic spline interpolation)
[Shift+I] Form a curve passing selected notes (linear interpolation)
[Ctrl+I] Delete the curve
[F] Fill notes on the curve

<color=#800000ff>Full English tutorial coming soon</color>
"
        },
        new string[] // Language 1 (Chinese)
        {
            "感谢您使用Deemo谱面编辑器Deenote！我输了，最后还是做了汉化。不过这个教程部" +
            "分我还是会写得尽量详细并且简单易懂。另外，欢迎各位提供好的想法来进一步优化写谱体验。谢谢大家！\n在下面的教程中我会为您详细介绍使用本软件写谱" +
            "的方法。信息量可能会很大，所以这篇教程会比较长。请大家准备好<color=#00000040>零食和饮料(划掉)</color>笔和纸，教程就要开始了！\n\n\n",
            "1. 项目文件板块\n" +
            "打开程序以后，可以看到右边的操作区中有两个按钮(Project/Settings)，点击按钮可以展开/关闭对应的板块。我们首先来讲项目文件板块，可以" +
            "通过点击[Project]按钮打开此板块。在该板块下可以看到六个按钮，下面为您一一介绍这些按钮的功能。\n\n",
            "1.1 创建新的项目文件\n" +
            "要使用Deenote制作谱面，您需要先创建一个项目文件(.dsproj)。点击[New]按钮（或者使用快捷键Ctrl+N）可以创建新的工程文件。创建新项目" +
            "时，点击[File Name]下的按钮来选择保存位置并且输入新项目的文件名，点击[Song]下面的按钮来选择要使用的音乐文件，Deenote目前支持MP3" +
            "，WAV，以及OGG格式的音乐文件。选择好以后，点击[Confirm]确定创建项目。或者，按[Cancel]可以取消创建新项目。\n\n",
            "1.2 打开已有的项目文件\n" +
            "点击[Open]按钮（或者快捷键Ctrl+O）来打开已有的项目文件。\n\n",
            "1.3 导出已打开项目的所有谱面\n" +
            "点击[Export]按钮（或快捷键Ctrl+E）可以导出已打开项目中所有已经存在的谱面文件。导出格式为.json。\n\n",
            "1.4 保存项目\n" +
            "点击[Save]按钮（或快捷键Ctrl+S）保存所有对目前项目文件做出的更改。\n\n",
            "1.5 另存为\n" +
            "点击[Save As]按钮（或快捷键Ctrl+Shift+S）可以将目前正在编辑的项目文件另存到其他文件中。\n\n" +
            "1.6 退出\n" +
            "点击[Quit]按钮（或快捷键Ctrl+Q）退出该程序。\n\n\n",
            "2. 项目信息板块\n" +
            "在创建新项目或打开已有项目以后，操作区中会出现新的[Info]板块。该板块中包含着项目的基本信息，包括项目名称（Project Name，这也是编辑界面" +
            "中会出现的曲名），制谱者(Charter Name)，以及歌曲文件名(Song)。制作谱面过程中有时会对多个不同的音乐文件做出参考（比如一些带人声的" +
            "歌曲为了方便采音可能会对原音乐/纯伴奏音乐两个不同的音乐文件进行参考），可以点击[Song]下面的按钮替换音乐文件。\n\n\n",
            "3. 谱面选择板块\n" +
            "在创建新项目或打开已有项目以后，操作区中会出现新的[Chart]板块。点击[Load to stage]按钮可以在编辑区域中打开对应的谱面；点击[Export]" +
            "按钮可以单独导出该难度的谱面到JSON文件中；点击[Import]按钮可以从JSON文件导入该难度谱面信息。\n\n\n",
            "4. 谱面编辑区域以及编辑板块\n" +
            "依照前面所说的方法将谱面[Load to stage]以后，屏幕左侧会出现类似Deemo游戏界面的一片区域，这就是谱面编辑区域。同时，右侧操作区中也会出现" +
            "新的[Edit]板块，这便是编辑板块。对于谱面编辑的大多数操作都与它们有关。\n\n",
            "4.1 谱面编辑区域\n\n" +
            "4.1.1 谱面进度调整\n" +
            "编辑区域上方的进度条可以拖动，拖动这个进度条可以调整音乐播放/谱面的进度。此外，还有很多的快捷方式可以调节音乐/谱面的进度。按上/下方向键或" +
            "向前/后滚动鼠标滚轮可以向前/后调整音乐播放进度；在按住上/下键的同时按住Shift可以更快地滚动谱面；按Home键可以跳转到音乐的开头；按End键" +
            "可以跳转到音乐的结尾。\n\n" +
            "4.1.2 播放音乐/预览谱面\n" +
            "点击分数显示处下方的休止符按钮可以播放/暂停音乐。同样，音乐播放也有快捷键。按回车键可以起到与休止符按钮同样的效果；按住空格键同样可以播放" +
            "音乐，但松开空格键时音乐进度会回到按空格键之前的位置（在某些节奏难以分辨的地方可以使用这种方法更方便地进行反复调整）。\n\n" +
            "4.1.3 谱面编辑\n" +
            "按住鼠标左键并拖动可以框选note，按住Ctrl框选可以多选；单击鼠标右键可以在鼠标所在位置放置note，重叠的click note会显示红色。\n\n",
            "4.2 编辑板块\n" +
            "编辑板块中的操作很多，所以这个板块中又分了六个小板块。点击这些小板块右面的[Expand/Collapse]按钮以展开/关闭该板块。" +
            "下面对这些小板块中的操作进行讲解。\n\n" +
            "4.2.1 基本操作(Basic Commands)\n" +
            "无论是什么文件的编辑器，基本的操作总是少不了的。这个板块中的操作分别是：[Cut]剪切，快捷键Ctrl+X；[Copy]复制，快捷键Ctrl+C；[Paste]" +
            "粘贴，快捷键Ctrl+V，另外在粘贴模式下按住Shift键可以固定粘贴的横向位置；[Undo]撤销，快捷键Ctrl+Z，现在的最大撤销次数是100次；[Redo]" +
            "重做，快捷键Ctrl+Shift+Z或Ctrl+Y；[Link]将选中的" +
            "note连成一串滑键（黄条），快捷键L；[Unlink]将选中的note分离成普通的点击键，快捷键U；[Quantize]将选中的note量化到格线上，快捷键Q；" +
            "[Mirror]将选中的note以中线(position=0)为对称轴做镜像翻转，快捷键M；按Z和X键可以按0.01为单位调整选中note大小，Shift+Z或X可以以" +
            "0.1为单位调整\n\n" +
            "4.2.2 选中note信息(Selected ... note(s))\n" +
            "这一部分显示被选中的note的基本信息，分别是：Note ID，note编号，修改此项可选中指定的单个note；" +
            "Position，note横向位置，显示在界面上的note横向位置都在-2到2之间，包含两端，另外有只播放钢琴音" +
            "而游玩时不显示的note，这些note的横向位置超出这个范围，目前本软件暂不支持这种类型note的编辑，不过可能会在以后的更新中添加对这些note的支持；" +
            "Time，时间，即为note落到判定线上的时间；Size，note大小，一般大小为1，本程序设定最大为5最小为0.1，不过为了观赏/游玩体验，建议最大不要超过2" +
            "最小不要小于0.5；Shift，这个值所有人都不知道是干什么用的所以还是不要动了吧；Is link note，若选中的note是滑键则此项为True，不是滑键则" +
            "为False；Piano sounds，钢琴note附带的钢琴音信息。其中，若对于某一项选中的note有不同的多个值，则该项显示为Several Values；若没有选择" +
            "任何note，则所有项显示为Not Available。这些项中，note横向位置、时间、note大小、Shift值和钢琴音可以点击修改，若要对钢琴音进行修改，点击" +
            "[Click to view]则会打开钢琴界面。\n" +
            "4.2.2.1 钢琴界面\n" +
            "在选中note信息板块中点击钢琴音信息编辑的[Click to view]字样会打开钢琴界面。该界面出现在屏幕的下方，分为左右两个部分。左部显示当前编辑的" +
            "note的钢琴音信息，右边的是128键钢琴窗口。点击左面的[Save]按钮可以将现在的钢琴信息保存到已经选中的note中并关闭钢琴窗，[Play]按钮可以试听" +
            "目前的钢琴音，[Back]按钮直接关闭钢琴窗而不保存目前的钢琴音信息。左面下方为各个钢琴音信息的显示区，每一个钢琴音有四个属性，分别是Duration，" +
            "音长；Delay，延迟，即从note判定到该声音响起经过的延迟时间；Pitch，音调，该钢琴音的音调；Volume，该钢琴音的音量（强度），最大值是127。" +
            "除了音调以外的三个值都可以点击修改，并且点击右下角的[Delete]按钮可以删除这一条钢琴音。点击右面钢琴的琴键可以添加一个钢琴音，音调为对应" +
            "琴键的音调，而剩下的三个值默认都是0。\n\n" +
            "4.2.3 播放预览设置(Player Settings)\n" +
            "这一板块包含谱面预览/音乐播放的基本设置。分别为：Note speed，note下落速度，与游戏中一样为0.5到9.5的半整数，可以通过左右两个箭头按钮调节，" +
            "也可以使用快捷键Ctrl+上/下方向键来调节；Music speed，音乐播放速度，调节范围为0.1到3.0的一位小数，可以通过左右的箭头按钮调节也可以使用" +
            "快捷键Alt+上/下方向键调节；Effect/Music/Piano volume，分别为打击效果音/音乐/钢琴音的音量，范围是0到100的整数，可以直接点击数字输入" +
            "也可以拖动右面的滑杆进行调节；Show links，显示滑键连线，选择此项后同一组的滑键间会显示黄色的连线。\n\n" +
            "4.2.4 note摆放与格线(Note Placement & Grid)\n" +
            "该板块包含有关note摆放和格线显示的设置。分别为：Vertical grid，竖直格线数量，以中线（0横向位置）为对称轴在横向区域上均匀地画出线，下方的" +
            "Offset（偏移）即为对称轴的偏移量，可调范围为-1到1，可以通过直接输入数字和滑动滑杆两种方式进行调节（例：早期官方谱面中note的横向位置大多" +
            "落在-2，-1.75，-1.5……1.75，2这九条线上，在本程序中若想让竖直格线在这些线上，可以设置竖直格线数量为8，偏移量为0.25）；Horizontal grid" +
            "横向格线数量，设置后会将每一拍平均分成对应数目份（比如需要16分note的格线，这项应设置为4，因为一般情况下16分note是一拍的四分之一，同理需要" +
            "16分三连音/俗称24分note格线时，这项要设置为6）；Snap to grid，对齐格线，此项开启时放置note会自动吸附到最近的格线上，可以使用快捷键G来" +
            "切换这一项的状态；Show note indicator，显示note指示，开启此项后，鼠标在谱面编辑区中的时候会显示淡化的note来指示note会摆放到的位置；Show" +
            " border，显示边界，开启此项后谱面编辑区域中会显示两条粗线表示-2和2的横向边界。\n\n" +
            "4.2.5 曲线生成(Curve Forming)\n" +
            "该板块包含生成曲线以及快速批量填充note功能。点击[Cubic]按钮（或快捷键I）可以作出过选定note的自然三次样条插值曲线，点击[Linear]按钮（或快" +
            "捷键Shift+I）可以作出过选定note的线性插值曲线，在生成曲线以后，除首尾以外的中间所有note会被自动删除。通俗点讲，就是Cubic生成的是一条过指" +
            "定note的光滑曲线，而Linear生成的是通过这些note的折线段。Fill amount为填充数量，[Fill Notes]按钮（或快捷键F）在已生成的曲线上均匀放置" +
            "上面设置的填充数量个note。在曲线开启时，对齐到格线功能会把note对齐到曲线而不是格线。点击[Disable]或使用快捷键Ctrl+I关闭曲线。\n\n" +
            "4.2.6 拍线生成(Beat Lines)\n" +
            "该板块包含有关拍线生成的功能。[BPM Calculator]是一个手动的BPM测试器，打开后先点击[Play]按钮播放，再手动在每一拍敲一次空格，程序就会计算" +
            "出粗略的BPM；按[Pause]按钮可以暂停音乐；按[Reset]按钮可以重置计数器；按[Create Beat Grids]可以自动填充测试出的BPM信息并关闭BPM测试" +
            "窗口；点击[Back]按钮或者按Esc键可以直接退出BPM测试界面。注意，手动测试BPM终究不准确，仅可作为粗略的参考。拍线生成板块的[Fill]按钮会在谱面" +
            "中以Start的数值为起点，以BPM中数值为速度填充拍线，直到End数值为止。输入End数值时若数值太大超过乐曲总长，该数值会自动被转换成乐曲总长。另外" +
            "，在选中note的时候，这些值会被自动改成Start为选中的第一个note的时间，End为选中的最后一个note的时间，BPM为使一拍长度为Start到End时间差的值" +
            "。（例：某乐曲测得BPM为180，第一拍在0.200秒处，那么可以填充Start=0.200，BPM=180.000，End填充很大的数字，会被自动转换为乐曲长，这样点击" +
            "Fill即可完成格线的填充）（再举例：某乐曲测得第一拍在0.200秒处，前四拍BPM为180，之后的BPM为170，那么先按之前的方法从0.2秒处到乐曲结尾填充" +
            "BPM为180的拍线，再在四拍后的位置放置一个note，选中这个note，从这个note的时间开始以BPM170填充拍线到乐曲末尾，即可完成拍线的填充）。\n\n" +
            "4.2.7 谱面连接(Chart Concatenation)\n" +
            "为了使组曲制作方便，本程序提供谱面连接功能。导入一个谱面之后，可以继续读取其他的谱面，并且以特定的offset（即该曲的开始时间）和给定的" +
            "速度（比如1.1倍速等）将其他的谱面连接到当前谱面之后。\n\n",
            "5. 设置板块\n" +
            "设置板块即为[Settings]板块，其中包含程序的基本设置以及其他杂项，分别是：[About]关于本程序的一些信息，以及本人的一些碎碎念（雾）；[Update" +
            " History]版本更新历史，里面记录了每次更新的内容，版本更新历史窗口左下角的[Check Update]按钮可以用来检查程序是否是最新版，另外在每次打开" +
            "程序时程序也会自动做版本检查，若有新版本发布，会弹出窗口提示，此时点击[Go to release page]转到发布页面，点击[Go to download page]转到" +
            "下载页面，点击[Update later]暂时忽略更新；[Tutorial]即为这个教程；Autosave为自动保存，选中此项后每隔五分钟程序会自动保存项目文件；" +
            "Light effect为编辑区域背景的光效开关；Show FPS为帧率显示开关；VSync On为垂直同步开关；Mouse Wheel Sensitivity为鼠标滚轮灵敏度，" +
            "默认值为10，可以在1（十分之一灵敏度）到100（十倍灵敏度）间调整；Resolution可选择不同的窗口大小，推荐1280x720，更小窗口可能会使得文字" +
            "较小，造成阅读上的困难。\n\n\n",
            "<color=#800000ff>这个教程到这里就结束了，希望您可以用Deenote制作出令您满意的优秀谱面！</color>"
        }
    };
    public GameObject leftPic;
    public GameObject rightPanel;
    public GameObject tutorialPanel;
    public GameObject viewport;
    public LocalizedText tutorialText;
    public void OpenTutorial()
    {
        CurrentState.ignoreAllInput = CurrentState.ignoreScroll = true;
        leftPic.SetActive(true);
        rightPanel.SetActive(false);
        tutorialPanel.SetActive(true);
        stage.StopPlaying();
    }
    public void CloseTutorial()
    {
        CurrentState.ignoreAllInput = CurrentState.ignoreScroll = false;
        leftPic.SetActive(!stage.stageActivated);
        rightPanel.SetActive(true);
        tutorialPanel.SetActive(false);
    }
    public void InitializeTutorialLanguage()
    {
        string[] localizedTexts = new string[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            StringBuilder finalText = new StringBuilder();
            foreach (string str in text[i]) finalText.Append(str);
            localizedTexts[i] = finalText.ToString();
        }
        tutorialText.SetStrings(localizedTexts);
        viewport.SetActive(false);
        viewport.SetActive(true);
    }
    private void Start()
    {
        stage = FindObjectOfType<StageController>();
        InitializeTutorialLanguage();
    }
}
