using UnityEngine;
using Newtonsoft.Json;

public class ToolbarInitialization : MonoBehaviour
{
    public ToolbarInitialization Instance { get; private set; }
    public ToolbarSelectable projectSelectable;
    public ToolbarSelectable editSelectable;
    public ToolbarSelectable windowsSelectable;
    public ToolbarSelectable settingsSelectable;
    public ToolbarSelectable testSelectable;
    private void InitializeProjectSelectable()
    {
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "New project", "创建新项目" },
            operation = new Operation
            {
                callback = () =>
                {
                    FileExplorer.SetTagContent("New project", "创建新项目");
                    FileExplorer.SetDefaultFileName("NewProject.dnt");
                    FileExplorer.Open(FileExplorer.Mode.InputFileName,
                        () => { },
                        ".dnt");
                },
                shortcut = new Shortcut { key = KeyCode.N }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.N }
        });
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Open project", "打开项目" },
            operation = new Operation
            {
                callback = () =>
                {
                    ProjectManagement.LoadFrom(FileExplorer.Result);
                    ProjectProperties.Instance.Open();
                },
                shortcut = new Shortcut { key = KeyCode.O }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.O }
        });
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Save project", "保存项目" },
            operation = new Operation
            {
                callback = () => { throw new System.NotImplementedException(); },
                shortcut = new Shortcut { key = KeyCode.S }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.S }
        });
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Save as...", "另存为..." },
            operation = new Operation
            {
                callback = () => { throw new System.NotImplementedException(); },
                shortcut = new Shortcut { shift = true, key = KeyCode.S }
            },
            globalShortcut = new Shortcut { ctrl = true, shift = true, key = KeyCode.S }
        });
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Quit", "退出" },
            operation = new Operation
            {
                callback = QuitApp.ShowConfirmQuitMessage,
                shortcut = new Shortcut { key = KeyCode.Q }
            },
            globalShortcut = new Shortcut { alt = true, key = KeyCode.F4 }
        });
    }
    private void InitializeEditSelectable()
    {

    }
    private void InitializeWindowsSelectable()
    {
        windowsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Project properties window", "项目属性窗口" },
            operation = new Operation
            {
                callback = () =>
                {
                    if (ProjectProperties.Instance.Opened)
                        ProjectProperties.Instance.Close();
                    else
                        ProjectProperties.Instance.Open();
                },
                shortcut = new Shortcut { key = KeyCode.P }
            }
        });
    }
    private void InitializeSettingsSelectable()
    {
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Check for updates", "更新检测" },
            operation = new Operation
            {
                callback = () => { VersionChecker.CheckForUpdate(true); },
                shortcut = new Shortcut { key = KeyCode.U }
            }
        });
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Language selection", "语言选择" },
            operation = new Operation
            {
                callback = () =>
                {
                    MessageBox.Activate(new[] { "Language", "语言" },
                        new[] { "Change language to...", "语言修改为..." },
                        new MessageBox.ButtonInfo
                        {
                            callback = () => { LanguageController.Language = 0; },
                            texts = new[] { "English" }
                        },
                        new MessageBox.ButtonInfo
                        {
                            callback = () => { LanguageController.Language = 1; },
                            texts = new[] { "中文" }
                        });
                },
                shortcut = new Shortcut { key = KeyCode.L }
            }
        });
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Background image settings", "背景图设置" },
            operation = new Operation
            {
                callback = BackgroundImageSetter.Open,
                shortcut = new Shortcut { key = KeyCode.B }
            }
        });
    }
    private void InitializeTestSelectable()
    {
        testSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Test Json Chart Loading" },
            operation = new Operation
            {
                callback = () =>
                {
                    FileExplorer.SetTagContent("Test json chart loading");
                    FileExplorer.Open(FileExplorer.Mode.SelectFile,
                        () =>
                        {
                            string json = System.IO.File.ReadAllText(FileExplorer.Result);
                            Chart chart = new Chart { difficulty = 0, level = "1" };
                            JsonChart jsonChart = JsonConvert.DeserializeObject<JsonChart>(json);
                            chart.FromJsonChart(jsonChart);
                            return;
                        }, ".json", ".txt");
                },
                shortcut = new Shortcut { key = KeyCode.T }
            }
        });
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ToolbarInitialization");
        }
    }
    private void Start()
    {
        InitializeProjectSelectable();
        InitializeEditSelectable();
        InitializeWindowsSelectable();
        InitializeSettingsSelectable();
        InitializeTestSelectable();
    }
}
