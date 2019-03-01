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
            name = OperationName.NewProject,
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
            name = OperationName.OpenProject,
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
            name = OperationName.SaveProject,
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
            name = OperationName.SaveProjectAs,
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
            name = OperationName.QuitApp,
            strings = new[] { "Quit", "退出" },
            operation = new Operation
            {
                callback = QuitApp.ShowConfirmQuitMessage,
                shortcut = new Shortcut { key = KeyCode.Q }
            },
            globalShortcut = new Shortcut { alt = true, key = KeyCode.F4 }
        }); // Quit
    }
    private void SaveBeforeOpening(Callback callback)
    {
        if (EditTracker.Instance.Edited)
            MessageBox.Instance.Activate(new[] { "Unsaved changes", "更改未保存" },
                new[]
                {
                    "There are unsaved changes in this project. Would you like to save them before" +
                    "opening another project?",
                    "当前项目中有尚未保存的更改。是否保存？"
                },
                new MessageBox.ButtonInfo
                {
                    callback = () =>
                    {
                        ProjectManagement.Save();
                        callback();
                    },
                    texts = new[] { "Yes", "是的" }
                },
                new MessageBox.ButtonInfo
                {
                    callback = callback,
                    texts = new[] { "Don't save", "不保存" }
                },
                new MessageBox.ButtonInfo { texts = new[] { "Cancel", "取消" } });
        else
            callback();
    }
    private void CreateNewProject() =>
        SaveBeforeOpening(() =>
        {
            FileExplorer.SetTagContent("New project", "创建新项目");
            FileExplorer.SetDefaultFileName("NewProject.dnt");
            FileExplorer.Instance.Open(FileExplorer.Mode.InputFileName, () =>
            {
                ProjectManagement.filePath = FileExplorer.Result;
                ActivateProjectRelatedFunctions();
                ProjectProperties.Instance.Open();
            }, ".dnt");
        });
    private void OpenExistingProject() =>
        SaveBeforeOpening(() =>
        {
            FileExplorer.SetTagContent("Open project", "打开项目");
            FileExplorer.Instance.Open(FileExplorer.Mode.SelectFile, () =>
            {
                ProjectManagement.LoadFrom(FileExplorer.Result);
                ProjectProperties.Instance.UpdateProperties();
                ActivateProjectRelatedFunctions();
                ProjectProperties.Instance.Open();
            }, ".dnt");
        });
    private void SaveProjectAs()
    {
        FileExplorer.SetTagContent("Save as...", "另存为...");
        FileExplorer.SetDefaultFileName("NewProject.dnt");
        FileExplorer.Instance.Open(FileExplorer.Mode.InputFileName, () => ProjectManagement.SaveAs(FileExplorer.Result), ".dnt");
    }

    private void InitializeEditSelectable()
    {
        editSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.Undo,
            strings = new[] { "Undo", "撤销" },
            operation = new Operation
            {
                callback = EditTracker.Instance.Undo,
                shortcut = new Shortcut { key = KeyCode.U }
            },
            globalShortcut = new Shortcut { key = KeyCode.Z, ctrl = true },
            isActive = false
        }); // Undo
        editSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.Redo,
            strings = new[] { "Redo", "重做" },
            operation = new Operation
            {
                callback = EditTracker.Instance.Redo,
                shortcut = new Shortcut { key = KeyCode.R }
            },
            globalShortcut = new Shortcut { key = KeyCode.Y, ctrl = true },
            isActive = false
        }); // Redo
        editSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.Cut,
            strings = new[] { "Cut", "剪切" },
            operation = new Operation
            {
                callback = () =>
                {
                    // ToDo: Implement cut operation
                },
                shortcut = new Shortcut { key = KeyCode.T }
            },
            globalShortcut = new Shortcut { key = KeyCode.X, ctrl = true },
            isActive = false
        }); // Cut
        editSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.Copy,
            strings = new[] { "Copy", "复制" },
            operation = new Operation
            {
                callback = () =>
                {
                    // ToDo: Implement copy operation
                },
                shortcut = new Shortcut { key = KeyCode.C }
            },
            globalShortcut = new Shortcut { key = KeyCode.C, ctrl = true },
            isActive = false
        }); // Copy
        editSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.Paste,
            strings = new[] { "Paste", "粘贴" },
            operation = new Operation
            {
                callback = () =>
                {
                    // ToDo: Implement paste operation
                },
                shortcut = new Shortcut { key = KeyCode.P }
            },
            globalShortcut = new Shortcut { key = KeyCode.V, ctrl = true },
            isActive = false
        }); // Paste
    }

    private void InitializeWindowsSelectable()
    {
        windowsSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.ProjectPropertiesWindow,
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
            name = OperationName.PerspectiveViewWindow,
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
        windowsSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.PlayerSettingsWindow,
            strings = new[] { "Player settings", "播放设置" },
            operation = new Operation
            {
                callback = () =>
                {
                    if (PlayerSettings.Instance.Opened)
                        PlayerSettings.Instance.Close();
                    else
                        PlayerSettings.Instance.Open();
                },
                shortcut = new Shortcut { key = KeyCode.S }
            }
        }); // Player settings
    }

    private void InitializeSettingsSelectable()
    {
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.UpdateCheck,
            strings = new[] { "Check for updates", "更新检测" },
            operation = new Operation
            {
                callback = () => { VersionChecker.CheckForUpdate(true); },
                shortcut = new Shortcut { key = KeyCode.U }
            }
        }); // Check for updates
        settingsSelectable.operations.Add(new ToolbarOperation
        {
            name = OperationName.LanguageSelection,
            strings = new[] { "Language selection", "语言选择" },
            operation = new Operation
            {
                callback = () =>
                {
                    MessageBox.Instance.Activate(new[] { "Language", "语言" },
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
            name = OperationName.SelectBackgroundImage,
            strings = new[] { "Background image settings", "背景图设置" },
            operation = new Operation
            {
                callback = BackgroundImageSetter.Instance.Open,
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
        projectSelectable.SetActive(OperationName.SaveProject, true);
        projectSelectable.SetActive(OperationName.SaveProjectAs, true);
        windowsSelectable.SetActive(OperationName.ProjectPropertiesWindow, true);
        PerspectiveView.Instance.Close();
        windowsSelectable.SetActive(OperationName.PerspectiveViewWindow, false);
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
