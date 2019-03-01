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
        string originalName = ProjectManagement.project.songName;
        string currentName = _songNameInputField.text;
        ProjectManagement.project.songName = _songNameInputField.text;
        PerspectiveView.Instance.SetSongName(_songNameInputField.text);
        EditTracker.Instance.AddStep(new EditTracker.EditOperation
        {
            undo = () =>
            {
                _songNameInputField.text = originalName;
                ProjectManagement.project.songName = originalName;
                PerspectiveView.Instance.SetSongName(originalName);
            },
            redo = () =>
            {
                _songNameInputField.text = currentName;
                ProjectManagement.project.songName = currentName;
                PerspectiveView.Instance.SetSongName(currentName);
            }
        });
    }
    public void ArtistCallback()
    {
        string originalName = ProjectManagement.project.artist;
        string currentName = _artistInputField.text;
        ProjectManagement.project.artist = _artistInputField.text;
        EditTracker.Instance.AddStep(new EditTracker.EditOperation
        {
            undo = () =>
            {
                _artistInputField.text = originalName;
                ProjectManagement.project.artist = originalName;
            },
            redo = () =>
            {
                _artistInputField.text = currentName;
                ProjectManagement.project.artist = currentName;
            }
        });
    }
    public void NoterCallback()
    {
        string originalName = ProjectManagement.project.noter;
        string currentName = _noterInputField.text;
        ProjectManagement.project.noter = _noterInputField.text;
        EditTracker.Instance.AddStep(new EditTracker.EditOperation
        {
            undo = () =>
            {
                _noterInputField.text = originalName;
                ProjectManagement.project.noter = originalName;
            },
            redo = () =>
            {
                _noterInputField.text = currentName;
                ProjectManagement.project.noter = currentName;
            }
        });
    }
    public void LoadAudioFileCallback()
    {
        if (ProjectManagement.project.music != null)
            Utility.OperationCannotUndoMessage(() =>
            {
                FileExplorer.SetTagContent("Choose an audio file", "选择一个音频文件");
                FileExplorer.Instance.Open(FileExplorer.Mode.SelectFile, () =>
                {
                    EditTracker.Instance.Edited = true;
                    using (FileStream stream = File.OpenRead(FileExplorer.Result))
                    {
                        AudioPlayer.Instance.LoadAudioFromStream(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        long length = stream.Length;
                        ProjectManagement.project.music = new byte[length];
                        stream.Read(ProjectManagement.project.music, 0, (int)length);
                    }
                }, ".mp3");
            });
    }
    public void LevelInputCallback(int difficulty)
    {
        string originalLevel = ProjectManagement.project.charts[difficulty].level;
        string currentLevel = _levelInputFields[difficulty].text;
        ProjectManagement.project.charts[difficulty].level = currentLevel;
        if (PerspectiveView.Instance.CurrentDifficulty == difficulty)
            PerspectiveView.Instance.SetDifficulty(difficulty, currentLevel);
        EditTracker.Instance.AddStep(new EditTracker.EditOperation
        {
            undo = () =>
            {
                _levelInputFields[difficulty].text = originalLevel;
                ProjectManagement.project.charts[difficulty].level = originalLevel;
                if (PerspectiveView.Instance.CurrentDifficulty == difficulty)
                    PerspectiveView.Instance.SetDifficulty(difficulty, originalLevel);
            },
            redo = () =>
            {
                _levelInputFields[difficulty].text = currentLevel;
                ProjectManagement.project.charts[difficulty].level = currentLevel;
                if (PerspectiveView.Instance.CurrentDifficulty == difficulty)
                    PerspectiveView.Instance.SetDifficulty(difficulty, currentLevel);
            }
        });
    }
    public void LoadChartCallback(int difficulty)
    {
        if (ProjectManagement.project.music == null)
        {
            MessageBox.Instance.Activate(new[] { "Audio is empty", "音频为空" },
                new[]
                {
                    "You haven't imported any audio file into this project.\n" +
                    "Please select an audio file before editing the charts.",
                    "您尚未导入音频文件。请先选择音频文件再编辑谱面。"
                },
                new MessageBox.ButtonInfo { texts = new[] { "OK", "好的" } });
            return;
        }
        AudioPlayer.Instance.Time = 0.0f;
        PerspectiveView.Instance.SetScore(0);
        PerspectiveView.Instance.SetSongName(ProjectManagement.project.songName);
        PerspectiveView.Instance.SetDifficulty(difficulty, ProjectManagement.project.charts[difficulty].level);
        // ToDo: Add orthogonal view things later
        ChartDisplayController.Instance.LoadFromProject(difficulty);
        ToolbarInitialization.Instance.windowsSelectable.SetActive(OperationName.PerspectiveViewWindow, true);
    }
    public void ImportChartCallback(int difficulty)
    {
        if (!ProjectManagement.project.charts[difficulty].IsEmpty)
            Utility.OperationCannotUndoMessage(() =>
            {
                FileExplorer.SetTagContent("Choose a chart file", "选择一个谱面文件");
                FileExplorer.Instance.Open(FileExplorer.Mode.SelectFile, () =>
                {
                    EditTracker.Instance.Edited = true;
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader reader = new StreamReader(FileExplorer.Result))
                    {
                        JsonChart chart = serializer.Deserialize(reader, typeof(JsonChart)) as JsonChart;
                        ProjectManagement.project.charts[difficulty] = Chart.FromJsonChart(chart);
                        if (ChartDisplayController.Instance.Difficulty == difficulty) LoadChartCallback(difficulty);
                    }
                }, ".json", ".txt");
            });
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
