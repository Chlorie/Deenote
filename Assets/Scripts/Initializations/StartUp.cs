using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class StartUp : MonoBehaviour
{
    private void Start()
    {
        LanguageController.Language = 0;
        if (new FileInfo("config.json").Exists)
        {
            JsonSerializer serializer = new JsonSerializer();
            FileStream stream = new FileStream("config.json", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(stream);
            AppConfig.config = serializer.Deserialize(reader, typeof(AppConfig)) as AppConfig;
            reader.Close();
            reader.Dispose();
            stream.Close();
            stream.Dispose();
        }
    }
}
