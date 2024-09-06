using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public class UserConfigManager : MonoBehaviour
    {
        public UserConfig Config => _config ??= LoadConfig();

        public void SaveConfig()
        {
            File.WriteAllText(ConfigTempPath, JsonConvert.SerializeObject(Config));
            if (File.Exists(ConfigPath)) File.Delete(ConfigPath);
            File.Move(ConfigTempPath, ConfigPath);
        }

        private static string DataPath = Application.persistentDataPath;
        private static string ConfigPath => Path.Combine(DataPath, "config.json");
        private static string ConfigTempPath => Path.Combine(DataPath, "config-temp.json");
        [SerializeField] private TextAsset _defaultConfigJsonAsset = null!;
        private UserConfig? _config;

        private UserConfig LoadConfig()
        {
            Directory.CreateDirectory(DataPath);
            if (!File.Exists(ConfigPath)) return LoadDefaultConfig();
            var config = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText(ConfigPath));
            if (config is null) Debug.LogWarning("User config file is in an invalid format!");
            return config ?? LoadDefaultConfig();
        }

        private UserConfig LoadDefaultConfig()
        {
            var config = JsonConvert.DeserializeObject<UserConfig>(_defaultConfigJsonAsset.text);
            if (config is null) Debug.LogWarning("Failed to load default user config!");
            return config ?? new UserConfig();
        }
    }
}