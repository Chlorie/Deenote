using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ProjectController : MonoBehaviour
{
    //-Declaration-
    public static ProjectController Instance { get; private set; } 
    //-Panel control buttons-
    public GameObject filePanelButton;
    public GameObject infoPanelButton;
    public GameObject chartsPanelButton;
    public GameObject editPanelButton;
    public GameObject settingsPanelButton;
    //-Panels-
    public GameObject filePanel;
    public GameObject newProjectPanel;
    public GameObject infoPanel;
    public GameObject chartsPanel;
    public GameObject editPanel;
    public GameObject settingsPanel;
    //-Canvas-
    public GameObject aboutCanvas;
    //-File Panel-
    public LocalizedText songSelectButtonText;
    public LocalizedText fileSelectButtonText;
    public Button songSelectConfirmButton;
    //-Info Panel-
    public InputField infoProjectNameInputField;
    public InputField charterNameInputField;
    public Text songNameText;
    //-Charts Panel-
    public InputField[] lvlInputFields;
    //-Settings Panel-
    private float lastAutoSaveTime;
    private const float AutoSaveTime = 300.0f;
    public Dropdown autoSaveDropdown;
    public Toggle vSyncToggle;
    private enum AutoSaveState { Off, On, OnAndSaveJson }
    private AutoSaveState autoSaveState = AutoSaveState.On;
    private bool vSync = true;
    //-About the project-
    private AudioClip songAudioClip; // Audio clip of the song
    private byte[] audioFileBytes; // All bytes of the audio file
    private string projectFileName; // Name of the project file
    private string projectFolder; // Where the project file is at
    private FileInfo songFile; // Where the song is saved, only used when loading song for the 1st time
    public Project project; // The project itself
    //-Other scripts-
    public StageController stage;
    public ProjectSaveLoad projectSL;
    public DirectorySelectorController directorySelectorController;
    public FileOpener fileOpener;
    //-Flags-
    private bool clearStageNewProjectMode;
    //-Stage-
    private int currentInStage = -1;
    public Text stageUIProjectName;
    public Text stageUILvl;
    public Text stageUIScore;
    public TextMesh stageStaveProjectName;
    public Image stageUIDiff;
    private readonly string[] diffNames = { "Easy", "Normal", "Hard", "Extra" };
    public Color[] textColors = new Color[4];
    public Image timeSliderImage;
    //-Data from assets-
    public Sprite[] diffImage = new Sprite[4];
    //-Others-
    public GameObject leftBackgroundImage;
    public GameObject aboutWindow;
    public Text debugText;

    //UGUI
    public Dropdown ResolutionDropDown;
    
    public UnityEvent ResolutionChange
    {
        get => ScreenResolutionSelector.resolutionChange;
        set => ScreenResolutionSelector.resolutionChange = value;
    }
    public Dropdown languageDropdown;


    //-Functions-
    private void Awake()
    {
        if (!(Instance is null))
            throw new InvalidOperationException($"{nameof(ProjectController)} is not allowed existing more than one instance");
        Instance = this;

        ScreenResolutionSelector.InitializeResolutionDropdown(ResolutionDropDown);
    }

    //-Initialization-
    public void PanelSelectionInit()
    {
        filePanelButton.SetActive(true);
        infoPanelButton.SetActive(false);
        chartsPanelButton.SetActive(false);
        editPanelButton.SetActive(false);
        settingsPanelButton.SetActive(true);
        infoPanel.SetActive(false);
        chartsPanel.SetActive(false);
        editPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
    //-File Panel-
    public void NewProject() //Project - New
    {
        if (project != null)
        {
            stage.StopPlaying();
            stage.editor.pianoSoundEditor.Deactivate(false);
            MessageScreen.Activate(
                new[] { "Current project will be closed when you start a new project", "启动新项目时当前的项目会被关闭" },
                new[] { "<color=#ff5555>Make sure that you have SAVED your project!</color>", "<color=#ff5555>请确认你已经保存当前的项目文件!</color>" },
                new[] { "Start a new project now!", "启动新项目!" }, ClearStageStartNewProject,
                new[] { "Take me back to my project", "返回到当前项目" }, () => { });
            clearStageNewProjectMode = true;
            return;
        }
        songFile = null;
        projectFileName = null;
        projectFolder = null;
        songSelectButtonText.Color = new Color(25.0f / 64, 25.0f / 64, 25.0f / 64, 0.5f);
        fileSelectButtonText.Color = new Color(25.0f / 64, 25.0f / 64, 25.0f / 64, 0.5f);
        songSelectButtonText.SetStrings("Select the song file", "选择音乐文件");
        fileSelectButtonText.SetStrings("Create a new project file", "创建新工程文件");
        newProjectPanel.SetActive(true);
        filePanel.SetActive(false);
        CheckConfirmButton();
    }
    public void CloseFilePanel() //Close the file panel if new project panel is currently on
    {
        if (newProjectPanel.activeInHierarchy)
            filePanel.SetActive(false);
    }
    public void SelectSong() //Project - New - Select Song
    {
        string[] acceptedExtension = { ".wav", ".mp3" };
        directorySelectorController.ActivateSelection(acceptedExtension, SongSelected);
    }
    public void SelectFile() //Project - New - Select Folder
    {
        string[] acceptedExtension = { ".dsproj" };
        directorySelectorController.ActivateSelection(acceptedExtension, FileSelected, true);
    }
    
    private void SongSelected() //Called by directory selector controller when song selected
    {
        songFile = new FileInfo(directorySelectorController.selectedItemFullName);
        songSelectButtonText.Color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        songSelectButtonText.SetStrings(songFile.Name);
        directorySelectorController.DeactivateSelection();
        CheckConfirmButton();
    }
    private void FileSelected() //Called by directory selector controller when song selected
    {
        projectFolder = directorySelectorController.selectedItemFullName;
        projectFileName = directorySelectorController.fileName;
        fileSelectButtonText.Color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        fileSelectButtonText.SetStrings(projectFileName + ".dsproj");
        directorySelectorController.DeactivateSelection();
        CheckConfirmButton();
    }
    private void CheckConfirmButton() //Update the state of the confirm button
    {
        if (songFile != null && projectFileName != null && projectFolder != null)
            songSelectConfirmButton.interactable = true;
        else
            songSelectConfirmButton.interactable = false;
    }
    public void CancelCreateProject() //Project - New - Cancel
    {
        newProjectPanel.SetActive(false);
    }
    public void ClearStageStartNewProject() //Create/open a new project when a project in currently opened
    {
        CloseProject();
        if (clearStageNewProjectMode)
            NewProject();
        else
            LoadProject();
    }
    public void ConfirmButtonPressed() //Project - New - Confirm
    {
        project = new Project();
        audioFileBytes = File.ReadAllBytes(songFile.FullName);
        songAudioClip = AudioLoader.LoadFromBuffer(audioFileBytes, songFile.Extension);
        project.songName = songFile.Name;
        project.charts = new Chart[4];
        for (int i = 0; i < 4; i++)
            project.charts[i] = new Chart
            {
                difficulty = i,
                level = "1"
            };
        stage.musicSource.clip = songAudioClip;
        infoPanelButton.SetActive(true);
        chartsPanelButton.SetActive(true);
        newProjectPanel.SetActive(false);
        infoPanel.SetActive(true);
        currentInStage = -1;
        InfoInitialization();
        LvlInputFieldInit();
        if (songAudioClip == null)
            MessageScreen.Activate(
                new[] { "Failed to load audio", "读取音频失败" },
                new[] { "Please select another audio file", "请选择其他的音频文件" },
                new[] { "Load another audio file", "读取其他的音频文件" },
                ChangeMusicFile);
    }
    public void SaveProject() //Project - Save
    {
        SavePlayerPrefs();
        if (project == null) return;
        if (currentInStage != -1)
            foreach (Chart chart in project.charts)
                chart.beats = project.charts[currentInStage].beats;
        StartCoroutine(projectSL.SaveProjectIntoFile(project, audioFileBytes, projectFolder + projectFileName + ".dsproj"));
    }
    public void SaveAs() //Project - Save As
    {
        string[] acceptedExtension = { ".dsproj" };
        if (project != null)
            directorySelectorController.ActivateSelection(acceptedExtension, SaveAsFileSelected, true);
    }
    private void SaveAsFileSelected()
    {
        string asFolder = directorySelectorController.selectedItemFullName;
        string asFile = directorySelectorController.fileName;
        string asFileFullName = asFolder + asFile + ".dsproj";
        directorySelectorController.DeactivateSelection();
        if (project == null) return;
        if (currentInStage != -1)
            foreach (Chart chart in project.charts)
                chart.beats = project.charts[currentInStage].beats;
        projectSL.SaveProjectIntoFile(project, audioFileBytes, asFileFullName);
    }
    public void LoadProject() //Project - Open
    {
        if (project == null)
        {
            directorySelectorController.ActivateSelection(new[] { ".dsproj" }, ProjectToLoadSelected());
            return;
        }
        stage.StopPlaying();
        stage.editor.pianoSoundEditor.Deactivate(false);
        MessageScreen.Activate(
            new[] { "Current project will be closed when you start a new project", "启动新项目时当前的项目会被关闭" },
            new[] { "<color=#ff5555>Make sure that you have SAVED your project!</color>", "<color=#ff5555>请确认你已经保存当前的项目文件!</color>" },
            new[] { "Start a new project now!", "启动新项目!" }, ClearStageStartNewProject,
            new[] { "Take me back to my project", "返回到当前项目" }, () => { });
        clearStageNewProjectMode = false;
    }
    public IEnumerator ProjectToLoadSelected(string fileName = null)
    {
        string projectFullDir = fileName ?? directorySelectorController.selectedItemFullName;
        string audioType = null;
        FileInfo projectFile;
        directorySelectorController.DeactivateSelection();
        yield return StartCoroutine(projectSL.LoadProjectFromFile(res => project = res,
            res => audioFileBytes = res, res => audioType = res, projectFullDir));
        songAudioClip = AudioLoader.LoadFromBuffer(audioFileBytes, audioType);
        infoPanelButton.SetActive(true);
        chartsPanelButton.SetActive(true);
        songAudioClip?.LoadAudioData();
        stage.musicSource.clip = songAudioClip;
        editPanelButton.SetActive(false);
        projectFile = new FileInfo(projectFullDir);
        projectFileName = projectFile.Name.Remove(projectFile.Name.Length - 7, 7);
        projectFolder = projectFile.FullName.Remove(projectFile.FullName.Length - projectFile.Name.Length, projectFile.Name.Length);
        filePanel.SetActive(false);
        currentInStage = -1;
        InfoInitialization();
        LvlInputFieldInit();
        if (songAudioClip == null)
            MessageScreen.Activate(
                new[] { "Failed to load audio", "读取音频失败" },
                new[] { "Please select another audio file", "请选择其他的音频文件" },
                new[] { "Load another audio file", "读取其他的音频文件" },
                ChangeMusicFile);
        yield return new WaitForSeconds(3.0f);
        projectSL.loadCompleteText.SetActive(false);
    }
    public void CloseProject()
    {
        stage.ClearStage();
        project = null;
        stage.stageActivated = false;
        stage.editor.activated = false;
        leftBackgroundImage.SetActive(true);
        stage.editor.activated = false;
        PanelSelectionInit();
    }
    //-Info Panel-
    private void InfoInitialization() //After creating a project, initialize info panel
    {
        infoProjectNameInputField.text = project.name;
        songNameText.text = project.songName;
        charterNameInputField.text = project.chartMaker;
    }
    public void InfoProjectNameFinishedEditing()
    {
        project.name = infoProjectNameInputField.text;
        stageUIProjectName.text = project.name;
        stageStaveProjectName.text = project.name;
    }
    public void CharterNameFinishedEditing() => project.chartMaker = charterNameInputField.text;
    public void ChangeMusicFile()
    {
        string[] acceptedExtension = { ".wav", ".mp3" };
        directorySelectorController.ActivateSelection(acceptedExtension, NewSongSelected);
    }
    private void NewSongSelected()
    {
        songFile = new FileInfo(directorySelectorController.selectedItemFullName);
        songNameText.text = songFile.Name;
        project.songName = songNameText.text;
        directorySelectorController.DeactivateSelection();
        audioFileBytes = File.ReadAllBytes(songFile.FullName);
        songAudioClip = AudioLoader.LoadFromBuffer(audioFileBytes, songFile.Extension);
        if (songAudioClip == null)
        {
            MessageScreen.Activate(
                new[] { "Failed to load audio", "读取音频失败" },
                new[] { "Please select another audio file", "请选择其他的音频文件" },
                new[] { "Load another audio file", "读取其他的音频文件" },
                ChangeMusicFile);
            return;
        }
        stage.musicSource.clip = songAudioClip;
        stage.timeSlider.value = 0.0f;
        stage.timeSlider.maxValue = songAudioClip.length;
        stage.OnSliderValueChanged();
    }
    //-Chart Panel-
    private void LvlInputFieldInit()
    {
        for (int i = 0; i < 4; i++) lvlInputFields[i].text = project.charts[i].level;
    }
    public void NewLvl(int diff)
    {
        project.charts[diff].level = lvlInputFields[diff].text;
        if (currentInStage == diff) stageUILvl.text = diffNames[diff] + " Lv" + project.charts[diff].level;
    }
    public void JSONFileSelect(int diff)
    {
        string[] allowedExtension = { ".json", ".txt", ".cytus" };
        directorySelectorController.ActivateSelection(allowedExtension, delegate { ImportChart(diff); });
    }
    public void ImportChart(int diff)
    {
        if (directorySelectorController.selectedItemFullName.EndsWith(".cytus"))
            ImportChartFromCytusChart(diff);
        else
            ImportChartFromJSONFile(diff);
    }
    public void ImportChartFromJSONFile(int diff)
    {
        byte[] bytes = File.ReadAllBytes(directorySelectorController.selectedItemFullName); //JSON file bytes
        char[] bytechars = new char[bytes.Length + 1];
        int length = bytes.Length, i = 0;
        while (bytes[i] != 0x7B) i++;
        int offset = i;
        for (; i < length && bytes[i] != 0x00; i++) bytechars[i - offset] = (char)bytes[i];
        string str = new string(bytechars);
        JSONChart jchart = Utility.JSONtoJChart(str);
        string level = project.charts[diff].level;
        List<float> beats = project.charts[diff].beats;
        project.charts[diff] = Utility.JCharttoChart(jchart);
        project.charts[diff].level = level;
        project.charts[diff].beats = beats;
        directorySelectorController.DeactivateSelection();
    }
    public void ImportChartFromCytusChart(int diff)
    {
        string[] cychart = File.ReadAllLines(directorySelectorController.selectedItemFullName);
        JSONChart jchart = Utility.CytusChartToJChart(cychart);
        string level = project.charts[diff].level;
        project.charts[diff] = Utility.JCharttoChart(jchart);
        project.charts[diff].level = level;
        directorySelectorController.DeactivateSelection();
    }
    public void ExportAllJSONCharts(int diff)
    {
        if (project != null)
            JSONExportDirectorySelect(diff + 4);
        else
            MessageScreen.Activate(new[] { "No project file is opened!", "目前没有已经打开的项目文件!" },
                new[] { "<color=ff7f7f>What are you expecting to be exported???</color>",
                    "<color=ff7f7f>你认为这样能导出什么东西呢???</color>" },
                new[] { "Back", "返回" }, delegate { });
    }
    public void JSONExportDirectorySelect(int diff)
    {
        string[] allowedExtension = { ".json" };
        string[] difficultyStrings = { "easy", "normal", "hard", "extra" };
        if (project.charts[diff % 4].notes.Count <= 0) return;
        directorySelectorController.ActivateSelection(allowedExtension, () => ExportChartToJSONChart(diff), true);
        directorySelectorController.SetInitialFileName(projectFileName + "." + difficultyStrings[diff % 4]);
    }
    public void ExportChartToJSONChart(int diff)
    {
        FileStream fs = new FileStream(directorySelectorController.selectedItemFullName +
            directorySelectorController.fileName + ".json", FileMode.Create);
        directorySelectorController.DeactivateSelection();
        Utility.WriteCharttoJSON(project.charts[diff % 4], fs);
        fs.Close();
        if (diff >= 4 && diff < 7) ExportAllJSONCharts(diff % 4 + 1);
    }
    public void LoadToStage(int diff)
    {
        leftBackgroundImage.SetActive(false);
        stage.editor.activated = true;
        editPanelButton.SetActive(true);
        stageUIDiff.sprite = diffImage[diff];
        stageUIProjectName.text = project.name;
        stageUILvl.text = diffNames[diff] + " LV" + project.charts[diff].level;
        stageUILvl.color = textColors[diff];
        timeSliderImage.color = textColors[diff];
        stageUIScore.text = "0.00 %";
        stageStaveProjectName.text = project.name;
        stage.StopPlaying();
        stage.ClearStage();
        stage.InitializeStage(project, diff, this);
        if (currentInStage != -1)
            foreach (Chart chart in project.charts) chart.beats = project.charts[currentInStage].beats;
        currentInStage = diff;
    }
    //-Settings Panel-
    public void UpdateAutoSave(int state)
    {
        autoSaveState = (AutoSaveState)state;
        if (autoSaveState != AutoSaveState.Off) lastAutoSaveTime = Time.time;
    }
    public void ToggleVSync(bool on)
    {
        vSync = on;
        QualitySettings.vSyncCount = on ? 1 : 0;
    }
    public void OpenAbout()
    {
        CurrentState.ignoreAllInput = true;
        CurrentState.ignoreScroll = true;
        aboutWindow.SetActive(true);
        stage.StopPlaying();
    }
    public void CloseAbout()
    {
        CurrentState.ignoreAllInput = false;
        CurrentState.ignoreScroll = false;
        aboutWindow.SetActive(false);
    }
    //-Other-
    private void UpdateAboutCanvas()
    {
        aboutCanvas.SetActive(false);
        aboutCanvas.SetActive(true);
    }
    private void LoadPlayerPrefs()
    {
        autoSaveDropdown.value = PlayerPrefs.GetInt("Autosave", 0);
        UpdateAutoSave(autoSaveDropdown.value);
        vSyncToggle.isOn = Utility.PlayerPrefsGetBool("VSync On", vSyncToggle.isOn);
        ToggleVSync(vSyncToggle.isOn);
        languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
    }
    public void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("Autosave", (int)autoSaveState);
        Utility.PlayerPrefsSetBool("Light Effect", stage.lightEffectState);
        Utility.PlayerPrefsSetBool("Show FPS", stage.showFPS);
        Utility.PlayerPrefsSetBool("VSync On", vSync);
        PlayerPrefs.SetInt("Mouse Wheel Sensitivity", stage.mouseSens);
        PlayerPrefs.SetInt("Note Speed", stage.chartPlaySpeed);
        PlayerPrefs.SetInt("Music Speed", stage.musicPlaySpeed);
        PlayerPrefs.SetInt("Effect Volume", stage.effectVolume);
        PlayerPrefs.SetInt("Music Volume", stage.musicVolume);
        PlayerPrefs.SetInt("Piano Volume", stage.pianoVolume);
        Utility.PlayerPrefsSetBool("Show Link Line", stage.linkLineParent.gameObject.activeSelf);
        PlayerPrefs.SetInt("XGrid Count", stage.editor.xGrid);
        PlayerPrefs.SetFloat("XGrid Offset", stage.editor.xGridOffset);
        PlayerPrefs.SetInt("TGrid Count", stage.editor.tGrid);
        PlayerPrefs.SetInt("Language", LanguageSelector.Language);
        Utility.PlayerPrefsSetBool("Snap To Grid", stage.editor.snapToGrid);
        Utility.PlayerPrefsSetBool("Show Indicator", stage.editor.noteIndicatorsToggler.activeSelf);
        Utility.PlayerPrefsSetBool("Show Border", stage.editor.border.activeSelf);
    }

    public void SetScreenResolution(int section) => ScreenResolutionSelector.SetScreenResolution(section);

    public void SetLanguage(int language) => LanguageSelector.Language = language;

    private void Start()
    {
        LoadPlayerPrefs();
        Utility.debugText = debugText;
        project = null;
        PanelSelectionInit();
        UpdateAboutCanvas();
        Application.runInBackground = true;
        fileOpener.CheckCommandLine();
    }

    private void Shortcuts()
    {
        if (Utility.DetectKeys(KeyCode.S, Utility.CTRL)) //Ctrl+S
            SaveProject();
        if (Utility.DetectKeys(KeyCode.S, Utility.CTRL + Utility.SHIFT)) //Ctrl+Shift+S
            SaveAs();
        if (Utility.DetectKeys(KeyCode.N, Utility.CTRL)) //Ctrl+N
            NewProject();
        if (Utility.DetectKeys(KeyCode.O, Utility.CTRL)) //Ctrl+O
            LoadProject();
        if (Utility.DetectKeys(KeyCode.Q, Utility.CTRL)) //Ctrl+Q
        {
            if (stage.stageActivated)
            {
                stage.StopPlaying();
                stage.editor.pianoSoundEditor.Deactivate(false);
            }
            RightScrollViewController controller = FindObjectOfType<RightScrollViewController>();
            controller.OpenQuitScreen();
        }
        if (Utility.DetectKeys(KeyCode.E, Utility.CTRL)) //Ctrl+E
        {
            if (stage.stageActivated)
            {
                stage.StopPlaying();
                stage.editor.pianoSoundEditor.Deactivate(false);
            }
            ExportAllJSONCharts(0);
        }
    }
    private void Update()
    {
        // Autosave
        lastAutoSaveTime += Time.deltaTime;
        if (autoSaveState != AutoSaveState.Off && lastAutoSaveTime > AutoSaveTime)
        {
            lastAutoSaveTime -= AutoSaveTime;
            SaveProject();
            if (autoSaveState == AutoSaveState.OnAndSaveJson && project != null)
            {
                string timeString = DateTime.Now.ToString("yyMMddHHmmss");
                for (int i = 0; i < 4; i++)
                {
                    if (project.charts[i].notes.Count == 0) continue;
                    string fileName = projectFolder + timeString + diffNames[i] + ".json";
                    FileStream fs = new FileStream(fileName, FileMode.Create);
                    Utility.WriteCharttoJSON(project.charts[i], fs);
                    fs.Close();
                }
            }
        }
        Shortcuts();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void RunOnStart() =>
        Application.wantsToQuit += () =>
        {
            FindObjectOfType<RightScrollViewController>().OpenQuitScreen();
            return false;
        };
}
