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
    }
    public static void SaveTo(string path)
    {
        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(stream))
            writer.Write(project);
    }
}
