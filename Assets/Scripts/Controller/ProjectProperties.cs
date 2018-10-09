using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ProjectProperties : Window
{
    public static ProjectProperties Instance { get; private set; }
    public new void Open()
    {
        UpdateProperties();
        base.Open();
        LanguageController.Refresh();
    }
    [SerializeField] private InputField _songNameInputField;
    [SerializeField] private InputField _artistInputField;
    [SerializeField] private InputField _noterInputField;
    [SerializeField] private InputField[] _levelInputFields;
    public void UpdateProperties()
    {
        _songNameInputField.text = ProjectManagement.project.songName;
        _artistInputField.text = ProjectManagement.project.artist;
        _noterInputField.text = ProjectManagement.project.noter;
        for (int i = 0; i < 4; i++) _levelInputFields[i].text = ProjectManagement.project.charts[i].level;
    }
    public void SongNameCallback()
    {
        ProjectManagement.project.songName = _songNameInputField.text;
        PerspectiveView.Instance.SetSongName(_songNameInputField.text);
    }
    public void ArtistCallback() => ProjectManagement.project.artist = _artistInputField.text;
    public void NoterCallback() => ProjectManagement.project.noter = _noterInputField.text;
    public void LoadAudioFileCallback()
    {
        FileExplorer.SetTagContent("Choose an audio file", "选择一个音频文件");
        FileExplorer.Instance.Open(FileExplorer.Mode.SelectFile, () =>
        {
            using (FileStream stream = File.OpenRead(FileExplorer.Result))
            {
                AudioPlayer.Instance.LoadAudioFromStream(stream);
                stream.Seek(0, SeekOrigin.Begin);
                long length = stream.Length;
                ProjectManagement.project.music = new byte[length];
                stream.Read(ProjectManagement.project.music, 0, (int)length);
            }
        }, ".mp3");
    }
    public void LevelInputCallback(int difficulty)
    {
        string level = _levelInputFields[difficulty].text;
        ProjectManagement.project.charts[difficulty].level = level;
        if (PerspectiveView.Instance.CurrentDifficulty == difficulty) PerspectiveView.Instance.SetDifficulty(difficulty, level);
    }
    public void LoadChartCallback(int difficulty)
    {
        AudioPlayer.Instance.Time = 0.0f;
        PerspectiveView.Instance.SetScore(0);
        PerspectiveView.Instance.SetSongName(ProjectManagement.project.songName);
        PerspectiveView.Instance.SetDifficulty(difficulty, ProjectManagement.project.charts[difficulty].level);
        // ToDo: Add orthogonal view things later
        ChartDisplayController.Instance.LoadChartFromProject(difficulty);
        ToolbarInitialization.Instance.windowsSelectable.SetActive("Perspective view", true);
    }
    public void ImportChartCallback(int difficulty)
    {
        FileExplorer.SetTagContent("Choose a chart file", "选择一个谱面文件");
        FileExplorer.Instance.Open(FileExplorer.Mode.SelectFile, () =>
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader reader = new StreamReader(FileExplorer.Result))
            {
                JsonChart chart = serializer.Deserialize(reader, typeof(JsonChart)) as JsonChart;
                ProjectManagement.project.charts[difficulty] = Chart.FromJsonChart(chart);
                if (ChartDisplayController.Instance.Difficulty == difficulty) LoadChartCallback(difficulty);
            }
        }, ".json", ".txt");
    }
    public void ExportChartCallback(int difficulty)
    {
        string fileName;
        switch (difficulty)
        {
            case 0: fileName = "easy"; break;
            case 1: fileName = "normal"; break;
            case 2: fileName = "hard"; break;
            case 3: fileName = "extra"; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(difficulty));
        }
        FileExplorer.SetTagContent("Choose where to save the file", "选择文件存储位置");
        FileExplorer.SetDefaultFileName(fileName + ".json");
        FileExplorer.Instance.Open(FileExplorer.Mode.InputFileName, () =>
        {
            JsonChart exportedChart = ProjectManagement.project.charts[difficulty].ToJsonChart();
            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter writer = new StreamWriter(FileExplorer.Result))
                serializer.Serialize(writer, exportedChart);
        }, ".json");
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ProjectProperties");
        }
    }
}
