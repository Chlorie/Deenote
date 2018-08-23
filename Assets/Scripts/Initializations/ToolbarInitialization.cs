using UnityEngine;

public class ToolbarInitialization : MonoBehaviour
{
    public static ToolbarInitialization Instance { get; private set; }
    public ToolbarSelectable projectSelectable;
    public ToolbarSelectable editSelectable;
    public ToolbarSelectable windowsSelectable;
    public ToolbarSelectable settingsSelectable;
    public ToolbarSelectable debugSelectable;
    private void InitializeProjectSelectable()
    {
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "New project", "创建新项目" },
            operation = new Operation
            {
                callback = CreateNewProject,
                shortcut = new Shortcut { key = KeyCode.N }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.N }
        }); // New project
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Open project", "打开项目" },
            operation = new Operation
            {
                callback = OpenExistingProject,
                shortcut = new Shortcut { key = KeyCode.O }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.O }
        }); // Open project
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Save project", "保存项目" },
            operation = new Operation
            {
                callback = ProjectManagement.Save,
                shortcut = new Shortcut { key = KeyCode.S }
            },
            globalShortcut = new Shortcut { ctrl = true, key = KeyCode.S },
            isActive = false
        }); // Save project
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Save as...", "另存为..." },
            operation = new Operation
            {
                callback = SaveProjectAs,
                shortcut = new Shortcut { shift = true, key = KeyCode.S }
            },
            globalShortcut = new Shortcut { ctrl = true, shift = true, key = KeyCode.S },
            isActive = false
        }); // Save as...
        projectSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Quit", "退出" },
            operation = new Operation
            {
                callback = QuitApp.ShowConfirmQuitMessage,
                shortcut = new Shortcut { key = KeyCode.Q }
            },
            globalShortcut = new Shortcut { alt = true, key = KeyCode.F4 }
        }); // Quit
    }
    private void CreateNewProject()
    {
        FileExplorer.SetTagContent("New project", "创建新项目");
        FileExplorer.SetDefaultFileName("NewProject.dnt");
        FileExplorer.Open(FileExplorer.Mode.InputFileName, () =>
        {
            ProjectManagement.filePath = FileExplorer.Result;
            ActivateProjectRelatedFunctions();
            ProjectProperties.Instance.Open();
        }, ".dnt");
    }
    private void OpenExistingProject()
    {
        FileExplorer.SetTagContent("Open project", "打开项目");
        FileExplorer.Open(FileExplorer.Mode.SelectFile, () =>
        {
            ProjectManagement.LoadFrom(FileExplorer.Result);
            ProjectProperties.Instance.UpdateProperties();
            ActivateProjectRelatedFunctions();
            ProjectProperties.Instance.Open();
        }, ".dnt");
    }
    private void SaveProjectAs()
    {
        FileExplorer.SetTagContent("Save as...", "另存为...");
        FileExplorer.SetDefaultFileName("NewProject.dnt");
        FileExplorer.Open(FileExplorer.Mode.InputFileName, () => ProjectManagement.SaveAs(FileExplorer.Result), ".dnt");
    }

    private void InitializeEditSelectable()
    {

    }

    private void InitializeWindowsSelectable()
    {
        windowsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Project properties", "项目属性" },
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
            },
            isActive = false
        }); // Project properties
        windowsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Perspective view", "透视视图" },
            operation = new Operation
            {
                callback = () =>
                {
                    if (PerspectiveView.Instance.Opened)
                        PerspectiveView.Instance.Close();
                    else
                        PerspectiveView.Instance.Open();
                },
                shortcut = new Shortcut { key = KeyCode.P, shift = true }
            },
            isActive = false
        }); // Perspective view
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
        }); // Check for updates
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
        }); // Language selection
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Background image settings", "背景图设置" },
            operation = new Operation
            {
                callback = BackgroundImageSetter.Open,
                shortcut = new Shortcut { key = KeyCode.B }
            }
        }); // Background image settings
    }

    private void InitializeDebugSelectable()
    {
        debugSelectable.operations.Add(new ToolbarOperation
        {
            strings = new[] { "Test Audio Playing" },
            operation = new Operation
            {
                callback = () =>
                {
                    if (AudioPlayer.Instance.IsPlaying)
                        AudioPlayer.Instance.Stop();
                    else
                        AudioPlayer.Instance.Play();
                },
                shortcut = new Shortcut { key = KeyCode.T }
            }
        }); // Test Audio Playing
    }

    private void ActivateProjectRelatedFunctions()
    {
        projectSelectable.SetActive("Save project", true);
        projectSelectable.SetActive("Save as...", true);
        windowsSelectable.SetActive("Project properties", true);
        windowsSelectable.SetActive("Perspective view", false);
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
        InitializeDebugSelectable();
    }
}
