using System.IO;

public static class ProjectManagement
{
    public static string filePath;
    public static SongData project = new SongData();
    // Start up
    public static void TryLoadFrom(string path)
    {
        if (!new FileInfo(path).Exists) return;
        MessageBox.Instance.Activate(new[] { "Load previous project", "读取之前的项目" },
            new[]
            {
                "Do you want to load the previous project opened in Deenote?",
                "是否要读取Deenote上一次打开的项目？"
            },
            new MessageBox.ButtonInfo { texts = new[] { "Yes", "是的" }, callback = () => { LoadFrom(path); } },
            new MessageBox.ButtonInfo { texts = new[] { "No", "不用了" } });
    }
    // Methods for I/O
    public static void LoadFrom(string path)
    {
        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                project = reader.ReadSongData();
                filePath = path;
            }
        }
        catch (IOException)
        {
            MessageBox.Instance.Activate(new[] { "Corrupted file", "文件已损坏" },
                new[]
                {
                    "The file is corrupted or in incorrect format. Deenote cannot load this file.",
                    "该文件已损坏，或出现了格式错误。Deenote无法读取该文件。"
                },
                new MessageBox.ButtonInfo { texts = new[] { "OK", "好的" } });
        }
        if (project.music == null) return;
        using (MemoryStream stream = new MemoryStream(project.music))
            AudioPlayer.Instance.LoadAudioFromStream(stream);
        AppConfig.config.openedFile = path;
        // Activate toolbar selections
        ToolbarInitialization instance = ToolbarInitialization.Instance;
        instance.projectSelectable.SetActive("Save project", true);
        instance.projectSelectable.SetActive("Save as...", true);
        instance.windowsSelectable.SetActive("Project properties", true);
        instance.windowsSelectable.SetActive("Perspective view", false);
    }
    public static void SaveAs(string path)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return;
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(stream))
            writer.Write(project);
    }
    public static void Save() => SaveAs(filePath);
}
