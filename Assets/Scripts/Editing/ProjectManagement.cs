using System.IO;

public static class ProjectManagement
{
    public static string filePath = null;
    public static SongData project = new SongData();
    // Methods for I/O
    public static void LoadFrom(string path)
    {
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            project = reader.ReadSongData();
            filePath = path;
        }
        if (project.music != null)
            using (MemoryStream stream = new MemoryStream(project.music))
                AudioPlayer.Instance.LoadAudioFromStream(stream);
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
