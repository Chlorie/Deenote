using System.IO;
using Newtonsoft.Json;

public class AppConfig
{
    public static AppConfig config = new AppConfig();
    public string openedFile = "";
    public string backgroundImage = "";
    public bool firstLaunch = true;
    public int language = 0;
    public int backgroundPosition = (int)BackgroundImageSetter.Position.Center;
    public int backgroundStretch = (int)BackgroundImageSetter.StretchMode.FitHeight;
    public static void Read()
    {
        if (new FileInfo("config.json").Exists)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader reader = new StreamReader("config.json"))
                config = serializer.Deserialize(reader, typeof(AppConfig)) as AppConfig;
        }
        config.ApplyConfig();
    }
    public static void Write()
    {
        config.UpdateConfig();
        JsonSerializer serializer = new JsonSerializer();
        using (StreamWriter writer = new StreamWriter("config.json"))
            serializer.Serialize(writer, config);
    }
    private void ApplyConfig()
    {
        LanguageController.Language = language;
        BackgroundImageSetter.Instance.SetBackgroundImage(backgroundImage);
        BackgroundImageSetter.Instance.SetPosition(backgroundPosition);
        BackgroundImageSetter.Instance.SetStretchMode(backgroundStretch);
    }
    private void UpdateConfig()
    {
        language = LanguageController.Language;
        firstLaunch = false;
    }
}
