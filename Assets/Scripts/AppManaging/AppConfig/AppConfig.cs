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
    public RectData fileExplorerRect;
    public RectData messageBoxRect;
    public RectData backgroundImageSetterRect;
    public RectData projectPropertiesRect;
    public RectData perspectiveViewRect;
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
        FileExplorer.Instance.Rect = fileExplorerRect.ToRect();
        MessageBox.Instance.Rect = messageBoxRect.ToRect();
        BackgroundImageSetter.Instance.Rect = backgroundImageSetterRect.ToRect();
        ProjectProperties.Instance.Rect = projectPropertiesRect.ToRect();
        PerspectiveView.Instance.Rect = perspectiveViewRect.ToRect();
    }
    private void UpdateConfig()
    {
        language = LanguageController.Language;
        firstLaunch = false;
        fileExplorerRect = new RectData(FileExplorer.Instance.Rect);
        messageBoxRect = new RectData(MessageBox.Instance.Rect);
        backgroundImageSetterRect = new RectData(BackgroundImageSetter.Instance.Rect);
        projectPropertiesRect = new RectData(ProjectProperties.Instance.Rect);
        perspectiveViewRect = new RectData(PerspectiveView.Instance.Rect);
    }
}
